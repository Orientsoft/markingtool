using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace DayEasy.MarkingTool.Core
{
    public class SortableListView : ListView
    {
        //最后单击的标题
        private GridViewColumnHeader _lastHeaderClicked;
        //默认升序
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private string _lastSortBy = string.Empty;

        //排序字段的附加属性
        public static readonly DependencyProperty SortPropertyNameProperty =
            DependencyProperty.RegisterAttached("SortPropertyName", typeof(string), typeof(SortableListView));
        // 附加属性的Get方法
        public static string GetSortPropertyName(GridViewColumn obj)
        {
            return (string)obj.GetValue(SortPropertyNameProperty);
        }
        //附加属性的Set方法
        public static void SetSortPropertyName(GridViewColumn obj, string value)
        {
            obj.SetValue(SortPropertyNameProperty, value);
        }

        public SortableListView()
        {
            //注册单击标题事件
            AddHandler(
                ButtonBase.ClickEvent,
                new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
            PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = MouseWheelEvent,
                    Source = sender
                };
                RaiseEvent(eventArg);
            };
        }

        public void SetDefault(string sortBy, ListSortDirection direction = ListSortDirection.Ascending)
        {
            _lastSortBy = sortBy;
            _lastDirection = direction;
        }

        public event SourceSort SortHandler;

        public delegate void SourceSort(SortDescription sort);

        /// <summary>
        /// 排序方法
        /// </summary>
        /// <param name="sortBy">排序字段</param>
        /// <param name="direction">升序/降序</param>
        private void Sort(string sortBy, ListSortDirection direction)
        {
            var dataView =
                CollectionViewSource.GetDefaultView(ItemsSource);
            if (dataView == null) return;
            dataView.SortDescriptions.Clear();
            var sort = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sort);
            dataView.Refresh();
            if (SortHandler != null)
                SortHandler(sort);
        }

        public void Sort()
        {
            if (!string.IsNullOrWhiteSpace(_lastSortBy))
                Sort(_lastSortBy, _lastDirection);
        }

        /// <summary> 单击标题调用的方法 </summary>
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            if (headerClicked == null || headerClicked.Role == GridViewColumnHeaderRole.Padding)
                return;
            //获得排序字段
            var sortBy = GetSortPropertyName(headerClicked.Column);
            if (sortBy == null)
                return;
            ListSortDirection direction;
            if (!headerClicked.Equals(_lastHeaderClicked))
            {
                direction = ((sortBy.Equals("index", StringComparison.CurrentCultureIgnoreCase) &&
                              _lastHeaderClicked == null)
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending);
                if (_lastHeaderClicked != null)
                    _lastHeaderClicked.SetResourceReference(StyleProperty, "Sort-Base");
            }
            else
            {
                direction = (_lastDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending);
            }
            headerClicked.SetResourceReference(StyleProperty,
                direction == ListSortDirection.Ascending ? "Sort-Asc" : "Sort-Desc");
            if (string.IsNullOrEmpty(sortBy))
            {
                sortBy = headerClicked.Column.Header as string;
            }
            //排序操作
            Sort(sortBy, direction);

            _lastHeaderClicked = headerClicked;
            _lastDirection = direction;
            _lastSortBy = sortBy;
        }
    }
}