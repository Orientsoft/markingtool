using DayEasy.MarkingTool.BLL.Config;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DayEasy.MarkingTool.UI.Scanner
{
    public partial class Setting
    {
        private readonly ScannerConfig _config;
        private readonly MarkingPaper _markingPaper;
        public Setting(MarkingPaper markingPaper)
        {
            InitializeComponent();
            _config = ConfigUtils<ScannerConfig>.Config;
            InitSetting();
            _markingPaper = markingPaper;
        }

        private void InitSetting()
        {
            var lights = new List<double>();
            var smearWidths = new List<int>();
            var smearHeights = new List<int>();
            for (int i = 0; i < 30; i++)
            {
                lights.Add(0.83D - i * 0.01D); //30
                if (i < 7)
                {
                    smearWidths.Add(15 - i); //7
                    smearHeights.Add(9 - i); //6
                }
            }
            LightScale.Items.Add(new ComboBoxItem
            {
                Content = "自动",
                DataContext = -1,
                IsSelected = _config.BasicThreshold < 0
            });
            foreach (var light in lights)
            {
                LightScale.Items.Add(new ComboBoxItem
                {
                    Content = light.ToString("0.00"),
                    DataContext = light,
                    IsSelected = (Math.Abs(_config.BasicThreshold - light) < 0.001)
                });
            }
            foreach (var smearWidth in smearWidths)
            {
                SmearWidth.Items.Add(new ComboBoxItem
                {
                    Content = smearWidth,
                    DataContext = smearWidth,
                    IsSelected = (_config.SmearWidth == smearWidth)
                });
            }
            foreach (var smearHeight in smearHeights)
            {
                SmearHeight.Items.Add(new ComboBoxItem
                {
                    Content = smearHeight,
                    DataContext = smearHeight,
                    IsSelected = (_config.SmearHeight == smearHeight)
                });
            }

            SheetType.Items.Add(new ComboBoxItem
            {
                Content = "旧卡",
                DataContext = 0,
                IsSelected = (_config.SheetType == 0)
            });
            SheetType.Items.Add(new ComboBoxItem
            {
                Content = "新卡",
                DataContext = 1,
                IsSelected = (_config.SheetType == 1)
            });

            RecognitionType.Items.Add(new ComboBoxItem
            {
                Content = "比例",
                DataContext = 0,
                IsSelected = (_config.RecognitionType == 0)
            });
            RecognitionType.Items.Add(new ComboBoxItem
            {
                Content = "区域",
                DataContext = 1,
                IsSelected = (_config.RecognitionType == 1)
            });

            switch (_config.Type)
            {
                case (int)ScannerType.Default:
                    DefaultRbt.IsChecked = true;
                    break;
                case (int)ScannerType.Light:
                    LightRbt.IsChecked = true;
                    break;
                case (int)ScannerType.Line:
                    LineRbt.IsChecked = true;
                    break;
            }
        }

        private void BtnSaveClick(object sender, RoutedEventArgs e)
        {
            _config.BasicThreshold = Math.Round(Convert.ToDouble(((ComboBoxItem)LightScale.SelectedItem).DataContext), 2);
            _config.SmearWidth = Convert.ToInt32(((ComboBoxItem)SmearWidth.SelectedItem).DataContext);
            _config.SmearHeight = Convert.ToInt32(((ComboBoxItem)SmearHeight.SelectedItem).DataContext);
            _config.SheetType = Convert.ToInt32(((ComboBoxItem)SheetType.SelectedItem).DataContext);
            _config.RecognitionType = Convert.ToInt32(((ComboBoxItem)RecognitionType.SelectedItem).DataContext);
            _config.Type = (int)ScannerType.Custom;
            if (DefaultRbt.IsChecked.HasValue && DefaultRbt.IsChecked.Value)
                _config.Type = (int)ScannerType.Default;
            if (LightRbt.IsChecked.HasValue && LightRbt.IsChecked.Value)
                _config.Type = (int)ScannerType.Light;
            if (LineRbt.IsChecked.HasValue && LineRbt.IsChecked.Value)
                _config.Type = (int)ScannerType.Line;

            ConfigUtils<ScannerConfig>.Instance.Set(_config);
            Close();
            _markingPaper.ReRecognition();
        }

        private void DefaultSettingClick(object sender, RoutedEventArgs e)
        {
            Height = 160;
            CustomPanel.Visibility = Visibility.Collapsed;
            LightScale.SelectedIndex = 0;
            SheetType.SelectedIndex = 1;
            RecognitionType.SelectedIndex = 0;
            SmearWidth.SelectedIndex = 4;
            SmearHeight.SelectedIndex = 3;
        }

        private void LightClick(object sender, RoutedEventArgs e)
        {
            Height = 160;
            CustomPanel.Visibility = Visibility.Collapsed;
            LightScale.SelectedIndex = 10;
        }

        private void LineClick(object sender, RoutedEventArgs e)
        {
            Height = 160;
            CustomPanel.Visibility = Visibility.Collapsed;
            SmearHeight.SelectedIndex = 6;
        }

        private void CustomClick(object sender, RoutedEventArgs e)
        {
            Height = 280;
            CustomPanel.Visibility = Visibility.Visible;
        }
    }
}
