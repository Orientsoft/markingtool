
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Data;

namespace DayEasy.MarkingTool.UI.Controls
{
    /// <summary>
    /// DeyiTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class DeyiTextBox
    {
        #region 属性
        private bool _isAutoComplete;
        public string Text
        {
            get { return InputBox.Text.Trim(); }
            set
            {
                InputBox.Text = value;
            }
        }

        /// <summary> Tab键顺序 </summary>
        public int TabIndex
        {
            get { return InputBox.TabIndex; }
            set { InputBox.TabIndex = value; }
        }

        /// <summary> 自动完成类型：0：帐号，1：试卷编号 </summary>
        public int TextType { get; set; }

        /// <summary> 自动完成列表最小高度 </summary>
        public double ListMinHeight
        {
            get { return ListView.MinHeight; }
            set { ListView.MinHeight = value; }
        }

        /// <summary> 自动完成列表最大高度 </summary>
        public double ListMaxHeight
        {
            get { return ListView.MaxHeight; }
            set { ListView.MaxHeight = value; }
        }

        /// <summary> PlaceHolder </summary>
        public string PlaceHolder
        {
            get { return TextPlace.Content.ToString(); }
            set
            {
                if (string.IsNullOrWhiteSpace(PlaceHolder))
                    TextPlace.Visibility = Visibility.Hidden;
                else
                    TextPlace.Content = value;
            }
        }

        /// <summary> 是否自动获取焦点 </summary>
        public bool AutoFocus
        {
            get { return InputBox.IsFocused; }
            set { if (value) InputBox.Focus(); }
        }

        /// <summary> 是否执行自动完成 </summary>
        public bool AutoComplete
        {
            get { return _isAutoComplete; }
            set
            {
                _isAutoComplete = value;
                if (_isAutoComplete) return;
                BtnHistory.Visibility = Visibility.Hidden;
                ListView.Visibility = Visibility.Hidden;
            }
        }

        //public bool ListVisiable
        //{
        //    get { return ListView.Visibility == Visibility.Visible; }
        //    set { ListView.Visibility = (value ? Visibility.Visible : Visibility.Hidden); }
        //}

        #endregion
        public DeyiTextBox()
        {
            TextType = 0;
            InitializeComponent();
            TextPlace.Visibility = string.IsNullOrWhiteSpace(Text) ? Visibility.Visible : Visibility.Hidden;
            InputBox.TextChanged += InputBox_TextChanged;
            BtnHistory.Click += (o, args) =>
            {
                if (ListView.Visibility == Visibility.Hidden)
                    BindNum(string.Empty, true);
                else
                    ListView.Visibility = Visibility.Hidden;
            };
            InputBox.KeyUp += (o, args) =>
            {
                if (args.Key != Key.Down)
                    return;
                BindNum(Text, true);
            };
            Loaded += (o, e) =>
            {
                TextPlace.Width = InputBox.Width = ListView.Width = Width;
            };
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                InputBox.Background = new SolidColorBrush(Colors.Transparent);
                TextPlace.Visibility = Visibility.Visible;
                ListView.Visibility = Visibility.Hidden;
                return;
            }
            TextPlace.Visibility = Visibility.Hidden;
            InputBox.Background = new SolidColorBrush(Colors.White);
            BindNum(Text);
        }

        private void BindNum(string num, bool isFocus = false)
        {
            if (!_isAutoComplete)
                return;
            ListView.Items.Clear();
            List<string> list;
            using (var utils = new CacheUtils())
            {
                list = utils.Get((CacheType)TextType, num, 5);
            }
            if (list == null || !list.Any())
            {
                if (isFocus)
                {
                    ListView.Visibility = Visibility.Visible;
                    var none = new ListBoxItem
                    {
                        DataContext = 0,
                        Content = "无历史记录",
                        Foreground = new SolidColorBrush(Colors.Gray),
                        IsEnabled = false,
                        Height = 42,
                        Margin = new Thickness(10, 0, 0, 0),
                        Width = Width - 20
                    };
                    none.SetResourceReference(StyleProperty, "AutoViewItem");
                    ListView.Items.Add(none);
                }
                else
                {
                    ListView.Visibility = Visibility.Hidden;
                }
                return;
            }
            foreach (var sNum in list)
            {
                var listItem = new ListBoxItem
                {
                    DataContext = 0,
                    Content = sNum,
                    Cursor = Cursors.Hand,
                    Width = Width - 10
                };
                listItem.SetResourceReference(StyleProperty, "AutoViewItem");
                listItem.MouseLeftButtonUp += (o, arg) =>
                {
                    if (!(o is ListBoxItem)) return;
                    var text = ((ListBoxItem)o).Content.ToString();
                    if (TextType == (int)CacheType.GroupCode)
                        text = Regex.Match(text, "^(GC[0-9]+)\\[").Groups[1].Value;
                    InputBox.Text = text;
                    InputBox.Focus();
                    InputBox.Select(InputBox.Text.Length, 0);
                    ListView.Visibility = Visibility.Hidden;
                };
                ListView.Items.Add(listItem);
            }
            ListView.Visibility = Visibility.Visible;
            if (isFocus)
            {
                ListView.Focus();
                //清空
                if (string.IsNullOrWhiteSpace(num))
                {
                    var clear = new ListBoxItem
                    {
                        DataContext = 1,
                        Content = "清空记录",
                        Cursor = Cursors.Hand,
                        Foreground = new SolidColorBrush(Colors.DarkGray),
                        Width = Width - 20
                    };
                    clear.SetResourceReference(StyleProperty, "AutoViewItem");
                    clear.MouseDoubleClick += (o, args) =>
                    {
                        using (var utils = new CacheUtils())
                        {
                            utils.Clear((CacheType)TextType);
                        }
                        ListView.Items.Clear();
                        ListView.Visibility = Visibility.Hidden;
                        InputBox.Focus();
                    };
                    ListView.Items.Add(clear);
                }
            }
            ListView.KeyDown += (o, args) =>
            {
                ListPaperNum_KeyDown(args.Key);
                args.Handled = (args.Key == Key.Enter);
            };
        }

        private void ListPaperNum_KeyDown(Key key)
        {
            switch (key)
            {
                case Key.Enter:
                    var item = ListView.SelectedItem as ListBoxItem;
                    if (item == null) return;
                    if (Helper.ToInt(item.DataContext) == 1)
                    {
                        using (var utils = new CacheUtils())
                        {
                            utils.Clear(CacheType.PaperNum);
                        }
                        InputBox.Focus();
                    }
                    else
                    {
                        InputBox.Text = item.Content.ToString();
                        InputBox.Focus();
                        InputBox.Select(InputBox.Text.Length, 0);
                        ListView.Visibility = Visibility.Hidden;
                    }
                    break;
                case Key.Escape:
                    InputBox.Focus();
                    ListView.Visibility = Visibility.Hidden;
                    break;
            }
        }
    }
}
