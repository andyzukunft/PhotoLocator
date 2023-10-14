﻿using System;
using System.Diagnostics;
using System.Windows;

namespace PhotoLocator.Helpers
{
    static class ExceptionHandler
    {
        public static void ShowException(Exception exception)
        {
            if (exception is OperationCanceledException || exception.InnerException is OperationCanceledException)
                return;
            if (exception is UserMessageException)
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void LogException(Exception exception)
        {
            Debug.WriteLine(exception.ToString());
        }
    }
}
