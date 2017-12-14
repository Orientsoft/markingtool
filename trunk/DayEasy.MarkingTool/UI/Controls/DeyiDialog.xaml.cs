
using DayEasy.MarkingTool.Core;
using System;
using System.Windows;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;

namespace DayEasy.MarkingTool.UI.Controls
{
    /// <summary>
    /// DeyiDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DeyiDialog
    {
        public DeyiDialog(string msg, int width, int height)
        {
            InitializeComponent();
            TextMessage.Text = msg;
            Width = width;
            Height = height;
        }

        private void AddButton(string text, string className, Thickness margin,
            Action handler = null, bool isFocus = false)
        {
            var btn = new Button
            {
                Content = text,
                Margin = margin,
                Width = 80,
                Height = 32
            };
            btn.SetResourceReference(StyleProperty, className);
            btn.Click += (o, args) =>
            {
                if (handler != null)
                    handler();
                Close();
            };
            BtnControls.Children.Add(btn);
            if (isFocus)
                btn.Focus();
        }

        public static void Alert(string msg, int width = 260, int height = 160)
        {
            var dialog = new DeyiDialog(msg, width, height);
            dialog.AddButton("确认", "Btn-Primary", new Thickness(0, 0, 15, 0), null, true);
            dialog.DeyiDialog();
        }

        public static bool Confirm(string msg, string yesBtn = "确　定", string noBtn = "取　消", int width = 260, int height = 160)
        {
            var dialog = new DeyiDialog(msg, width, height);
            var result = false;
            dialog.AddButton(yesBtn, "Btn-Primary", new Thickness(0), () => { result = true; }, true);
            dialog.AddButton(noBtn, "Btn-Default", new Thickness(15, 0, 15, 0), () => { result = false; });
            dialog.DeyiDialog();
            return result;
        }

        public static bool? SureOrCancel(string msg, string yesBtn = "是", string noBtn = "否",
            string cancelBtn = "取消", int width = 320, int height = 160)
        {
            var dialog = new DeyiDialog(msg, width, height);
            bool? result = null;
            dialog.AddButton(yesBtn, "Btn-Success", new Thickness(0), () => { result = true; }, true);
            dialog.AddButton(noBtn, "Btn-Info", new Thickness(15, 0, 0, 0), () => { result = false; });
            dialog.AddButton(cancelBtn, "Btn-Default", new Thickness(15, 0, 15, 0), () => { result = null; });
            dialog.DeyiDialog();
            return result;
        }

        private void TitleMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
