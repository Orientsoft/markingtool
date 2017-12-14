using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.Core;
using DayEasy.MarkingTool.ImageView;

namespace DayEasy.MarkingTool.UI.Scanner
{
    /// <summary> 打开试卷 </summary>
    public partial class ImageView
    {
        private readonly string _basePath = string.Empty;
        private readonly ObservableCollection<ImageModel> _thrumbs;
        private readonly List<string> _selectPaths;
        private readonly IEnumerable<string> _selectedNames; 
        public List<string> SelectPaths
        {
            get { return _selectPaths ?? new List<string>(); }
        }

        public ImageView()
        {
            InitializeComponent();
            _thrumbs = new ObservableCollection<ImageModel>();
            _selectPaths = new List<string>();
            AddHandler(Keyboard.KeyUpEvent, (KeyEventHandler)HandleKeyUpEvent);
        }

        public ImageView(string basePath, IEnumerable<string> selecteds)
            : this()
        {
            _basePath = basePath;
            _selectedNames = selecteds;
        }

        /// <summary>
        /// 键盘事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUpEvent(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    StartMarking();
                    break;
                case Key.Delete:
                case Key.Back:
                    DeleteSelected();
                    break;
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                return;
            switch (e.Key)
            {
                case Key.A:
                    CheckAll.IsChecked = true;
                    break;
                case Key.Z:
                    CheckAll.IsChecked = false;
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List.ItemsSource = _thrumbs;
            if (string.IsNullOrWhiteSpace(_basePath))
                return;
            BrowseImage(_basePath);
        }

        private void BrowseImage(string basePath)
        {
            _thrumbs.Clear();
            var arrFileName = Helper.GetAllImagePath(basePath).Where(f => !_selectedNames.Any(p =>
                p == Path.GetFileNameWithoutExtension(f) || p == f)).ToArray();

            Array.ForEach(arrFileName, fileName =>
            {
                var thrumb = new ImageModel
                {
                    FileName = fileName,
                    ThrumbHeight = 100
                };

                _thrumbs.Add(thrumb);
            });
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            List.SelectAll();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StartMarking();
        }

        /// <summary> 开始识别 </summary>
        private void StartMarking()
        {
            PickupPicture();
            if (_selectPaths == null || !_selectPaths.Any())
            {
                WindowsHelper.ShowError("请先选择要识别的试卷！");
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            List.UnselectAll();
        }

        private void PickupPicture()
        {
            var selectedItems = List.SelectedItems;
            foreach (var item in selectedItems)
            {
                if (item is ImageModel)
                {
                    var itemTemp = item as ImageModel;
                    _selectPaths.Add(itemTemp.FileName);
                }
            }
        }

        private void DeleteSelected()
        {
            ImageModel itemTemp = null;
            var selectedItems = List.SelectedItems;
            while (selectedItems.Count != 0)
            {
                if (selectedItems[0] is ImageModel)
                {
                    itemTemp = selectedItems[0] as ImageModel;
                }
                if (itemTemp == null)
                {
                    continue;
                }
                _thrumbs.Remove(itemTemp);
            }
        }

        private void list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var temp = List.SelectedItem as ImageModel;
            if (temp == null || !File.Exists(temp.FileName))
                return;
            new PaperImageDetail(temp.FileName).Show();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelected();
        }
    }
}
