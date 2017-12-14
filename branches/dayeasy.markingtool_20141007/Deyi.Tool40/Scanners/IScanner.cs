using Deyi.Tool.Step;
using System.Drawing;

namespace Deyi.Tool.Scanners
{
    interface IScanner
    {
        StepResult Scan(Image img);
    }
}
