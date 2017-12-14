using Deyi.Tool.Common;
using Deyi.Tool.Scanners;
using Deyi.Tool.Step;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;


namespace Deyi.Tool.Steps
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
            var fileName = (String)args[0];
            var y = (int)args[1];
            if (!File.Exists(fileName))
            {
                return new StepResult(false, new FileNotFoundException(string.Format("没有找到文件:{0}", fileName)));
            }
            _baseImageCode = Image.FromFile(fileName);
            _baseImageCode = ImageHelper.MakeImage(_baseImageCode, 0, 0, _baseImageCode.Width, y - 4);
            _baseImageCode = ImageHelper.BinarizeImage(new Bitmap(_baseImageCode), DeyiKeys.BasicThreshold);
            return StepResult.Success;
        }

        public StepResult Process(params object[] args)
        {

            //  System.Drawing.Image img_withQRCode = System.Drawing.Image.FromFile(savePath + fileName_ew);
            //img_withQRCode.Save("code.png");
            try
            {
                var result = PreAction(args);
                if (!result.IsSuccess)
                {
                    return result;
                }
                var info = new Dictionary<string, string>();
                _scanner.Scan(_baseImageCode);
                var infos = _scanner.StudentNo.Split(new[] { ",", "，" }, StringSplitOptions.RemoveEmptyEntries);
                if (infos.Length == 3)
                {
                    info.Add("Name", infos[0]);
                    info.Add("IDNo", infos[1]);
                    info.Add("StudentNo", infos[2]);
                }
                else
                {
                    throw new Exception("学生信息不正确");
                }
                info.Add("BatchCode", _scanner.BatchNo);
                return new InfomationScaningResult(info);
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
