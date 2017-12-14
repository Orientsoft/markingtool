using System.Windows.Media;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using PrintPaperTemplate.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using PrintDialog = System.Windows.Controls.PrintDialog;

namespace PrintPaperTemplate
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _take;
        private int _size;
        private PrintDialog _printDialog = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private HSSFWorkbook _hssfworkbook;



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = @"Excel 文件|*.xls";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtPath.Text = dialog.FileName;
                    InitializeSheet();
                }
            }
        }

        private void InitializeWorkbook(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                _hssfworkbook = new HSSFWorkbook(file);
            }

        }

        public void InitializeSheet()
        {
            if (string.IsNullOrWhiteSpace(txtPath.Text))
            {
                Helper.ShowError(@"请选择路径！");
                return;
            }
            try
            {
                InitializeWorkbook(txtPath.Text);
            }
            catch (Exception ex)
            {
                Helper.ShowError(ex.Message);
                return;
            }
            if (_hssfworkbook.NumberOfSheets == 0)
            {
                Helper.ShowError(@"没有找到Excel的书签！");
                return;
            }

            var listSheet = new Dictionary<int, string>();

            for (int i = 0; i < _hssfworkbook.NumberOfSheets; i++)
            {
                listSheet.Add(i + 1, _hssfworkbook.GetSheetName(i));
            }

            cbSheet.ItemsSource = listSheet;
            cbSheet.SelectedValuePath = "Key";
            cbSheet.DisplayMemberPath = "Value";
            cbSheet.SelectedValue = 1;
        }

        private void print_start_Click(object sender, RoutedEventArgs e)
        {
            if (cbSheet.SelectedIndex == -1)
            {
                Helper.ShowError(@"请选择工作表");
                return;
            }
            _take = Helper.ToInt(txtStudentCount.Text);
            if (_take < 1)
            {
                Helper.ShowError(@"人数必须为数字！");
                return;
            }
            _size = Helper.ToInt(((ComboBoxItem) PrintCount.SelectedItem).Content.ToString());
            if (_size < 1)
            {
                Helper.ShowError(@"份数必须为数字！");
                return;
            }

            InitPrint(true);

            var index = int.Parse(cbSheet.SelectedValue.ToString());
            // cbSheet.IsEnabled = false;
            try
            {
                var sheet = _hssfworkbook.GetSheetAt(index - 1);
                var list = ConvertToList(sheet);
                if (!list.Any())
                {
                    Helper.ShowError("没有获取到学生信息，请确保Excel中包括“学生ID”、“学生姓名”、“班级ID”和“用户帐号”四列！");
                    return;
                }

                if (list != null && list.Count > 0)
                {
                    Print(list);
                }
            }
            catch
            {
                InitPrint(false);
                Helper.ShowError(@"Excel表信息有误！~");
            }
        }

        private void InitPrint(bool isStart)
        {
            print_start.IsEnabled = !isStart;
            print_cancel.IsEnabled = isStart;
        }

        private List<Student> ConvertToList(ISheet sheet)
        {
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

            var list = new List<Student>();
            rows.MoveNext();
            while (rows.MoveNext())
            {
                var row = (HSSFRow) rows.Current;
                if (row.LastCellNum < 4)
                    break;
                var idCell = row.GetCell(0);
                long id = (idCell.CellType == CellType.NUMERIC
                    ? Convert.ToInt64(idCell.NumericCellValue)
                    : Helper.ToLong(idCell.StringCellValue));
                if (id <= 0) continue;
                list.Add(new Student
                {
                    Id = id,
                    Name = row.GetCell(1).StringCellValue,
                    Email = row.GetCell(2).StringCellValue,
                    ClassId = row.GetCell(3).StringCellValue
                });
                if (list.Count >= _take)
                    break;
            }
            return list;
        }

        private void Print(List<Student> stus)
        {
            _printDialog = new PrintDialog();
            if (!_printDialog.ShowDialog().GetValueOrDefault())
                return;
            try
            {
                //var vises = PrintHelper.PrintingTemplates(stus, _size);
                var vises = PrintHelper.PrintTemplates(stus, _size);
#if !DEBUG
                var doc = new VisualDocumentPaginator(vises);
                doc.SetSize(100, 70);
                _printDialog.PrintDocument(doc, "正在打印...");
#endif
#if DEBUG
                for (var i = 0; i < vises.Children.Count; i++)
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("test{0}.png", i));
                    Helper.SaveVisual((DrawingVisual)vises.Children[i], path);
                }
#endif
            }
            catch (Exception e)
            {
                Helper.ShowError(e.Message);
            }
            finally
            {
                InitPrint(false);
            }
        }

        private void print_cancel_Click_1(object sender, RoutedEventArgs e)
        {
            if (_printDialog != null && _printDialog.PrintQueue.IsBusy)
            {
                _printDialog.PrintQueue.Pause();
                _printDialog.PrintQueue.Purge();
            }
            InitPrint(false);
        }
    }
}
