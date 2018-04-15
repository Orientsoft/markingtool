using System;

namespace DayEasy.MarkingTool.BLL.Entity
{
    public class LineInfo
    {
        public int StartX { get; set; }
        /// <summary> 起始坐标 </summary>
        public int StartY { get; set; }

        /// <summary> 偏移坐标:小于0，向上；大于0，向下 </summary>
        public int Move { get; set; }

        /// <summary> 黑点数 </summary>
        public int BlackCount { get; set; }

        /// <summary> 偏移角度 </summary>
        public double Angle
        {
            get
            {
                //计算sin角度,Math.PI π=180度
                return Math.Asin(Move / (double)BlackCount) / (Math.PI / 180D);
            }
        }

        /// <summary> 黑点比例 </summary>
        public float BlackScale { get; set; }
    }
}
