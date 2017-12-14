﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.MarkingTool.Core;
using DayEasy.Models.Open.Paper;

namespace DayEasy.MarkingTool.UI.Print
{
    /// <summary>
    /// 试卷列表
    /// </summary>
    public partial class BatchList
    {
        private readonly MPaperDto _paperInfo;
        private readonly ObservableCollection<PrintBatchInfo> _list = new ObservableCollection<PrintBatchInfo>();

        public BatchList(MPaperDto paperInfo)
        {
            CloseWindow = true;
            InitializeComponent();
            _paperInfo = paperInfo;
            InitData();
            Loaded += BatchPrint_Loaded;

        }

        private void InitData()
        {
            var isPaperAb = (_paperInfo.PaperType == (byte)PaperType.PaperAb);
            if (isPaperAb)
            {
                LblType.Content = "AB卷";
            }
            var list = RestHelper.Instance.BatchPrints(_paperInfo.Id);
            if (list != null && list.Status)
            {
                var source = list.Data.OrderByDescending(t => t.MarkingTime).ToList();
                source.ForEach(
                    t =>
                    {
                        t.PaperAb = isPaperAb;
                        t.NormalPaper = !isPaperAb;
                        _list.Add(t);
                    });
                List.ItemsSource = _list;
            }
            LblPaperTitle.Text = _paperInfo.PaperTitle;
        }

        private void PrintEvent(PrintBatchInfo batchInfo, byte sectionType)
        {
            var print = new PrintList(_paperInfo, batchInfo.Batch, batchInfo.GroupName, sectionType);
            print.DeyiWindow(this);
        }

        #region 按钮/窗口事件
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow = false;
            Close();
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

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
                return;
            var batch = btn.Tag.ToString();
            var sectionType = Helper.ToByte(btn.Uid);
            var batchInfo = _list.FirstOrDefault(t => t.Batch == batch);
            PrintEvent(batchInfo, sectionType);
        }

        #endregion

        private void JointPrintClick(object sender, RoutedEventArgs e)
        {
            var joint = new JointPrintList(_paperInfo);
            joint.DeyiWindow(this);
        }
    }
}