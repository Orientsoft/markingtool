using System;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;

namespace DayEasy.MarkingTool.Core
{
    /// <summary>
    /// 更新辅助类
    /// </summary>
    public class UpdateHelper
    {
        private ManifestInfo _manifest;

        public static UpdateHelper Instance
        {
            get { return Singleton<UpdateHelper>.Instance ?? (Singleton<UpdateHelper>.Instance = new UpdateHelper()); }
        }


        public bool CheckVersion()
        {
            var manifest = RestHelper.Instance.Manifest();
            if (!manifest.Status)
                return true;
            _manifest = manifest.Data;
            _manifest.Md5 = Guid.NewGuid();
            return _manifest != null && _manifest.AppVersion > Helper.CurrentVersion();
        }

        public ManifestInfo Manifest
        {
            get { return _manifest; }
        }
    }
}
