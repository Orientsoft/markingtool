using Deyi.Tool.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Deyi.Tool
{
    /// <summary>
    /// Window2.xaml 的交互逻辑
    /// </summary>
    public partial class PictureListWindow : Window
    {


        public PictureListWindow(ref List<string> picPaths)
        {
            PicPaths = picPaths;
            InitializeComponent();
        }

        private List<string> PicPaths = null;

        private int size
        {
            get
            {
                if (null == PicPaths)
                {
                    return 0;
                }
                return PicPaths.Count;
            }
        }

        private int columnCount
        {
            get
            {
                return gdDetail.ColumnDefinitions.Count;
            }
        }

        private void InitPic()
        {
            if (null == PicPaths)
            {
                return;
            }
            RowDefinition rDef = null;
            int totalRow = (int)Math.Ceiling((decimal)size / columnCount);
            var realRow = totalRow > 3 ? totalRow : 4;
            for (int i = 0; i < realRow; i++)
            {
                rDef = new RowDefinition();
                rDef.Height = new GridLength(180, GridUnitType.Pixel);
                gdDetail.RowDefinitions.Add(rDef);
            }



            Image img = null;
            int r = 0, s = 0;
            Border bder = null;
            StackPanel stack = null;
            TextBlock txt = null;
            while (s < size)
            {
                for (int c = 0; c < columnCount; c++)
                {
                    if (s == size)
                    {
                        break;
                    }
                    img = new Image() { Source = new BitmapImage(new Uri(PicPaths[s])) };
                    img.Height = 140d;
                    img.Width = 100d;
                    img.MouseLeave += img_MouseLeave;
                    img.MouseEnter += img_MouseEnter;
                    txt = new TextBlock();
                    txt.Text = System.IO.Path.GetFileNameWithoutExtension(PicPaths[s]);
                    txt.TextAlignment = TextAlignment.Center;
                    //img.MouseUp += m;
                    bder = new Border();

                    bder.MouseLeftButtonDown += bder_MouseLeftButtonDown;
                    // bder.BorderThickness = new Thickness(1d);
                    bder.Background = Brushes.White;

                    stack = new StackPanel();
                    stack.Children.Add(img);
                    stack.Children.Add(txt);
                    bder.Tag = PicPaths[s];
                    bder.Width = 130d;
                    bder.Height = 170d;

                    bder.Child = stack;

                    Grid.SetRow(bder, r);
                    Grid.SetColumn(bder, c);
                    gdDetail.Children.Add(bder);

                    s++;
                }
                r++;
            }
        }

        void bder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;

            Brush orb = null;

            if (e.ClickCount == 1)
            {
                orb = border.Background;
                if (orb == Brushes.DarkCyan)
                {
                    border.Background = Brushes.White;
                }
                else
                {
                    border.Background = Brushes.DarkCyan;
                }

            }
            else if (e.ClickCount == 2)
            {
                border.BorderBrush = orb;
                new PaperImageDetail(((border.Child as StackPanel).Children[0] as Image).Source).ShowDeyiDialog(this);
            }
        }

        void img_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image img = sender as Image;
            img.ForceCursor = true;
            img.Width += 8d;
            img.Height += 12d;
        }


        void img_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image img = sender as Image;
            //img.ForceCursor = true;
            img.Width -= 8d;
            img.Height -= 12d;
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            PickupPicture();
            DialogResult = true;
            this.Close();
        }

        private void PickupPicture()
        {
            Border border;
            var children = gdDetail.Children;
            foreach (var item in children)
            {
                try
                {
                    border = item as Border;
                    if (border.Background != Brushes.DarkCyan)
                    {
                        // list.Add(border.Tag.ToString());
                        PicPaths.Remove(border.Tag.ToString());
                    }
                }
                catch
                {
                    continue;
                }
            }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckOrUncheckedAll(CheckedType.Checked);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOrUncheckedAll(CheckedType.Unchecked);
        }

        private void CheckOrUncheckedAll(CheckedType checkType)
        {
            Border border;
            var children = gdDetail.Children;
            foreach (var item in children)
            {
                if (item is Border)
                {
                    border = item as Border;
                    if (checkType == CheckedType.Checked)
                    {
                        if (border.Background != Brushes.DarkCyan)
                        {
                            border.Background = Brushes.DarkCyan;
                        }
                    }
                    else
                    {
                        if (border.Background == Brushes.DarkCyan)
                        {
                            border.Background = Brushes.White;
                        }
                    }

                }
            }

        }

        private void DeleteChecked()
        {
            Border border;
            var children = gdDetail.Children;
            for (var i = 0; i < children.Count; i++)
            {
                if (children[i] is Border)
                {
                    border = children[i] as Border;
                    if (border.Background == Brushes.DarkCyan)
                    {
                        PicPaths.Remove(border.Tag.ToString());
                        gdDetail.Children.Remove(border);
                        i--;
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
                    DeleteChecked();
                }
            }
        }


        enum CheckedType : byte
        {
            Unchecked = 0,
            Checked = 1
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitPic();
        }

    }
}
