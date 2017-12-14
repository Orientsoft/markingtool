using System;
using System.Drawing;
using System.IO;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Scanners;
using DayEasy.MarkingTool.BLL.Steps.Result;

namespace DayEasy.MarkingTool.BLL.Steps
{
    /// <summary>
    /// 获取线条位置
    /// </summary>
    public class FindLineStage : IStep
    {
        IScanner _scanner;
        Image _paperImage;

        private StepResult PreAction(params object[] args)
        {
            var fileName = (String)args[0];
            if (!File.Exists(fileName))
            {
                return new StepResult(false, new FileNotFoundException(string.Format("没有找到文件:{0}", fileName)));
            }
            _paperImage = Image.FromFile(fileName);
            _scanner = new LineScanner();
            return StepResult.Success;
        }

        public StepResult Process(params object[] args)
        {
            try
            {
                var result = PreAction(args);
                if (!result.IsSuccess)
                {
                    return result;
                }

                return _scanner.Scan(_paperImage);
            }
            catch (Exception)
            {
                return new StepResult(false, new Exception("未能找到图片分割线"));
            }
            finally
            {
                _paperImage.Dispose();
            }

        }
    }
}
