using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DayEasy.MarkingTool.BLL.Common
{
    public class PaperNumManager
    {
        private readonly string _filePath = "paperNums.db";
        private readonly string _accountPath = "accounts.db";
        private readonly Encoding _fileEncoding = Encoding.UTF8;
        private static List<string> NumList { get; set; }
        private static List<string> AccountList { get; set; }

        private PaperNumManager()
        {
            _filePath = Path.Combine(DeyiKeys.MarkingPath, _filePath);
            _accountPath = Path.Combine(DeyiKeys.MarkingPath, _accountPath);
            if (!File.Exists(_filePath))
            {
                using (File.Create(_filePath)) { }
                NumList = new List<string>();
            }
            else
            {
                NumList = File.ReadAllLines(_filePath, _fileEncoding).ToList();
            }

            if (!File.Exists(_accountPath))
            {
                using (File.Create(_accountPath)) { }
                AccountList = new List<string>();
            }
            else
            {
                AccountList = File.ReadAllLines(_accountPath, _fileEncoding).ToList();
            }
        }

        public static PaperNumManager Instance
        {
            get
            {
                return Singleton<PaperNumManager>.Instance ??
                       (Singleton<PaperNumManager>.Instance = new PaperNumManager());
            }
        }

        public List<string> Take(int count, int type = 0, string word = null)
        {
            return
                (type == 0 ? AccountList : NumList).Where(t => string.IsNullOrWhiteSpace(word) || t.Contains(word))
                    .Reverse()
                    .Take(count)
                    .ToList();
        }

        public void Clear(int type = 0)
        {
            if (type == 0)
            {
                AccountList = new List<string>();
                File.WriteAllText(_accountPath, string.Empty);
            }
            else
            {
                NumList = new List<string>();
                File.WriteAllText(_filePath, string.Empty);
            }
        }

        public void AddNum(string paperNum)
        {
            if (NumList.Contains(paperNum))
                NumList.Remove(paperNum);
            NumList.Add(paperNum);
            File.WriteAllLines(_filePath, NumList, _fileEncoding);
        }

        public void AddAccount(string account)
        {
            if (AccountList.Contains(account))
                AccountList.Remove(account);
            AccountList.Add(account);
            File.WriteAllLines(_accountPath, AccountList, _fileEncoding);
        }
    }
}
