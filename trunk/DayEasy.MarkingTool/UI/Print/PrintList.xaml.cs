using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.MarkingTool.Core;
using DayEasy.Models.Open.Paper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DayEasy.MarkingTool.UI.Print
{
    /// <summary> 套打列表 </summary>
    public partial class PrintList
    {
        private readonly string _batch;
        private readonly string _groupName;
        private readonly bool _isJoint;
        private readonly byte _sectionType;
        private readonly MPaperDto _paperInfo;
        private int _skip;
        private int _size = 50;
        private readonly ObservableCollection<PrintBatchDetail> _details = new ObservableCollection<PrintBatchDetail>();

        public PrintList(MPaperDto info, string batch, string groupName, byte sectionType, bool isJoint = false)
        {
            CloseWindow = true;
            InitializeComponent();
            _batch = batch;
            _groupName = groupName;
            _isJoint = isJoint;
            if (!isJoint)
                _size = 100;
            _sectionType = sectionType;
            _paperInfo = info;
            InitData();
            Loaded += BatchPrint_Loaded;
            List.MouseDoubleClick += (o, args) =>
            {
                var item = List.SelectedItem as PrintBatchDetail;
                if (item == null)
                    return;
                var result = ShowMarkingDetail(_details.IndexOf(item));
                if (result.HasValue && result.Value)
                {
                    List.ScrollIntoView(item);
                }
            };
            //排序
            List.SortHandler += sort =>
            {
                if (string.IsNullOrWhiteSpace(sort.PropertyName))
                    return;
                var sorts = new[] { "StudentName", "Score", "Index" };
                if (!sorts.Contains(sort.PropertyName))
                    return;
                Func<PrintBatchDetail, object> orderBy;
                switch (sort.PropertyName)
                {
                    case "StudentName":
                        orderBy = t => t.StudentName;
                        break;
                    case "Score":
                        orderBy = t => t.Score;
                        break;
                    default:
                        orderBy = t => t.Index;
                        break;
                }
                var tempList = (sort.Direction == ListSortDirection.Ascending
                    ? _details.OrderBy(orderBy)
                    : _details.OrderByDescending(orderBy)).ToList();
                _details.Clear();
                tempList.ForEach(t => _details.Add(t));
            };
        }

        private void AgencyList()
        {
            if (!_isJoint) return;
            var agencyResult = RestHelper.Instance.JointAgencies(_batch);
            if (agencyResult.Status && agencyResult.Data.Count > 1)
            {
                foreach (var agency in agencyResult.Data)
                {
                    agencies.Items.Add(new ComboBoxItem
                    {
                        Content = agency.Value,
                        DataContext = agency.Key
                    });
                }
                agencies.Visibility = Visibility.Visible;
                agencies.SelectionChanged += (sender, e) =>
                {
                    _skip = 0;
                    LoadDetails();
                };
            }
        }

        private void LoadDetails()
        {
            _details.Clear();
            string id = string.Empty;
            if (_isJoint)
            {
                var combo = agencies.SelectedItem as ComboBoxItem;
                if (combo != null)
                    id = combo.DataContext.ToString();
            }
            var list = _isJoint
                ? RestHelper.Instance.JointPrintDetails(_batch, _sectionType, _skip, _size, id)
                : RestHelper.Instance.PrintDetails(_batch, _sectionType, _skip, _size);
            if (list.Status)
            {
                BtnNext.IsEnabled = list.Count > _skip + _size;
                BtnPrev.IsEnabled = _skip > 0;
                if (list.Count < _size)
                    _size = list.Count;
                var printDetails = list.Data.OrderBy(t => t.Index).ToList();
                printDetails.ForEach(t =>
                {
                    t.Index += _skip;
                    _details.Add(t);
                });
                if (_details.Any())
                    BtnPrint.IsEnabled = true;
                List.ItemsSource = _details;
            }
            TxtSkip.Text = _skip.ToString();
            TxtSize.Text = _size.ToString();
        }

        private void InitData()
        {
            AgencyList();
            LoadDetails();
            LblPaperTitle.Text = _paperInfo.PaperTitle;
            LblClass.Text = _groupName;
            if (_sectionType > 0)
            {
                LblType.Content = ((ScannerType)_sectionType).GetText();
            }
            if (!_details.Any(t => t.PageCount > 1))
                return;
            PrintType.Visibility = Visibility.Visible;
        }

        private bool? ShowMarkingDetail(int index)
        {
            var count = _details.Count;
            var dWin = new PrintDetail(_groupName, _paperInfo.PaperTitle, index, count);
            dWin.ObtianDirctoryHandler += dWin_ObtianDirctoryHandler;
            return dWin.DeyiDialog(this);
        }

        PrintBatchDetail dWin_ObtianDirctoryHandler(int index)
        {
            int count = _details.Count;
            if (index < 0) index = count - 1;
            if (index > count - 1)
                index = 0;
            return _details[index];
        }

        #region 按钮/窗口事件

        /// <summary>
        /// 套打
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
                return;
            var item = _details.First(t => t.Id == btn.Tag.ToString());
            if (item == null)
                return;
            var result = WindowsHelper.PrintDetails(new List<PrintBatchDetail> { item });
            if (result.Status)
                item.IsPrint = true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow = false;
            Close();
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
                return;
            var item = _details.FirstOrDefault(t => t.Id == btn.Tag.ToString());
            if (item != null)
            {
                ShowMarkingDetail(_details.IndexOf(item));
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowsHelper.ShowQuestion("确认删除该套打记录？"))
                return;
            var btn = sender as Button;
            if (btn == null)
                return;
            var id = btn.Tag.ToString();
            var item = _details.FirstOrDefault(t => t.Id == id);
            if (item != null)
            {
                _details.Remove(item);
            }
        }

        /// <summary>
        /// 窗体加载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BatchPrint_Loaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
                Owner.Hide();
        }

        private void BtnPrintList_Click(object sender, RoutedEventArgs e)
        {
            bool? isOdd = null;
            var type = Helper.ToInt(((ComboBoxItem)PrintType.SelectedItem).DataContext);
            if (type > 0)
                isOdd = (type == 1);
            BatchPrintMission(isOdd);
        }

        private void BatchPrintMission(bool? isOdd = null)
        {
            var list = _details.Where(t => t.IsChecked).ToList();
            if (!list.Any())
            {
                WindowsHelper.ShowError("请至少选择一条记录！");
                return;
            }
            //            WindowsHelper.ShowMsg(
            //                JsonHelper.ToJson(list.Select(t => new { t.Index, t.StudentName }), NamingType.CamelCase, true), 1000, 500);
            //            return;
            var result = WindowsHelper.PrintDetails(list, isOdd);
            if (!result.Status)
                return;
            var prints = result.Data.ToList();
            foreach (var detail in _details)
            {
                if (prints.Contains(detail.Id))
                    detail.IsPrint = true;
            }
        }

        private void CheckAll(object sender, RoutedEventArgs e)
        {
            var box = sender as CheckBox;
            if (box == null)
                return;
            foreach (PrintBatchDetail detail in _details)
            {
                detail.IsChecked = box.IsChecked.HasValue && box.IsChecked.Value;
            }
        }

        private void CheckItem(object sender, RoutedEventArgs e)
        {
            var box = sender as CheckBox;
            if (box == null) return;
            var id = box.Tag.ToString();
            var item = _details.First(t => t.Id == id);
            item.IsChecked = box.IsChecked.HasValue && box.IsChecked.Value;
        }

        #endregion

        private void LoadDetailClick(object sender, RoutedEventArgs e)
        {
            _skip = Helper.ToInt(TxtSkip.Text);
            _size = Helper.ToInt(TxtSize.Text, 50);
            LoadDetails();
            List.Sort();
        }

        private void NextPageClick(object sender, RoutedEventArgs e)
        {
            _skip += _size;
            TxtSkip.Text = _skip.ToString();
            LoadDetails();
            List.Sort();
        }

        private void PrevPageClick(object sender, RoutedEventArgs e)
        {
            _skip -= _size;
            if (_skip < 0)
                _skip = 0;
            TxtSkip.Text = _skip.ToString();
            LoadDetails();
            List.Sort();
        }
    }
}
