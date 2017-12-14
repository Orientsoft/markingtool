using DayEasy.MarkingTool.BLL.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DayEasy.MarkingTool.ImageView;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// ImageView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageView
    {
        public ImageView()
        {
            InitializeComponent();
            AddHandler(Keyboard.KeyUpEvent,(KeyEventHandler)HandleKeyUpEvent);
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
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control) return;
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

        public ImageView(string basePath, string[] selecteds)
            : this()
        {
            BasePath = basePath;
            Selecteds = selecteds;
        }

        private string _basePath = string.Empty;
        private string[] _selecteds;

        /// <summary>
        /// 设置已选图片名
        /// </summary>
        public string[] Selecteds
        {
            get
            {
                if (_selecteds == null)
                {
                    return new string[0];
                }
                return _selecteds;
            }
            set { _selecteds = value; }
        }

        /// <summary>
        /// 图片文件夹
        /// </summary>
        public string BasePath
        {
            get { return _basePath; }
            set { _basePath = value; }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List.ItemsSource = Thrumbs;
            if (string.IsNullOrWhiteSpace(BasePath))
            {
                return;
            }
            BrowseImage(BasePath);
        }

        private readonly ThrumbViewModelCollection _thrumbs = new ThrumbViewModelCollection();
        // private ICommand _browseCommand;

        protected ThrumbViewModelCollection Thrumbs { get { return _thrumbs; } }

        private void BrowseImage(string basePath)
        {
            _thrumbs.Clear();
            var arrFileName = Helper.GetAllImagePath(basePath).Where(f => !Selecteds.Any(p =>
                 p == Path.GetFileNameWithoutExtension(f) || p == f)).ToArray();

            Array.ForEach(arrFileName, fileName =>
            {
                var thrumb = new ThrumbViewModel
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

        private void StartMarking()
        {
            PickupPicture();
            DialogResult = true;
            Close();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            List.UnselectAll();
        }

        private readonly List<string> _selectPaths = new List<string>();

        public List<string> SelectPaths { get { return _selectPaths; } }

        void PickupPicture()
        {
            var selectedItems = List.SelectedItems;
            foreach (var item in selectedItems)
            {
                if (item is ThrumbViewModel)
                {
                    var itemTemp = item as ThrumbViewModel;
                    SelectPaths.Add(itemTemp.FileName);
                }
            }
        }

        private void DeleteSelected()
        {
            ThrumbViewModel itemTemp = null;
            var selectedItems = List.SelectedItems;
            while (selectedItems.Count != 0)
            {
                if (selectedItems[0] is ThrumbViewModel)
                {
                    itemTemp = selectedItems[0] as ThrumbViewModel;
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
            var temp = List.SelectedItem as ThrumbViewModel;
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
