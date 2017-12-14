using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Data;
using DayEasy.MarkingTool.Core;
using DayEasy.Models.Open.User;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DayEasy.MarkingTool.UI.Scanner
{
    /// <summary> 绑定学生 </summary>
    public partial class BindStudent : DeyiWindow
    {
        private readonly string _pictureId;
        private readonly MarkingPaper _markingPaper;
        private string _groupId;
        private readonly List<long> _studentIds;

        private enum SearchType
        {
            None = -1,
            Code = 0,
            GroupCode = 1,
            Name = 2
        }

        public BindStudent(string pictureId, MarkingPaper markingPaper, List<long> studentIds, string code = null)
        {
            InitializeComponent();
            _pictureId = pictureId;
            _markingPaper = markingPaper;
            _studentIds = studentIds ?? new List<long>();
            if (!string.IsNullOrWhiteSpace(code))
            {
                GroupCode.Text = code;
                if (code.Length == 5)
                {
                    SearchStudents(false);
                }
            }
            InitClass();
        }

        private static void InitClass()
        {
            var groups = RestHelper.Instance.TeacherGroups();
            if (groups == null || !groups.Status || !groups.Data.Any())
                return;
            using (var utils = new CacheUtils())
            {
                utils.ClearGroupCode();
                foreach (var dto in groups.Data)
                {
                    utils.Set(string.Format("{0}[{1}]", dto.Code, dto.Name), CacheType.GroupCode);
                }
            }
        }


        private void BtnGroupSearch(object sender, RoutedEventArgs e)
        {
            SearchStudents();
        }

        private SearchType CheckSearchType(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return SearchType.None;
            if (Regex.IsMatch(word, "^\\d{5,}$", RegexOptions.IgnoreCase))
                return SearchType.Code;
            if (Regex.IsMatch(word, "^GC\\d{4,}$", RegexOptions.IgnoreCase))
                return SearchType.GroupCode;
            return SearchType.Name;
        }

        /// <summary> 根据得一号搜索 </summary>
        /// <param name="code"></param>
        /// <param name="showMessage"></param>
        private void SearchByCode(string code, bool showMessage = true)
        {
            var studentResult = DeyiApp.Student(code);
            if (!studentResult.Status)
            {
                if (showMessage) WindowsHelper.ShowError(studentResult.Message);
                return;
            }
            var student = studentResult.Data;
            if (_studentIds.Contains(student.Id))
            {
                if (showMessage) WindowsHelper.ShowError("[{0}]已存在".FormatWith(student.Name));
                return;
            }
            var studentList = student.ClassList.Select(t => new StudentDto
            {
                Id = student.Id,
                Name = student.Name,
                ClassList = new Dictionary<string, string> { { t.Key, t.Value } }
            }).ToList();
            GroupName.Content = "{0}  的班级圈：".FormatWith(student.Name);
            BindStudents(studentList);
        }

        /// <summary> 根据班级圈圈号搜索 </summary>
        /// <param name="groupCode"></param>
        /// <param name="showMessage"></param>
        private void SearchByGroupCode(string groupCode, bool showMessage = true)
        {
            var students = DeyiApp.Students(groupCode);
            if (!students.Status)
            {
                if (showMessage)
                    WindowsHelper.ShowError(students.Message);
                return;
            }
            var data = students.Data;
            var list = data.Students.Where(s => !_studentIds.Contains(s.Id));
            _groupId = data.GroupId;
            using (var utils = new CacheUtils())
            {
                utils.Set(string.Format("{0}[{1}]", groupCode, data.GroupName), CacheType.GroupCode);
            }
            GroupName.Visibility = Visibility.Visible;
            GroupName.Content = data.GroupName + "  的学生列表：";
            BindStudents(list.Select(t => new StudentDto
            {
                Id = t.Id,
                Name = t.Name
            }));
        }

        private void SearchByName(string name, bool showMessage = true)
        {
            var students = RestHelper.Instance.StudentSearch(name);
            if (!students.Status)
            {
                if (showMessage)
                    WindowsHelper.ShowError(students.Message);
                return;
            }
            var data = students.Data;
            var list = data.Where(t => !_studentIds.Contains(t.Id)).ToList();
            GroupName.Visibility = Visibility.Visible;
            GroupName.Content = name + "  的相关搜索：";
            BindStudents(list.Select(t => new StudentDto
            {
                Id = t.Id,
                Name = t.Name,
                ClassList = new Dictionary<string, string> { { t.ClassId, t.Name + "," + t.ClassName + "," + t.Agency } }
            }));
        }

        private void SearchStudents(bool showMessage = true)
        {
            var code = GroupCode.Text.Trim();
            var type = CheckSearchType(code);
            if (type == SearchType.None)
            {
                if (showMessage)
                    WindowsHelper.ShowError("班级圈号/得一号/学生姓名格式不正确！");
                return;
            }
            _groupId = null;
            switch (type)
            {
                case SearchType.Code:
                    SearchByCode(code, showMessage);
                    return;
                case SearchType.GroupCode:
                    SearchByGroupCode(code, showMessage);
                    return;
                case SearchType.Name:
                    SearchByName(code, showMessage);
                    return;
            }
        }

        private void BindStudents(IEnumerable<StudentDto> students)
        {
            StudentList.Items.Clear();
            foreach (var item in students)
            {
                string textWord;
                var width = 72;
                var align = HorizontalAlignment.Center;
                if (item.ClassList != null && item.ClassList.Any())
                {
                    textWord = item.ClassList.First().Value;
                    var classInfo = textWord.Split(',');
                    if (classInfo.Length == 3)
                    {
                        width = 390;
                        textWord = classInfo[0] + "  " + classInfo[1] + "  " + classInfo[2];
                        align = HorizontalAlignment.Left;
                    }
                    else
                    {
                        width = 200;
                    }
                }
                else
                {
                    textWord = item.Name;
                }
                var listItem = new ListBoxItem
                {
                    Width = width,
                    HorizontalContentAlignment = align,
                    ToolTip = "双击绑定",
                    Content = new TextBlock
                    {
                        Text = textWord
                    },
                    DataContext = item
                };
                listItem.MouseDoubleClick += ChooseStudent;
                StudentList.Items.Add(listItem);
            }
        }

        private void ChooseStudent(object sender, MouseButtonEventArgs e)
        {
            var box = sender as ListBoxItem;
            if (box == null)
                return;
            var student = box.DataContext as StudentDto;
            if (student == null)
                return;
            if (string.IsNullOrWhiteSpace(_groupId))
            {
                _groupId = student.ClassList.First().Key;
            }
            _markingPaper.BindStudent(_pictureId, _groupId, student.Id, student.Name);
            DialogResult = true;
            Close();
        }
    }
}
