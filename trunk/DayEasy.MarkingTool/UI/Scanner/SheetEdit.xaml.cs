using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DayEasy.MarkingTool.UI.Scanner
{
    /// <summary> 答题卡修改 </summary>
    public partial class SheetEdit
    {
        private readonly string _markId;
        private readonly List<ObjectiveItem> _objectives;
        private readonly IList<int[]> _sheets;
        private List<TextBox> _sheetList;

        private List<TextBox> SheetTexts
        {
            get
            {
                if (_sheetList != null && _sheetList.Any())
                    return _sheetList;
                _sheetList = FindChilds<TextBox>(SheetList, string.Empty);
                return _sheetList;
            }
        }

        public class SheetItem
        {
            public string Sort { get; set; }
            public string Answer { get; set; }
            public string Color { get; set; }
        }

        public SheetEdit(string markId, IList<int[]> sheets, string name, List<ObjectiveItem> objectives)
        {
            InitializeComponent();
            Title = string.Concat(name, " - 客观题修改");
            _markId = markId;
            _objectives = objectives;
            _sheets = sheets;
            var rows = (int)Math.Ceiling(objectives.Count / 6D);
            Height = rows * 32 + 100;
            InitSheets();
        }

        private void InitSheets()
        {
            var data = new List<SheetItem>();
            var index = 0;
            foreach (var item in _objectives)
            {
                var sheet = _sheets[index];
                var answer = sheet.ToAnswer();
                data.Add(new SheetItem
                {
                    Sort = item.Sort,
                    Answer = answer,
                    Color = (sheet.All(t => t < 0) || (item.Single && sheet.Count() > 1)) ? "Red" : "Gray"
                });
                index++;
            }
            SheetList.Items.Clear();
            SheetList.ItemsSource = data;
        }

        private void BtnSave(object sender, RoutedEventArgs e)
        {
            SetSheet();
        }

        private void SetSheet()
        {
            var markingPaper = Owner as MarkingPaper;
            if (markingPaper == null)
            {
                WindowsHelper.ShowError("保存失败~！");
                return;
            }
            var text = string.Join(",", SheetTexts.Select(t => t.Text));
            var sheets = text.ToSheet();
            if (sheets.Count != _objectives.Count)
            {
                WindowsHelper.ShowError("答案数量不匹配，请检查~！");
                return;
            }
            markingPaper.SetSheets(_markId, sheets);
            Close();
        }

        private static List<T> FindChilds<T>(DependencyObject obj, string name)
            where T : FrameworkElement
        {
            var childList = new List<T>();
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                var item = child as T;
                if (item != null && (item.Name == name || string.IsNullOrEmpty(name)))
                {
                    childList.Add(item);
                }
                childList.AddRange(FindChilds<T>(child, name)); //指定集合的元素添加到List队尾  
            }
            return childList;
        }

        private void SheetKeyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
                return;
            if (e.Key != Key.Enter)
                return;
            var item = sender as TextBox;
            if (item == null) return;
            var list = SheetTexts;
            var index = list.IndexOf(item);
            if (index < list.Count - 1) index++;
            var next = list[index];
            next.Focus();

        }

        private void SheetFocus(object sender, RoutedEventArgs e)
        {
            var item = sender as TextBox;
            if (item == null) return;
            item.Select(0, item.Text.Length);
        }

        private void SheetChanged(object sender, RoutedEventArgs e)
        {
            var item = sender as TextBox;
            if (item == null) return;
            var text = Regex.Match(item.Text, "([a-z]+)", RegexOptions.IgnoreCase).Groups[1].Value;
            item.Text = text.ToUpper();
            var list = SheetTexts;
            var index = list.IndexOf(item);
            var objective = _objectives[index];
            var color = (string.IsNullOrWhiteSpace(text) || (objective.Single && text.Length > 1)) ? Colors.Red : Colors.Gray;
            item.BorderBrush = new SolidColorBrush(color);
        }
    }
}
