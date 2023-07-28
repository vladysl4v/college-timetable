﻿using System.Windows;

using LightTimetable.Tools;
using LightTimetable.Common;
using LightTimetable.ViewModels;
using LightTimetable.Properties;


namespace LightTimetable.Views
{
    // MVVM violating
    public partial class TimetableView : Window
    {
        private SizeChangedEventHandler _sizeChanged;
        private TimetableViewModel _viewModel => (TimetableViewModel)DataContext;
        
        public TimetableView()
        {
            WindowMediator.OnRepositionRequired += InvokeWindowResize;

            _sizeChanged = PositionCalculator.Calculate((WindowPosition)Settings.Default.WindowPosition);
            InitializeComponent();
        }

        public void InvokeWindowResize()
        {
            Width = Settings.Default.ShowOutages ? 425 : 400;
            Width += _viewModel.IsDataGridExpanded ? 100 : 0;
            _sizeChanged = PositionCalculator.Calculate((WindowPosition)Settings.Default.WindowPosition);
            _sizeChanged.Invoke(this, null);
        }

        public void WindowSizeChanged(object? s, SizeChangedEventArgs e) => _sizeChanged.Invoke(s, e);
    }
}
