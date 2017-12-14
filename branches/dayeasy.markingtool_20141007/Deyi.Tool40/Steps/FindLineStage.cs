using Deyi.Tool.Common;
using Deyi.Tool.Scanners;
using Deyi.Tool.Step;
using System;
using System.Drawing;
using System.IO;

namespace Deyi.Tool.Steps
{
    /// <summary>
    /// 获取线条位置
    /// </summary>
    public class FindLineStage : IStep
    {
        IScanner _scanner = null;
        Image _paperImage = null;

        private StepResult PreAction(params object[] args)
        {
            var fileName = (String)args[0];
            if (!File.Exists(fileName))
            {
                return new StepResult(false, new FileNotFoundException(string.Format("没有找到文件:{0}", fileName)));
            }
            _paperImage = Image.FromFile(fileName);
            _paperImage = ImageHelper.BinarizeImage((Bitmap) _paperImage, DeyiKeys.AnswerThreshold);
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
