using System;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.Models.Open.System;

namespace DayEasy.MarkingTool.Core
{
    /// <summary>
    /// 更新辅助类
    /// </summary>
    public class UpdateHelper
    {
        private MManifestDto _manifest;

        public static UpdateHelper Instance
        {
            get { return Singleton<UpdateHelper>.Instance ?? (Singleton<UpdateHelper>.Instance = new UpdateHelper()); }
        }


        public bool CheckVersion()
        {
            var manifest = RestHelper.Instance.Manifest();
            if (manifest == null || !manifest.Status)
                return false;
            _manifest = manifest.Data;
            _manifest.Md5 = Guid.NewGuid();
            return _manifest != null && new Version(_manifest.Version) > Helper.CurrentVersion();
        }

        public MManifestDto Manifest
        {
            get { return _manifest; }
        }
    }
}
