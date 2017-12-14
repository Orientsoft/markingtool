using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.Core;
using System;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZXing;
using ZXing.Common;
using Path = System.Windows.Shapes.Path;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// PrintPaperTemplate.xaml 的交互逻辑
    /// </summary>
    public partial class PrintPaperTemplate
    {
        public PrintPaperTemplate()
        {
            InitializeComponent();
            //DrawRectangle();
            TxtIdNo.Focus();
        }

        private void DrawRectangle()
        {
            var rectangle = new RectangleGeometry {Rect = new Rect(45, 16, 60, 60)};
            var path = new Path {Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 1, Data = rectangle};
            CvsPrintArea.Children.Add(path);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtIdNo.Text))
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

            var arr = TxtIdNo.Text.Split(new[] { ",", "，" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 3)
            {
                MessageBox.Show("输入顺序：真实姓名,身份证号,学号");
                return;
            }

            LblCode.Content = arr[2];
            LblName.Content = arr[0];

           

            try
            {
                var writer = new BarcodeWriter();
                writer.Format = BarcodeFormat.QR_CODE;
                writer.Options = options;
                using (var bmp = writer.Write(Helper.DesEncrypt(string.Join(",", arr))))
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        var img = new ImageBrush();
                        var converter = new ImageSourceConverter();
                        img.ImageSource = (ImageSource)converter.ConvertFrom(ms);
                        ImgStudentIdNo.Source = img.ImageSource;
                        var dialog = new PrintDialog();

                        if (dialog.ShowDialog() == true)
                        {
                            dialog.PrintVisual(CvsPrintArea, "正在打印作业纸模板");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WindowsHelper.ShowError(ex.Message);
            }
        }

        private void btnPrint2_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Helper.DesDecrypt(TxtIdNo2.Text));
        }
    }
}
