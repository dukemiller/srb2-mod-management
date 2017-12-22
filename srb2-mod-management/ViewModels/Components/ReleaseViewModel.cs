using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using srb2_mod_management.Models;
using srb2_mod_management.Repositories;
using srb2_mod_management.Repositories.Interface;
using srb2_mod_management.Services.Interface;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System.Text.RegularExpressions;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;

namespace srb2_mod_management.ViewModels.Components
{
    public class ReleaseViewModel : ViewModelBase
    {
        private static readonly WebClient Downloader = new WebClient();
        private readonly IModRetreiverService _modService;
        private readonly IDownloadedModsRepository _downloadedMods;
        private static string ImageFolder => Path.Combine(SettingsRepository.ApplicationDirectory, "images");
        private DiscoverModel _model;
        private Release _release;
        private Mod _mod;
        private string _image;
        private string _status;
        private int _index;
        private bool _loadingImage;
        private bool _downloading;
        private bool _downloaded;
        private bool _available;

        // 

        public ReleaseViewModel(IModRetreiverService modService, IDownloadedModsRepository downloadedMods)
        {
            _modService = modService;
            _downloadedMods = downloadedMods;

            DownloadCommand = new RelayCommand(Download, CanDownload);
            RefreshCommand = new RelayCommand(() => _modService.UpdateRelease(_model.ReleaseInfo));
            PreviousImageCommand = new RelayCommand(PreviousImage, () => !LoadingImage);
            NextImageCommand = new RelayCommand(NextImage, () => !LoadingImage);
            WebpageCommand = new RelayCommand(() => Process.Start(_model.ReleaseInfo.Url));
        }

        // 

        public async Task<ReleaseViewModel> SetModel(DiscoverModel model)
        {
            // Models

            _model = model;
            Release = await _modService.RetrieveRelease(_model.ReleaseInfo);
            Downloaded = _downloadedMods.Contains(_model.Category, Release);

            // Set download information

            if (Downloaded)
            {
                Mod = _downloadedMods.Find(model.Category, model.ReleaseInfo);
                foreach (var file in Mod.Files)
                {
                    file.PropertyChanged -= FileOnPropertyChanged;
                    file.PropertyChanged += FileOnPropertyChanged;
                }
            }
            
            if (Retreivable())
            {
                Status = "Available to use.";
                Available = true;
            }

            else
            {
                Status = "This cannot be automatically implemented. " +
                         "To use, download and set up manually.";
                Available = false;
            }

            Index = 0;
            Image = "";

            if (model.Refresh)
                await _modService.UpdateRelease(_model.ReleaseInfo);

            return this;
        }

        /// <summary>
        ///     If the mod is suitable to be retrieved
        /// </summary>
        private bool Retreivable()
        {
            var containsFilteredWord = new[] {"md2", "launcher"}.Any(word => Release.Name.ToLower().Contains(word));
            var containsFilteredChangedThing = new[] {"Models", "Additional Software"}.Any(changedThing => Release.ChangedThings.Contains(changedThing)) && !Release.Name.Contains("ArchPack");
            return !containsFilteredWord && !containsFilteredChangedThing;
        }

        /// <summary>
        ///     Save on any file change property
        /// </summary>
        private async void FileOnPropertyChanged(object sender, PropertyChangedEventArgs args) =>
            await _downloadedMods.Save();

        /// <summary>
        ///     Only set the image on the visibility being changed to visible
        /// </summary>
        public async Task VisibleChanged(bool visible)
        {
            if (visible)
                await SetImage(Release.Screenshots[Index], Index);
        }

        // 

        /// <summary>
        ///     The index for the screenshots
        /// </summary>
        public int Index
        {
            get => _index;
            set => Set(() => Index, ref _index, value);
        }

        /// <summary>
        ///     The path to the current screenshot
        /// </summary>
        public string Image
        {
            get => _image;
            set => Set(() => Image, ref _image, value);
        }

        /// <summary>
        ///     The corresponding release model to the mod
        /// </summary>
        public Release Release
        {
            get => _release;
            set => Set(() => Release, ref _release, value);
        }

