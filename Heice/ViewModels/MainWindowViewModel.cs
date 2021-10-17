using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using Heice.Utils;
using ImageMagick;

namespace Heice.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region [ctor]
        public MainWindowViewModel()
        {
            MapDependencies<MainWindowViewModel>();
            CanChangeDirectories = true;
            
            FileCount = 0;
            ConvertedFileCount = 0;

            IsProgressRunning = false;
        }
        #endregion

        #region - properties -

        public bool CanChangeDirectories { get => GetProp<bool>(); set => SetProp(value); }

        public string SourceFolderPath
        {
            get => GetProp<string>();
            set
            {
                SetProp(value);

                if (Directory.Exists(value))
                {
                    ConvertedFileCount = 0;
                    FileCount = Directory.GetFiles(value).Length;
                }
            }
        }

        public string TargetFolderPath { get => GetProp<string>(); set => SetProp(value); }
        
        public bool IsProgressRunning { get => GetProp<bool>(); set => SetProp(value); }
        
        public int FileCount { get => GetProp<int>(); set => SetProp(value); }
        
        public int ConvertedFileCount { get => GetProp<int>(); set => SetProp(value); }
        
        [DependsOn(nameof(FileCount))]
        [DependsOn(nameof(ConvertedFileCount))]
        public string ProgressText => $"( {ConvertedFileCount} | {FileCount})";

        public string CurrentFileText { get => GetProp<string>(); set => SetProp(value); }

        public ICommand BtnStartCommand => new RelayCommand(_ => ExecuteStart());

        [DependsOn(nameof(IsProgressRunning))]
        [DependsOn(nameof(SourceFolderPath))]
        [DependsOn(nameof(TargetFolderPath))]
        public bool CanStart
        {
            get
            {
                if (IsProgressRunning)
                    return false;
                
                if (!Directory.Exists(SourceFolderPath))
                    return false;

                if (!Directory.Exists(TargetFolderPath))
                    return false;

                if (SourceFolderPath == TargetFolderPath)
                    return false;

                return true;
            }
        }

        #endregion

        public void ExecuteStart()
        {
            Task.Run(() =>
            {
                try
                {
                    IsProgressRunning = true;
                    ConvertedFileCount = 0;
                
                    if (!Directory.Exists(SourceFolderPath))
                        return;

                    var files = Directory.GetFiles(SourceFolderPath);

                    foreach (var filePath in files)
                    {
                        var fInfo = new FileInfo(filePath);

                        CurrentFileText = fInfo.Name;

                        switch (fInfo.Extension)
                        {
                            case ".heic": ProcessHeicFileAsync(filePath, fInfo).Wait(); break;
                            default: continue;
                        }

                        ConvertedFileCount++;
                    }
                }
                finally
                {
                    IsProgressRunning = false;
                }
            });
        }

        public async Task ProcessHeicFileAsync(string filePath, FileInfo fileInfo)
        {
            await Task.Run(() =>
            {
                using var img = new MagickImage(filePath);

                var newPath = Path.Combine(TargetFolderPath, fileInfo.Name.Replace(fileInfo.Extension, "") + ".jpg");
            
                if (File.Exists(newPath))
                    File.Delete(newPath);
            
                img.Write(newPath);
            });
        }
    }
}