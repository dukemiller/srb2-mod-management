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
using SharpCompress.Common;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;

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
        private bool _updating;
        private bool _updateAvailable;

        // 

        public ReleaseViewModel(IModRetreiverService modService, IDownloadedModsRepository downloadedMods)
        {
            _modService = modService;
            _downloadedMods = downloadedMods;

            DownloadCommand = new RelayCommand(Download, CanDownload);
            RefreshCommand = new RelayCommand(Refresh);
            UpdateCommand = new RelayCommand(Update, () => !Updating);
            PreviousImageCommand = new RelayCommand(PreviousImage, () => !LoadingImage);
            NextImageCommand = new RelayCommand(NextImage, () => !LoadingImage);
            WebpageCommand = new RelayCommand(() => Process.Start(_model.ReleaseInfo.Url));
        }
        
        // 

        public async Task<ReleaseViewModel> SetModel(DiscoverModel model)
        {
            _model = model;
            Release = await _modService.RetrieveRelease(_model.ReleaseInfo);
            Downloaded = _downloadedMods.Contains(_model.Category, Release);

            // Set download information
            await SetInformation();

            return this;
        }

        private async Task SetInformation()
        {
            UpdateAvailable = Release.UpdateAvailable;

            if (Downloaded)
            {
                Mod = _downloadedMods.Find(_model.Category, _model.ReleaseInfo);
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

            if (_model.Refresh)
            {
                await _modService.UpdateRelease(_model.ReleaseInfo);
                Release = await _modService.RetrieveRelease(_model.ReleaseInfo);
                Downloaded = _downloadedMods.Contains(_model.Category, Release);
            }

            if (UpdateAvailable)
                Status = "An update is available.";
        }

        /// <summary>
        ///     If the mod is suitable to be retrieved
        /// </summary>
        private bool Retreivable()
        {
            var hasDownloads = Release.Downloads.Count > 0;
            var onWhiteList = new[] {"ArchPack", "MonitorsPlus"}
                .Any(word => Release.Name.Contains(word));
            var containsFilteredWord = new[] {"md2", "launcher"}
                .Any(word => Release.Name.ToLower().Contains(word));
            var containsFilteredChangedThing = new[] {"Models", "Additional Software"}
                .Any(changedThing => Release.ChangedThings.Contains(changedThing));

            return hasDownloads && (onWhiteList || (!containsFilteredWord && !containsFilteredChangedThing));
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
        ///     If currently ongoing an update
        /// </summary>
        public bool Updating
        {
            get => _updating;
            set => Set(() => Updating, ref _updating, value);
        }

        /// <summary>
        ///     If an update is available.
        /// </summary>
        public bool UpdateAvailable
        {
            get => _updateAvailable;
            set
            {
                Set(() => UpdateAvailable, ref _updateAvailable, value);
                UpdateCommand.RaiseCanExecuteChanged();
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
            set => Set(() => Available, ref _available, value);
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

        public RelayCommand UpdateCommand { get; set; }

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

            var (extractedFiles, successful) = await DownloadFiles(path);

            if (!successful)
                return;

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
        ///     Attempt to download the files of the current release
        /// </summary>
        private async Task<(List<string> files, bool successful)> DownloadFiles(string path)
        {
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
                return (downloaded, false);
            }

            foreach (var filepath in downloaded)
            {
                if (new[] { ".wad", ".lua" }.Any(ext => filepath.EndsWith(ext)))
                    extractedFiles.Add(filepath);

                else
                {
                    if (new[] { ".zip", ".rar", ".7z" }.Any(ext => filepath.EndsWith(ext))
                        && !Regex.Match(filepath,
                            @"(part[_ +-.]?(?:0?[2-9]|1\d])|7z.\d?\d[1-9]|\.[0-9]?(?:0[2-9]|[1-9][0-9])$)",
                            RegexOptions.IgnoreCase).Success)
                    {
                        using (var archive = ArchiveFactory.Open(filepath))
                            foreach (var entry in archive.Entries)
                                if (!entry.IsDirectory)
                                {
                                    if (File.Exists(Path.Combine(path, entry.Key)))
                                        continue;
                                    extractedFiles.Add(entry.Key);
                                    entry.WriteToDirectory(path,
                                        new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                                }
                    }

                    File.Delete(filepath);
                }
            }

            return (extractedFiles, true);
        }

        /// <summary>
        ///     If the download button is available to be pressed
        /// </summary>
        private bool CanDownload()
        {
            return !_downloadedMods.Contains(_model.Category, Release)
                   && !Downloading;
        }
        
        private async void Refresh()
        {
            await _modService.UpdateRelease(_model.ReleaseInfo);
            Release = await _modService.RetrieveRelease(_model.ReleaseInfo);
            await SetInformation();
        }

        private async void Update()
        {
            Updating = true;
            UpdateCommand.RaiseCanExecuteChanged();

            // Backup current files

            var backupPath = Path.Combine(SettingsRepository.ApplicationDirectory, $"{Mod.Id}-backup.zip");
            using (var zip = File.OpenWrite(backupPath))
            using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, new ZipWriterOptions(CompressionType.None)))
                foreach (var file in Mod.Files)
                {
                    zipWriter.Write(Path.GetFileName(file.Path), file.Path);
                    File.Delete(file.Path);
                }

            // Download new files 

            var path = Path.Combine(SettingsRepository.ApplicationDirectory, "mods",
                _model.Category.ToString().ToLower());
            var (extractedFiles, successful) = await DownloadFiles(path);

            if (!successful)
            {
                // Remove all files

                foreach (var file in extractedFiles)
                    File.Delete(file);

                // Restore backup

                using (var stream = File.OpenRead(backupPath))
                {
                    var reader = ReaderFactory.Open(stream);
                    while (reader.MoveToNextEntry())
                        if (!reader.Entry.IsDirectory)
                            reader.WriteEntryTo(path);
                }
                File.Delete(backupPath);
                Updating = false;
                return;
            }
            
            // Replace mods downloaded files with new extracted files

            File.Delete(backupPath);
            Mod.Files = new ObservableCollection<ModFile>(extractedFiles.Select(file =>
                new ModFile {Path = Path.Combine(path, file)}));
            Updating = false;
            Release.UpdateAvailable = false;
            await _downloadedMods.Update(_model.Category, Mod);
            await _modService.ReplaceRelease(Release);
            await SetInformation();
        }
    }
}