        /// <summary>
        ///     If currently loading an image
        /// </summary>
        public bool LoadingImage
        {
            get => _loadingImage;
            set
            {
                _loadingImage = value;
                NextImageCommand.RaiseCanExecuteChanged();
                PreviousImageCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        ///     If currently in the process of downloading the mod
        /// </summary>
        public bool Downloading
        {
            get => _downloading;
            set
            {
                Set(() => Downloading, ref _downloading, value);
                DownloadCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Whether or not the mod is downloaded
        /// </summary>
        public bool Downloaded
        {
            get => _downloaded;
            set => Set(() => Downloaded, ref _downloaded, value);
        }

        /// <summary>
        ///     If the mod is permissed to be downloadable
        /// </summary>
        public bool Available
        {
            get => _available;
            set => Set(() => Available, ref _available,  value);
        }

        /// <summary>
        ///     The status text replacing the download button
        /// </summary>
        public string Status
        {
            get => _status;
            set => Set(() => Status, ref _status, value);
        }

        /// <summary>
        ///     The associated mod itself
        /// </summary>
        public Mod Mod
        {
            get => _mod;
            set => Set(() => Mod, ref _mod, value);
        }

        public RelayCommand RefreshCommand { get; set; }

        public RelayCommand DownloadCommand { get; set; }

        public RelayCommand WebpageCommand { get; set; }

        public RelayCommand PreviousImageCommand { get; set; }

        public RelayCommand NextImageCommand { get; set; }

        /// <summary>
        ///     The current position in the total count of screenshots
        /// </summary>
        public string Progress => $"{Index + 1}/{Release.Screenshots.Count}";

        /// <summary>
        ///     The text for the download button
        /// </summary>
        public string DownloadText => Downloading ? "Downloading ..." : "Download";

        // 

        /// <summary>
        ///     Change the image back a position
        /// </summary>
        private async void PreviousImage()
        {
            Index = Index - 1 < 0 ? Release.Screenshots.Count - 1 : Index - 1;
            await SetImage(Release.Screenshots[Index], Index);
        }

        /// <summary>
        ///     Change the image up a position
        /// </summary>
        private async void NextImage()
        {
            Index = (Index + 1) % Release.Screenshots.Count;
            await SetImage(Release.Screenshots[Index], Index);
        }

        /// <summary>
        ///     Gather the files filename given a url
        /// </summary>
        private static async Task<string> GetFilename(string url)
        {
            if (!url.ToLower().Contains("srb2.org"))
            {
                var uri = new Uri(url);
                return Path.GetFileName(uri.LocalPath);
            }

            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Timeout = 3000;
            request.AllowAutoRedirect = false;
            request.Method = "HEAD";
            var response = (HttpWebResponse) await request.GetResponseAsync();
            var disposition = Uri.UnescapeDataString(response.Headers["content-disposition"]);
            var filename = disposition.Split(new[] {"filename=\"", "filename*=UTF-8''"},
                StringSplitOptions.None)[1].Split('"')[0];
            response.Close();
            return filename;
        }

        /// <summary>
        ///     Set the image given the index
        /// </summary>
        private async Task SetImage(string image, int index)
        {
            if (!File.Exists(image))
            {
                LoadingImage = true;

                if (!Directory.Exists(ImageFolder))
                    Directory.CreateDirectory(ImageFolder);

                var releaseFolder = Path.Combine(ImageFolder, Release.Id.ToString());

                if (!Directory.Exists(releaseFolder))
                    Directory.CreateDirectory(releaseFolder);

                try
                {
                    var downloadPath = Path.Combine(releaseFolder, await GetFilename(image));
                    if (!File.Exists(downloadPath))
                        await Downloader.DownloadFileTaskAsync(image, downloadPath);
                    Release.Screenshots[index] = downloadPath;
                    await _modService.ReplaceRelease(Release);
                }

                catch
                {
                }


                //
            }

            LoadingImage = false;
            Image = Release.Screenshots[Index];
            RaisePropertyChanged(nameof(Progress));
        }

        /// <summary>
        ///     Attempt to download the current mod
        /// </summary>
        private async void Download()
        {
            Downloading = true;
            RaisePropertyChanged(nameof(DownloadText));

            // Create necessary paths

            var path = Path.Combine(SettingsRepository.ApplicationDirectory, "mods",
                _model.Category.ToString().ToLower());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Download and extract

            var downloaded = new List<string>();

            var extractedFiles = new List<string>();

            var downloadPath = "";

            try
            {
                foreach (var download in Release.Downloads)
                {
                    downloadPath = Path.Combine(path, download.Filename);
                    if (!File.Exists(downloadPath))
                        await Downloader.DownloadFileTaskAsync(download.Link, downloadPath);
                    downloaded.Add(downloadPath);
                }
            }

            catch (WebException)
            {
                // Remove the offending file
                File.Delete(downloadPath);

                // Display error
                var dialog = SimpleIoc.Default.GetInstance<IDialogCoordinator>();
                await dialog.ShowMessageAsync(this,
                    "Network Error",
                    "A network error occured while trying to complete this action. " +
                    "Ensure you're properly connected with no firewall blocking this application and try again. " +
                    "If the problem persists, it may be a server issue.");

                Downloaded = false;
                Downloading = false;
                RaisePropertyChanged(nameof(DownloadText));
                return;
            }

            foreach (var filepath in downloaded)
            {
                if (new[] {".wad", ".lua"}.Any(ext => filepath.EndsWith(ext)))
                    extractedFiles.Add(filepath);

                else
                {
                    if (new[] {".zip", ".rar", ".7z"}.Any(ext => filepath.EndsWith(ext))
                        && !Regex.Match(filepath, @"(part[_ +-.]?(?:0?[2-9]|1\d])|7z.\d?\d[1-9])",
                            RegexOptions.IgnoreCase).Success)
                    {
                        using (var archive = ArchiveFactory.Open(filepath))
                            foreach (var entry in archive.Entries)
                                if (!entry.IsDirectory)
                                {
                                    extractedFiles.Add(entry.Key);
                                    entry.WriteToDirectory(path,
                                        new ExtractionOptions {ExtractFullPath = true, Overwrite = true});
                                }
                    }

                    File.Delete(filepath);
                }
            }

            // Add to repository

            var name = extractedFiles
                .Aggregate(Release.Name, (current, thing) => current.Replace($"({thing})", ""))
                .Trim();

            var mod = new Mod
            {
                Id = Release.Id,
                Name = name,
                Files = new ObservableCollection<ModFile>(extractedFiles.Select(file =>
                    new ModFile {Path = Path.Combine(path, file)})),
                ChangedThings = Release.ChangedThings
            };

            await _downloadedMods.Add(_model.Category, mod);

            Mod = mod;

            // Notify

            Downloaded = true;
            Downloading = false;
            Status = "Download successful.";
            RaisePropertyChanged(nameof(DownloadText));
            DownloadCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        ///     If the download button is available to be pressed
        /// </summary>
        private bool CanDownload()
        {
            return !_downloadedMods.Contains(_model.Category, Release)
                   && !Downloading;
        }
    }
}