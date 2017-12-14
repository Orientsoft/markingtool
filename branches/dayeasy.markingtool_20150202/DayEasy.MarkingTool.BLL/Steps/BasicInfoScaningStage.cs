using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Scanners;
using DayEasy.MarkingTool.BLL.Steps.Result;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DayEasy.MarkingTool.BLL.Steps
{

    /// <summary>
    /// 基本信息处理
    /// 
    /// StudentNo 学生身份证号
    /// PaperBatchNum 作业批次号
    /// </summary>
    public class BasicInfoScaningStage : IStep
    {
        private readonly PaperBasicInfoScanner _scanner = new PaperBasicInfoScanner();

        private Image _baseImageCode;

        //在此步骤中, 既要扫描二维码，也要扫描条形码。

        private StepResult PreAction(object[] args)
        {
            var orgiFileName = (String)args[0];
            var y = (int)args[1];
            if (!File.Exists(orgiFileName))
                return new StepResult(false, new FileNotFoundException(string.Format("没有找到文件:{0}", orgiFileName)));
            var fileName = Path.GetFileNameWithoutExtension(orgiFileName);
            if (string.IsNullOrWhiteSpace(fileName))
                return new StepResult(false, new Exception("贴码区域图片获取失败"));
            var basePath = Path.Combine(DeyiKeys.SavePath, fileName, DeyiKeys.RecognitionName);
            _baseImageCode = Image.FromFile(Path.Combine(basePath, "0.jpg"));
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
                result =_scanner.Scan(_baseImageCode);
                if (!result.IsSuccess || _scanner.Student == null)
                {
                    throw new Exception("学生信息不正确");
                }
                return new InfomationScaningResult(_scanner.Student);
            }
            catch (Exception e)
            {
                return new StepResult(false, e);
            }
            finally
            {
                if (_baseImageCode != null)
                {
                    _baseImageCode.Dispose();
                }
            }
        }
    }
}
