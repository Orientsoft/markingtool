using System;
using System.Collections.Generic;
using System.Threading;

namespace DayEasy.MarkingTool.BLL.Scanner
{
    public class ScannerItem
    {
        public List<string> Images { get; set; }
        public int Index { get; set; }
    }

    public class ScannerTask
    {
        private Queue<ScannerItem> _tasks = new Queue<ScannerItem>();
        private readonly Action<List<string>, int> _scnanerAction;
        private readonly int _runningCount;
        public ScannerTask(Action<List<string>, int> action, int run = 3)
        {
            _scnanerAction = action;
            _runningCount = run;
        }

        public void Start()
        {
            for (var i = 0; i < _runningCount; i++)
                StartAsync();
        }

        public event Action Completed;

        public void Add(ScannerItem item)
        {
            _tasks.Enqueue(item);
        }

        private void StartAsync()
        {
            lock (_tasks)
            {
                if (_tasks.Count > 0)
                {
                    var t = _tasks.Dequeue();
                    ThreadPool.QueueUserWorkItem(h =>
                    {
                        var arr = (object[])h;
                        _scnanerAction?.Invoke((List<string>)arr[0], (int)arr[1]);
                        StartAsync();
                    }, new object[] { t.Images, t.Index });
                }
                else
                {
                    Completed?.Invoke();
                }
            }
        }
    }
}
