using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media.Imaging;
using Deyi.Tool.Common;

namespace Deyi.Tool
{
    /// <summary>
    /// ImageView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageView : Window
    {
        public ImageView()
        {
            InitializeComponent();
        }

        public ImageView(string basePath, string[] selecteds)
            : this()
        {
            BasePath = basePath;
            Selecteds = selecteds;
        }

        private string _basePath = string.Empty;
        private string[] _selecteds = null;

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
            list.ItemsSource = Thrumbs;
            if (string.IsNullOrWhiteSpace(BasePath))
            {
                return;
            }
            BrowseImage(BasePath);
        }

        private ThrumbViewModelCollection _thrumbs = new ThrumbViewModelCollection();
        // private ICommand _browseCommand;

        protected ThrumbViewModelCollection Thrumbs { get { return _thrumbs; } }

        //public ICommand BrowseCommand
        //{
        //    get
        //    {
        //        if (_browseCommand == null)
        //        {
        //            _browseCommand = new RelayCommand(() => { BrowseImage(); }, () => { return true; });
        //        }

        //        return _browseCommand;
        //    }
        //}

        private void BrowseImage(string basePath)
        {

            ThrumbViewModel thrumb;
            _thrumbs.Clear();
            var arrFileName = Helper.GetAllImagePath(basePath).Where(f => !Selecteds.Any(p =>
                 p == Path.GetFileNameWithoutExtension(f) || p == f)).ToArray();

            Array.ForEach(arrFileName, fileName =>
            {
                thrumb = new ThrumbViewModel()
                {
                    FileName = fileName,
                    ThrumbHeight = 100
                };

                _thrumbs.Add(thrumb);
            });
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            list.SelectAll();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            PickupPicture();
            DialogResult = true;
            this.Close();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            list.UnselectAll();
        }

        private List<string> _selectPaths = new List<string>();

        public List<string> SelectPaths { get { return _selectPaths; } }

        void PickupPicture()
        {
            var selectedItems = list.SelectedItems;
            ThrumbViewModel itemTemp = null;
            foreach (var item in selectedItems)
            {
                if (item is ThrumbViewModel)
                {
                    itemTemp = item as ThrumbViewModel;
                    if (itemTemp != null)
                    {
                        SelectPaths.Add(itemTemp.FileName);
                    }
                }
            }
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {

                switch (e.Key)
                {
                    case Key.A:
                        checkAll.IsChecked = true;
                        break;
                    case Key.Z:
                        checkAll.IsChecked = false;
                        break;
                }
            }
            else
            {
                if (e.Key == Key.Delete || e.Key == Key.Back)
                {
                    DeleteSelected();
                }
            }
        }

        private void DeleteSelected()
        {
            ThrumbViewModel itemTemp = null;
            var selectedItems = list.SelectedItems;
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
            var temp = list.SelectedItem as ThrumbViewModel;
            if (!File.Exists(temp.FileName))
            {
                return;
            }
            new PaperImageDetail(new BitmapImage(new Uri(temp.FileName))).Show();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelected();
        }

    }
}
