﻿using MapControl;
using PhotoLocator.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PhotoLocator
{
    /// <summary>
    /// Interaction logic for SlideShowWindow.xaml
    /// </summary>
    public partial class SlideShowWindow : Window, INotifyPropertyChanged
    {
        readonly Collection<PictureItemViewModel> _pictures;
        int _pictureIndex;
        DispatcherTimer _timer;

        public SlideShowWindow(Collection<PictureItemViewModel> pictures, PictureItemViewModel selectedPicture, int slideShowInterval)
        {
            _pictures = pictures;
            SelectedPicture = selectedPicture;
            _pictureIndex = Math.Max(0, pictures.IndexOf(selectedPicture));
            _timer = new DispatcherTimer(TimeSpan.FromSeconds(slideShowInterval), DispatcherPriority.Normal, HandleTimerEvent, Dispatcher);
            InitializeComponent();
            DataContext = this;
            Map.DataContext = this;
            Map.map.TargetZoomLevel = 7;
            UpdatePicture();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, newValue))
                return false;
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public Visibility MapToolsVisibility => Visibility.Hidden;

        public Location? MapCenter
        {
            get => _mapCenter;
            set => SetProperty(ref _mapCenter, value);
        }
        private Location? _mapCenter;

        public bool IsMapVisible { get => _isMapVisible; set => SetProperty(ref _isMapVisible, value); }
        private bool _isMapVisible;

        public ImageSource? PictureSource { get => _pictureSource; set => SetProperty(ref _pictureSource, value); }
        private ImageSource? _pictureSource;

        public string? PictureName { get => _pictureName; set => SetProperty(ref _pictureName, value); }
        private string? _pictureName;

        public PictureItemViewModel SelectedPicture { get; private set; }

        private void UpdatePicture()
        {
            SelectedPicture = _pictures[_pictureIndex];
            PictureSource = new BitmapImage(new Uri(SelectedPicture.FullPath));
            PictureName = Path.GetFileNameWithoutExtension(SelectedPicture.Name);
            if (SelectedPicture.GeoTag is null)
                IsMapVisible = false;
            else
            {
                MapCenter = SelectedPicture.GeoTag;
                IsMapVisible = true;
            }
            _timer.Stop();
            _timer.Start();
            WinAPI.SetThreadExecutionState(WinAPI.EXECUTION_STATE.ES_DISPLAY_REQUIRED);
        }

        private void HandleTimerEvent(object? sender, EventArgs e)
        {
            _pictureIndex = (_pictureIndex + 1) % _pictures.Count;
            UpdatePicture();
        }

        private void HandlePreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
            else if (e.Key == Key.Up || e.Key == Key.PageUp)
            {
                _pictureIndex = Math.Max(0, _pictureIndex - 1);
                UpdatePicture();
            }
            else if (e.Key == Key.Down || e.Key == Key.PageDown)
            {
                _pictureIndex = Math.Min(_pictures.Count - 1, _pictureIndex + 1);
                UpdatePicture();
            }
            else if (e.Key == Key.Home)
            {
                _pictureIndex = 0;
                UpdatePicture();
            }
            else if (e.Key == Key.End)
            {
                _pictureIndex = _pictures.Count - 1;
                UpdatePicture();
            }
        }
    }
}
