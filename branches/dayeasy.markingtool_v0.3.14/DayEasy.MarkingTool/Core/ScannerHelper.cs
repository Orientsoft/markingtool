using System.Diagnostics;
using System.IO;
using AxMTKTWOCXLib;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using Microsoft.Win32;

namespace DayEasy.MarkingTool.Core
{
    /// <summary>
    /// 扫描仪相关辅助类
    /// </summary>
    public class ScannerHelper : IDisposable
    {
        private static string _prefix;
        private static AxMTKTWOCX _amx;
        private static int _scannerType = 0;
        private readonly List<string> _scannerList;

        public ScannerHelper(Grid parentGrid, string prefix, int scannerType = 0)
        {
            _scannerType = scannerType;
            _prefix = prefix;
            // 创建OCX的对象
            if (_amx == null)
            {
                _amx = new AxMTKTWOCX();
            }
            var host = new WindowsFormsHost { Child = _amx, Width = 0, Height = 0 };
            parentGrid.Children.Add(host);
            Grid.SetRow(host, 0);
            _scannerList = new List<string>();

        }

        public List<string> Scnaner()
        {
            _amx.BeginInit();
            _amx.PostScanEveryPage += _ax_PostScanEveryPage;
            //打开扫描仪
            if (!OpenScanner())
                return new List<string>();
            if (!_scannerList.Any())
            {
                SetDefaultForScan();
            }
            SetNamingRule();
            //_amx.Show();
            _amx.Scan(-1, 0);
            _amx.CloseScanner();
            return _scannerList;
        }

        private bool OpenScanner()
        {
            if (_amx.OpenScanner() == 1) return true;
            WindowsHelper.ShowError("连接扫描仪失败！");
            return false;
        }

        /// <summary>
        /// 检查Ocx组件
        /// </summary>
        /// <returns></returns>
        public static bool CheckOcx()
        {
            const string id = "{8D0E432E-09CE-41E8-9305-0D822A00090D}";
            RegistryKey rkTest = Registry.ClassesRoot.OpenSubKey("TypeLib\\" + id + "\\");
            return rkTest != null;
        }

        /// <summary>
        /// 注册组件
        /// </summary>
        public static bool RegistOcx()
        {
            if (!WindowsHelper.ShowQuestion("是否先注册扫描仪组件？"))
                return false;
            var path = Path.Combine(DeyiKeys.CurrentDir, "config", "ocx", "MTKTWOCX.ocx");
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "regsvr32",
                    Arguments = " /s " + path
                }
            };
            process.Start();
            process.WaitForExit();
            process.Close();
            process.Dispose();
            return true;
        }

        private void _ax_PostScanEveryPage(object sender, _DMTKTWOCXEvents_PostScanEveryPageEvent e)
        {
            if (e.bSuccess != 1) return;
            var path = _amx.GetCurrentScanImagePath();
            if (!_scannerList.Contains(path))
                _scannerList.Add(path);
        }

        /// <summary>
        /// 设置扫描仪默认配置
        /// </summary>
        private static void SetDefaultForScan()
        {
            _amx.DSMode = 0;
            _amx.SetScanImageLayout(0, 0, 8.27, 11.69);
            //_amx.SetScanImageLayout(0, 0, 21, 29.7);
            //单面
            if (_scannerType == 0)
            {
                //扫描仪类型
                //0 Flatbed 平板扫描仪
                //1 ADF  
                //2 ADF duplex
                _amx.ScanSourceType = 1;

                //合并页面
                //0：不合并页面
                //1：合并页面
                _amx.CombineImages = 0;
                _amx.ShowUI = 0;
            }
            else
            {
                //双面
                _amx.ScanSourceType = 2;
                _amx.CombineImages = 1;
                //合并页面的类别
                //0：第一张图在最上面。
                //1：第一张图在最下面。
                //2：第一张图在左边。
                //3：第一张图在右边。
                //（该属性只有在CombineImages属性设为1才生效）
                _amx.CombineImagesDirection = 0;
                _amx.ShowUI = 1;
            }

            //0:bmp
            //1:jpg
            //2.单页tif
            //3.多页tif
            //4.单页pdf
            //5.多页pdf
            //6.可检索的单页pdf，需要支持OCR识别。
            //7.可检索的多页pdf，需要支持OCR识别。
            _amx.ImageFormat = 1;

            // 是否开启自动纠偏。
            //1：开启。
            //0：不开启。
            _amx.AutoDeskew = 1;
            //自动节选
            _amx.AutoCrop = 1;
            //自动出去空白页
            _amx.AutoDiscardBlankPages = 1;

            //0  BW1bpp  黑白二值图
            //1  Gray8bpp 8位灰度图
            //2  RGB24bpp  24 位RGB图
            _amx.ScanPixelType = 1;

            //设置对比度 -100-100
            _amx.Contrast = 15;

            //设置亮度-100-100
            _amx.Brightness = 15;

            //压缩质量1-100
            _amx.CompressionRate = 15;

            //语言类型
            //0：中文简体
            //1：繁体中文
            //2:  英文
            _amx.LanguageType = 0;

            //阀值
            _amx.SetScanThreshold(128);

            //设置扫描分辨300dpi
            _amx.ScanResolution = 300;

            //0 反射稿 (普通纸张)
            //1 透明稿（胶片、X-Ray）
            _amx.Material = 0;

            //扫描仪影像输出可设RGB&BW 与 Gray&BW
            _amx.MultiStream = 1;
            _amx.SetMultiStreamOrder(2);
            _amx.SetMultiStreamResolution(0);
        }

        /// <summary>
        /// 设置命名规则
        /// </summary>
        /// <param name="nStartIndex"></param>
        /// <param name="indexLenght"></param>
        private static void SetNamingRule(int nStartIndex = 1, int indexLenght = 4)
        {
            _amx.SetImageName(Helper.CreateSavePath(), _prefix, nStartIndex, indexLenght);
        }

        public void Dispose()
        {
            if (_amx != null)
                _amx.Dispose();
        }
    }
}
