using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Recognition;
using System.Collections.Generic;

namespace DayEasy.MarkingTool.Test.Scanner
{
    public class ScannerV2 : DRecognition
    {
        public ScannerV2(string imagePath, List<ObjectiveItem> objectives)
            : base(imagePath, objectives, false)
        {

        }

        protected override void FindLines(int skip = 0)
        {
            var imageData = SourceBmp.ToBinaryArray();
            var width = imageData.GetLength(1);
            var height = imageData.GetLength(0);
            Lines = new List<LineInfo>();
            for (var y = 100; y < height; y++)
            {
                int black = 0,
                    move = 0,
                    currentY = y,
                    startX = 0;
                var prevBlack = false;
                for (var x = 0; x < width; x++)
                {
                    if (imageData[y + move, x] == 0)
                    {
                        if (!prevBlack)
                            startX = x;
                        black++;
                        prevBlack = true;
                    }
                    else if (prevBlack)
                    {
                        bool prev = (y + move > 0 && imageData[y + move - 1, x] == 0),
                            next = (y + move < height - 1 && imageData[y + move + 1, x] == 0);
                        if (prev || next)
                        {
                            black++;
                            move += (prev ? -1 : 1);
                        }
                        else
                        {
                            prevBlack = false;
                        }
                    }
                }
                if (black < 780 * 0.85)
                    continue;
                Lines.Add(new LineInfo
                {
                    StartX = startX,
                    StartY = currentY,
                    BlackCount = black,
                    BlackScale = (black / (float)width),
                    Move = move
                });
                y += 25;
                if (Lines.Count >= 2)
                    break;
            }
        }
    }
}
