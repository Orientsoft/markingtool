using System;

namespace DayEasy.MarkingTool.BLL.Config
{
    /// <summary>
    /// 配置文件名属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FileNameAttribute : Attribute
    {
        public string Name { get; set; }

        public FileNameAttribute(string name)
        {
            Name = name;
        }
    }
}
