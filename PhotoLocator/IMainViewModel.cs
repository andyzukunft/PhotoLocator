﻿using PhotoLocator.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoLocator
{
    public interface IMainViewModel
    {
        ISettings Settings { get; }

        PictureItemViewModel? SelectedItem { get; set; }

        IEnumerable<PictureItemViewModel> GetSelectedItems();

        Task SelectFileAsync(string outFileName);

        string? ProgressBarText { get; set; }

        Task RunProcessWithProgressBarAsync(Func<Action<double>, Task> body, string text, PictureItemViewModel? focusItem = null);
    }
}