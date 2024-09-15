﻿using Microsoft.Win32;
using PhotoLocator.Helpers;
using PhotoLocator.Metadata;
using PhotoLocator.PictureFileFormats;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PhotoLocator
{
    public class JpegTransformCommands
    {
        private readonly IMainViewModel _mainViewModel;

        public JpegTransformCommands(IMainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public ICommand RotateLeftCommand => new RelayCommand(async o => await RotateSelectedAsync(270));

        public ICommand RotateRightCommand => new RelayCommand(async o => await RotateSelectedAsync(90));

        public ICommand Rotate180Command => new RelayCommand(async o => await RotateSelectedAsync(180));

        private async Task RotateSelectedAsync(int angle)
        {
            var allSelected = _mainViewModel.GetSelectedItems().Where(item => item.IsFile && JpegTransformations.IsFileTypeSupported(item.Name)).ToArray();
            if (allSelected.Length == 0)
                throw new UserMessageException("Unsupported file format");
            await _mainViewModel.RunProcessWithProgressBarAsync(progressCallback => Task.Run(() =>
            {
                progressCallback(-1);
                int i = 0;
                foreach (var item in allSelected)
                {
                    JpegTransformations.Rotate(item.FullPath, item.GetProcessedFileName(), angle); //TODO: Make async
                    item.Rotation = Rotation.Rotate0;
                    item.IsChecked = false;
                    progressCallback((double)(++i) / allSelected.Length);
                }
            }), "Rotating...");
        }

        public ICommand LocalContrastCommand => new RelayCommand(o =>
        {
            if (_mainViewModel.SelectedItem?.IsDirectory != false)
                return;
            LocalContrastViewModel localContrastViewModel;
            BitmapMetadata? metadata = null;
            using (var cursor = new MouseCursorOverride())
            {
                var image = _mainViewModel.SelectedItem.LoadPreview(default, int.MaxValue, true);
                try
                {
                    using var file = File.OpenRead(_mainViewModel.SelectedItem.FullPath);
                    var decoder = BitmapDecoder.Create(file, ExifHandler.CreateOptions, BitmapCacheOption.OnLoad);
                    metadata = decoder.Frames[0].Metadata as BitmapMetadata;
                    image ??= decoder.Frames[0];
                }
                catch { }
                localContrastViewModel = new LocalContrastViewModel() 
                { 
                    SourceBitmap = image ?? throw new UserMessageException(_mainViewModel.SelectedItem.ErrorMessage) 
                };
            }
            var window = new LocalContrastView();
            window.Owner = Application.Current.MainWindow;
            window.OkButton.Content = "_Save as...";
            window.DataContext = localContrastViewModel;
            if (window.ShowDialog() != true)
                return;
            var dlg = new SaveFileDialog();
            dlg.InitialDirectory = Path.GetDirectoryName(_mainViewModel.SelectedItem.FullPath);
            dlg.FileName = Path.GetFileNameWithoutExtension(_mainViewModel.SelectedItem.Name) + ".jpg";
            dlg.Filter = GeneralFileFormatHandler.SaveImageFilter;
            dlg.DefaultExt = "jpg";
            if (dlg.ShowDialog() != true)
                return;
            GeneralFileFormatHandler.SaveToFile(localContrastViewModel.PreviewPictureSource!, dlg.FileName, metadata);
        });
    }
}