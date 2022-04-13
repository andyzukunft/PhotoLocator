﻿using PhotoLocator.Helpers;
using PhotoLocator.MapDisplay;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PhotoLocator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            Panel.SetZIndex(ProgressGrid, 1000);
            _viewModel = new MainViewModel();
            _viewModel.GetSelectedMapLayerName = GetSelectedMapLayerName;
            _viewModel.SelectedViewModeItem = MapViewItem;
            _viewModel.ScrollIntoView = PictureListBox.ScrollIntoView;
            _viewModel.ViewModeCommand = new RelayCommand(s =>
                _viewModel.SelectedViewModeItem = _viewModel.SelectedViewModeItem == MapViewItem ? PreviewViewItem : MapViewItem);
            DataContext = _viewModel;
        }

        private void HandleWindowLoaded(object sender, RoutedEventArgs e)
        {
            var settings = new RegistrySettings();
            _viewModel.PhotoFileExtensions = settings.PhotoFileExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            _viewModel.SavedFilePostfix = settings.SavedFilePostfix;
            _viewModel.SlideShowInterval = settings.SlideShowInterval;
            _viewModel.ShowMetadataInSlideShow = settings.ShowMetadataInSlideShow;
            var i = settings.LeftColumnWidth;
            if (i > 10 && i < Width)
                LeftColumn.Width = new GridLength(i);
            var selectedLayer = settings.SelectedLayer;
            Map.mapLayersMenuButton.ContextMenu.Items.OfType<MenuItem>().FirstOrDefault(item => Equals(item.Header, selectedLayer))?.
                RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
                Dispatcher.BeginInvoke(() =>
                {
                    _viewModel.PhotoFolderPath = Path.GetDirectoryName(args[1]);
                    _viewModel.HandleDroppedFilesAsync(args[1..]).WithExceptionShowing();
                });
            else
                Dispatcher.BeginInvoke(() => _viewModel.PhotoFolderPath = settings.PhotoFolderPath);

            if (settings.FirstLaunch < 1)
            {
                MainViewModel.AboutCommand.Execute(null);
                settings.FirstLaunch = 1;
            }
            PictureListBox.Focus();

            Task.Run(() => CleanupTileCache(MapView.TileCachePath)).WithExceptionLogging();
        }

        private static void CleanupTileCache(string tileCachePath)
        {
            Task.Delay(4000).Wait();
            var timeThreshold = DateTime.Now - TimeSpan.FromDays(365);
            foreach (var cacheFile in new DirectoryInfo(tileCachePath).EnumerateFiles("*.*", SearchOption.AllDirectories))
                if (cacheFile.CreationTime < timeThreshold)
                    cacheFile.Delete();
        }

        private void HandleWindowClosed(object sender, EventArgs e)
        {
            var settings = new RegistrySettings();
            if (!string.IsNullOrEmpty(_viewModel.PhotoFolderPath))
                settings.PhotoFolderPath = _viewModel.PhotoFolderPath;
            if (_viewModel.PhotoFileExtensions != null)
                settings.PhotoFileExtensions = String.Join(",", _viewModel.PhotoFileExtensions);
            if (_viewModel.SavedFilePostfix != null)
                settings.SavedFilePostfix = _viewModel.SavedFilePostfix;
            settings.SlideShowInterval = _viewModel.SlideShowInterval;
            settings.ShowMetadataInSlideShow = _viewModel.ShowMetadataInSlideShow;
            settings.LeftColumnWidth = (int)LeftColumn.Width.Value;
            settings.SelectedLayer = GetSelectedMapLayerName();
            _viewModel.Dispose();
        }

        private string? GetSelectedMapLayerName()
        {
            return Map.mapLayersMenuButton.ContextMenu.Items.Cast<MenuItem>().FirstOrDefault(i => i.IsChecked)?.Header as string;
        }

        private void HandlePictureListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.PictureSelectionChanged();
        }

        private void HandlePathEditPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _viewModel.PhotoFolderPath = PathEdit.Text;
        }

        private void HandleDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop) is string[] droppedEntries && droppedEntries.Length > 0)
                Dispatcher.BeginInvoke(() => _viewModel.HandleDroppedFilesAsync(droppedEntries).WithExceptionShowing());
        }

        private void HandleViewModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel is null)
                return;
            if (_viewModel.InSplitViewMode) // For some reason this doesn't work when done on the bound properties
            {
                MapRow.Height = new GridLength(1, GridUnitType.Star);
                PreviewRow.Height = new GridLength(1, GridUnitType.Star);
            }
            else if (_viewModel.IsMapVisible)
            {
                MapRow.Height = new GridLength(1, GridUnitType.Star);
                PreviewRow.Height = new GridLength(0, GridUnitType.Star);
            }
            else if (_viewModel.IsPreviewVisible)
            {
                MapRow.Height = new GridLength(0, GridUnitType.Star);
                PreviewRow.Height = new GridLength(1, GridUnitType.Star);
            }
        }
    }
}
