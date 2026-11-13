using System;
using System.Windows;
using System.Windows.Controls;

namespace CrunchUtilities
{
    public partial class ConfigControl : UserControl
    {
        private readonly Action _save;

        public ConfigControl()
        {
            InitializeComponent();
        }

        public ConfigControl(object config, Action save) : this()
        {
            DataContext = config;   // the PropertyGrid inherits this
            _save = save;
        }

        private void SaveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            _save?.Invoke();
        }
    }
}