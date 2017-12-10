using System;
using System.Collections.Generic;
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

namespace srb2_mod_management.ViewModels.Components
{
    public class ReleaseViewModel : ViewModelBase
    {
        private static readonly WebClient Downloader = new WebClient();
        private readonly IModRetreiverService _modService;
        private readonly IDownloadedModsRepository _downloadedMods;
        private static string ImageFolder => Path.Combine(SettingsRepository.ApplicationDirectory, "images");
        private Release _release;
        private string _image;
        private int _index;
        private DiscoverModel _model;
        private bool _loadingImage;
        private bool _downloading;
        private bool _notDownloaded;
        private string _status;
        private Mod _mod;

        // 

        public ReleaseViewModel(IModRetreiverService modService, IDownloadedModsRepository downloadedMods)
        {
            _modService = modService;
            _downloadedMods = downloadedMods;
            PreviousImageCommand = new RelayCommand(PreviousImage, () => !LoadingImage);
            NextImageCommand = new RelayCommand(NextImage, () => !LoadingImage);
            WebpageCommand = new RelayCommand(() => Process.Start(_model.ReleaseInfo.Url));
        }

        // 

        public async Task<ReleaseViewModel> SetModel(DiscoverModel model)
        {
            _model = model;
            Release = await _modService.RetrieveRelease(_model.ReleaseInfo);
            DownloadCommand = new RelayCommand(Download,
                () => !_downloadedMods.AlreadyContains(_model.Category, Release)
                      && !Downloading);
            NotDownloaded = !_downloadedMods.AlreadyContains(_model.Category, Release);
            if (!NotDownloaded)
            {
                Mod = _downloadedMods.Find(model.Category, model.ReleaseInfo);
                foreach (var file in Mod.Files)
                {
                    file.PropertyChanged -= FileOnPropertyChanged;
                    file.PropertyChanged += FileOnPropertyChanged;
                }
            }
            Status = "Available to use.";
            Index = 0;
            Image = "";
            return this;
        }

        private async void FileOnPropertyChanged(object sender, PropertyChangedEventArgs args) => await _downloadedMods.Save();

        public async Task VisibleChanged(bool visible)
        {
            if (visible)
                await SetImage(Release.Screenshots[Index], Index);
        }

        // 

        public int Index
        {
            get => _index;
            set => Set(() => Index, ref _index, value);
        }

        public string Image
        {
            get => _image;
            set => Set(() => Image, ref _image, value);
        }

        public Release Release
        {
            get => _release;
            set => Set(() => Release, ref _release, value);
        }

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

        public bool Downloading
        {
            get => _downloading;
            set
            {
                Set(() => Downloading, ref _downloading, value);
                DownloadCommand.RaiseCanExecuteChanged();
            }
        }
       
        public bool NotDownloaded
        {
            get => _notDownloaded;
            set => Set(() => NotDownloaded, ref _notDownloaded, value);
        }

        public string Status
        {
            get => _status;
            set => Set(() => Status, ref _status, value);
        }

        public Mod Mod
        {
            get => _mod;
            set => Set(() => Mod, ref _mod, value);
        }

        public RelayCommand DownloadCommand { get; set; }

        public RelayCommand WebpageCommand { get; set; }

        public RelayCommand PreviousImageCommand { get; set; }

        public RelayCommand NextImageCommand { get; set; }

        public string Progress => $"{Index + 1}/{Release.Screenshots.Count}";

        public string DownloadText => Downloading ? "Downloading ..." : "Download";
        
        // 

        private async void PreviousImage()
        {
            Index = Index - 1 < 0 ? Release.Screenshots.Count - 1 : Index - 1;
            await SetImage(Release.Screenshots[Index], Index);
        }

        private async void NextImage()
        {
            Index = (Index + 1) % Release.Screenshots.Count;
            await SetImage(Release.Screenshots[Index], Index);
        }

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
                    await _modService.UpdateRelease(Release);
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

            foreach (var download in Release.Downloads)
            {
                var filepath = Path.Combine(path, download.Filename);
                if (!File.Exists(filepath))
                    await Downloader.DownloadFileTaskAsync(download.Link, filepath);
                downloaded.Add(filepath);
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
                Files = extractedFiles.Select(file => new ModFile { Path = Path.Combine(path, file) }).ToList(),
                ChangedThings = Release.ChangedThings
            };

            await _downloadedMods.Add(_model.Category, mod);

            Mod = mod;

            // Notify

            NotDownloaded = false;
            Downloading = false;
            Status = "Download successful.";
            RaisePropertyChanged(nameof(DownloadText));
            DownloadCommand.RaiseCanExecuteChanged();
        }
    }
}