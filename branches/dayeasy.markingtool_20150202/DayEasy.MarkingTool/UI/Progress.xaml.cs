using DayEasy.MarkingTool.BLL.Steps;
using DayEasy.MarkingTool.BLL.Steps.Result;
using DayEasy.Open.Model.Paper;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// Progress.xaml 的交互逻辑
    /// </summary>
    public partial class Progress
    {
        public Progress()
        {
            InitializeComponent();
        }

        public override void EndInit()
        {
            base.EndInit();
            Loaded += Progress_Loaded;
        }

        void Progress_Loaded(object sender, RoutedEventArgs e)
        {
            Porgess();
            BtnNext.IsEnabled = false;
        }



        public void SetPaper(List<string> paperImages, PaperInfo origPaper)
        {
            _paperPath = paperImages;
            _origPaper = origPaper;
        }


        List<string> _paperPath = null;
        PaperInfo _origPaper = null;

        readonly IStep[] _steps = new IStep[]
        {
             new CuttingStage(),
             new BasicInfoScaningStage(),
             new ChoiceSubjectScaningStage(),
             //new PacketingStage(),
             //new StatisticsStage()
        };


        private void Porgess()
        {
            if (null == _paperPath || _paperPath.Count < 1)
            {
                return;
            }

            PBar.Maximum = _paperPath.Count * 3;
            //tbMsg.AppendText();
            StepResult result = null;
            foreach (var item in _paperPath)
            {
                result = _steps[0].Process(item);
                if (result.IsSuccess)
                {
                    ShowMessage(string.Format("图片分析{0}:处理成功...", item));
                }
                else
                {
                    ShowMessage(string.Format("图片分析{0}:出现错误...", result.Exception.Message));
                }

                result = _steps[1].Process(item);
                if (result.IsSuccess)
                {
                    var temp = (result as InfomationScaningResult);
                    ShowMessage(string.Format("扫描试卷信息,学号:{0}", temp.ScanResult.Id));

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
            this.Dispatcher.Invoke(new Action(() => { TbMsg.AppendText(string.Format("{0}{1}", msg, Environment.NewLine)); PBar.Value += 1; }));
        }
    }
}
