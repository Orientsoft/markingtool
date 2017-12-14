using Deyi.Tool.Common;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using IO = System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZXing;
using ZXing.Common;

namespace Deyi.Tool
{
    /// <summary>
    /// PrintPaperTemplate.xaml 的交互逻辑
    /// </summary>
    public partial class PrintPaperTemplate : Window
    {
        public PrintPaperTemplate()
        {
            InitializeComponent();
            //DrawRectangle();
            txtIDNo.Focus();
        }

        private void DrawRectangle()
        {
            var rectangle = new RectangleGeometry();
            rectangle.Rect = new Rect(45, 16, 60, 60);
            var path = new Path();
            path.Fill = Brushes.White;
            path.Stroke = Brushes.Black;
            path.StrokeThickness = 1;
            path.Data = rectangle;
            cvsPrintArea.Children.Add(path);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIDNo.Text))
            {
                MessageBox.Show("身份证号码不能为空");
                return;
            }

            var options = new EncodingOptions()
            {
                Width = 80,
                Height = 80,
                Margin = 1,
                PureBarcode = true
            };

            var arr = txtIDNo.Text.Split(new string[] { ",", "，" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 3)
            {
                MessageBox.Show("输入顺序：真实姓名,身份证号,学号");
                return;
            }

            lblCode.Content = arr[2];
            lblName.Content = arr[0];

           

            try
            {
                var writer = new BarcodeWriter();
                writer.Format = BarcodeFormat.QR_CODE;
                writer.Options = options;
                using (var bmp = writer.Write(Helper.DESEncrypt(string.Join(",", arr))))
                {
                    using (var ms = new IO.MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        var img = new ImageBrush();
                        var converter = new ImageSourceConverter();
                        img.ImageSource = (ImageSource)converter.ConvertFrom(ms);
                        imgStudentIDNo.Source = img.ImageSource;
                        var dialog = new PrintDialog();

                        if (dialog.ShowDialog() == true)
                        {
                            dialog.PrintVisual(cvsPrintArea, "正在打印作业纸模板");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.ShowError(ex.Message);
            }
        }

        private void btnPrint2_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Helper.DESDecrypt(txtIDNo2.Text));
        }
    }
}
