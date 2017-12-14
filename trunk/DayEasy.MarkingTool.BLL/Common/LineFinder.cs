using DayEasy.MarkingTool.BLL.Entity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary> 线条查询器 </summary>
    public class LineFinder : IDisposable
    {
        /// <summary> 二值化之后的原图片 </summary>
        private readonly Bitmap _sourceBmp;

        private readonly int _width;
        private readonly int _height;

        public LineFinder(Image bmp)
        {
            _width = bmp.Width;
            _height = bmp.Height;
            _sourceBmp = (Bitmap)bmp.Clone();
        }

        /// <summary> 查找线条 </summary>
        /// <param name="length">线条最小长度</param>
        /// <param name="lineSkip">线条间距</param>
        /// <param name="lineCount">线条数</param>
        /// <param name="skip">跳过多少像素</param>
        /// <param name="position">线条方向</param>
        /// <returns></returns>
        public List<LineInfo> Find(int length, int lineSkip, int lineCount = -1, int skip = 0,
            LinePosition position = LinePosition.Horizontal)
        {
            if (position == LinePosition.Horizontal)
                return FindHorizontalLines(length, lineSkip, lineCount, skip);
            return FindVerticalLines(length, lineSkip, lineCount, skip);
        }

        /// <summary> 查找水平线条 </summary>
        /// <param name="length">线条最小长度</param>
        /// <param name="lineHeight">线条间距</param>
        /// <param name="count">线条数</param>
        /// <param name="skip">跳过多少y像素</param>
        /// <returns></returns>
        private List<LineInfo> FindHorizontalLines(int length, int lineHeight, int count = -1, int skip = 0)
        {
            var lines = new List<LineInfo>();
            var data = _sourceBmp.LockBits(new Rectangle(0, 0, _width, _height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            unsafe
            {
                const int bpp = 4;
                var ptr = (byte*)data.Scan0;
                int remain = data.Stride - data.Width * bpp,
                    linePtr = remain + data.Width * bpp;
                //跳过最小Y坐标
                ptr += linePtr * skip;
                //遍历每行
                for (int y = skip; y < data.Height-50; y++)
                {
                    int black = 0,
                        move = 0,
                        currentY = y,
                        startX = 0;
                    var prevBlack = false;
                    //遍历行内像素点
                    for (var x = 0; x < data.Width; x++)
                    {
                        //黑点判断
                        if (IsBlack(ptr))
                        {
                            if (!prevBlack)
                                startX = x;
                            black++;
                            prevBlack = true;
                        }
                        else if (prevBlack)
                        {
                            ptr -= linePtr;
                            var prev = IsBlack(ptr);
                            ptr += 2 * linePtr;
                            var next = IsBlack(ptr);
                            if (prev || next)
                            {
                                black++;
                                move += (prev ? -1 : 1);
                                if (prev) ptr -= 2 * linePtr;
                            }
                            else
                            {
                                ptr -= linePtr;
                                prevBlack = false;
                            }
                        }
                        ptr += bpp;
                    }
                    ptr += remain;
                    //还原
                    ptr -= linePtr * move;
                    //行内黑点数判断
                    if (black < length)
                        continue;
                    lines.Add(new LineInfo
                    {
                        StartX = startX,
                        StartY = currentY,
                        BlackCount = black,
                        BlackScale = (black / (float)_width),
                        Move = move
                    });
                    if (count > 0 && lines.Count >= count)
                        break;
                    y += lineHeight;
                    ptr += linePtr * lineHeight;
                }
            }
            _sourceBmp.UnlockBits(data);
            return lines;
        }

        /// <summary>
        /// 查找垂直线条
        /// </summary>
        /// <param name="length">线条最小长度</param>
        /// <param name="lineHeight">线条间距</param>
        /// <param name="count">线条数</param>
        /// <param name="skip">跳过多少x像素</param>
        /// <returns></returns>
        private List<LineInfo> FindVerticalLines(int length, int lineHeight, int count = -1, int skip = 0)
        {
            var lines = new List<LineInfo>();
            var data = _sourceBmp.LockBits(new Rectangle(0, 0, _width, _height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                const int bpp = 3;
                var ptr = (byte*)data.Scan0;
                int remain = data.Stride - data.Width * bpp,
                    linePtr = remain + data.Width * bpp;
                //跳过最小Y坐标
                //遍历每行
                for (int x = skip; x < data.Width; x++)
                {
                    int black = 0,
                        move = 0,
                        currentX = x;
                    var prevBlack = false;
                    ptr += skip * bpp;
                    //遍历行内像素点
                    for (int y = 0; y < data.Width; y++)
                    {
                        //黑点判断
                        if (IsBlack(ptr))
                        {
                            black++;
                            prevBlack = true;
                        }
                        else if (prevBlack)
                        {
                            bool isBlack = false;
                            if (x > 0)
                            {
                                ptr -= bpp;
                                if (IsBlack(ptr))
                                {
                                    move--;
                                    black++;
                                    isBlack = true;
                                }
                                else
                                {
                                    ptr += bpp;
                                }
                            }
                            if (!isBlack && x < data.Width - 1)
                            {
                                ptr += bpp;
                                if (IsBlack(ptr))
                                {
                                    move++;
                                    black++;
                                }
                                else
                                {
                                    ptr -= bpp;
                                    prevBlack = false;
                                }
                            }
                        }
                        ptr += linePtr;
                    }
                    ptr += remain;
                    ptr -= bpp * move;
                    //行内黑点数判断
                    if (black >= length)
                    {
                        lines.Add(new LineInfo
                        {
                            StartY = currentX,
                            BlackCount = black,
                            Move = move
                        });
                        if (count > 0 && lines.Count >= count)
                            break;
                        x += lineHeight;
                        ptr += linePtr * lineHeight;
                    }
                }
            }
            _sourceBmp.UnlockBits(data);
            return lines;
        }

        private unsafe bool IsBlack(byte* ptr)
        {
            try
            {
                return (ptr[2] * 0.299) + (ptr[1] * 0.587) + (ptr[0] * 0.114) < DeyiKeys.ScannerConfig.LineTreshold;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _sourceBmp?.Dispose();
            GC.Collect();
        }
    }

    public enum LinePosition
    {
        /// <summary> 水平 </summary>
        Horizontal,
        /// <summary> 垂直 </summary>
        Vertical
    }
}
