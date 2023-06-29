﻿using CommunityToolkit.Mvvm.Input;

using System.Windows;

using LightTimetable.Views;
using LightTimetable.Tools;
using LightTimetable.Properties;


namespace LightTimetable.ViewModels
{
    public partial class NotifyIconViewModel
    {
        private SettingsView? _settingsWindow;

        public NotifyIconViewModel()
        {
            Application.Current.MainWindow = new TimetableView();
        }

        #region Commands

        [RelayCommand]
        private void SingleClick()
        {
            if (Settings.Default.OpenWindowMode == 0)
                ShowTimetable();
        }
        
        [RelayCommand]
        private void DoubleClick()
        {
            if (Settings.Default.OpenWindowMode == 1)
                ShowTimetable();
        }

        [RelayCommand]
        private void MiddleClick()
        {
            switch (Settings.Default.MiddleMouseClick)
            {
                case 1: RefreshData(); break;
                case 2: OpenSettings(); break;
            }
        }

        [RelayCommand]
        private void ShowTimetable()
        {
            Application.Current.MainWindow?.Show();
        }

        [RelayCommand]
        private void RefreshData()
        {
            WindowMediator.ReloadRequired();
        }

        [RelayCommand]
        private void OpenSettings()
        {
            if (_settingsWindow != null) 
                return;
            _settingsWindow = new SettingsView();
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        }

        [RelayCommand]
        private void CloseApplication()
        {
            Application.Current.Shutdown();
        }

        #endregion

    }
}
