using Deyi.Tool.Step;
using Deyi.Tool.Steps;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Deyi.Tool
{
    /// <summary>
    /// Progress.xaml 的交互逻辑
    /// </summary>
    public partial class Progress : Window
    {
        public Progress()
        {
            InitializeComponent();
        }

        public override void EndInit()
        {
            base.EndInit();
            this.Loaded += Progress_Loaded;
        }

        void Progress_Loaded(object sender, RoutedEventArgs e)
        {
            Porgess();
            btnNext.IsEnabled = false;
        }



        public void SetPaper(List<string> paperImages, PaperServiceReference.PaperDetail origPaper)
        {
            this.paperPath = paperImages;
            this.origPaper = origPaper;
        }


        List<string> paperPath = null;
        PaperServiceReference.PaperDetail origPaper = null;

        IStep[] steps = new IStep[]
        {
             new CuttingStage(),
             new BasicInfoScaningStage(),
             new ChoiceSubjectScaningStage(),
             //new PacketingStage(),
             //new StatisticsStage()
        };


        private void Porgess()
        {
            if (null == paperPath || paperPath.Count < 1)
            {
                return;
            }

            pBar.Maximum = paperPath.Count * 3;
            //tbMsg.AppendText();
            StepResult result = null;
            foreach (var item in paperPath)
            {
                result = steps[0].Process(item);
                if (result.IsSuccess)
                {
                    ShowMessage(string.Format("图片分析{0}:处理成功...", item));
                }
                else
                {
                    ShowMessage(string.Format("图片分析{0}:出现错误...", result.Exception.Message));
                }

                result = steps[1].Process(item);
                if (result.IsSuccess)
                {
                    var temp = (result as InfomationScaningResult);
                    ShowMessage(string.Format("扫描试卷信息,学号:{0},试卷批次号:{1}", temp.ScanResult["StudentNo"], temp.ScanResult["BatchCode"]));

                }
                else
                {
                    ShowMessage(string.Format("扫描试卷信息出现错误:{0}", result.Exception.Message));
                    continue;
                }
            }

        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {

            //result = steps[2].Process(item.Key, OrigPaper);

            //if (result.IsSuccess)
            //{
            //    ShowMessage(item.Key);
            //}
            //else
            //{
            //    ShowMessage(result.Exception.Message);
            //}


        }

        private void ShowMessage(string msg)
        {
            this.Dispatcher.Invoke(new Action(() => { tbMsg.AppendText(string.Format("{0}{1}", msg, Environment.NewLine)); pBar.Value += 1; }));
        }
    }
}
