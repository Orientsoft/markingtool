using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.Models.Open.Work;
using LiteDB;

namespace DayEasy.MarkingTool.BLL.Data
{
    public class CacheUtils : IDisposable
    {
        private const string ModelName = "cacheModel";
        //private readonly string _dbPath;
        private readonly Logger _logger = Logger.L<CacheUtils>();
        private readonly LiteDatabase _database;

        public CacheUtils(string dbName = null)
        {
            var path = Path.Combine(DeyiKeys.MarkingPath,
                string.IsNullOrWhiteSpace(dbName) ? DeyiKeys.LiteDbName : dbName);
            _database = new LiteDatabase(path);
        }

        public IEnumerable<CacheModel> GetModels(CacheType type, string key = null, int count = 10)
        {
            try
            {
                IEnumerable<CacheModel> models;
                if (string.IsNullOrWhiteSpace(key))
                    models = _database.GetCollection<CacheModel>(ModelName)
                        .Find(t => t.Type == (int) type);
                else
                    models = _database.GetCollection<CacheModel>(ModelName)
                        .Find(Query.And(Query.EQ("Type", (int) type), Query.StartsWith("Value", key)));
                return models
                    .OrderByDescending(t => t.Created)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                if (DeyiKeys.MarkingConfig.IsDebug)
                    _logger.E(Helper.Format(ex));
                return new List<CacheModel>();
            }
        }

        public List<string> Get(CacheType type, string key = null, int count = 10)
        {
            try
            {
                return GetModels(type, key, count).Select(t => t.Value).ToList();
            }
            catch { return new List<string>(); }
        }

        /// <summary> 设置缓存 </summary>
        /// <param name="value"></param>
        /// <param name="cacheType"></param>
        public void Set(string value, CacheType cacheType = CacheType.Account)
        {
            try
            {
                var type = (int)cacheType;
                var col = _database.GetCollection<CacheModel>(ModelName);
                if (col.Exists(t => t.Type == type && t.Value == value))
                {
                    var item = col.FindOne(t => t.Type == type && t.Value == value);
                    item.Created = DateTime.Now;
                    col.Update(item);
                    return;
                }
                if (cacheType == CacheType.SelectPath)
                {
                    col.Delete(t => t.Type == type);
                }
                col.Insert(new CacheModel
                {
                    Id = CombHelper.Guid16(),
                    Type = type,
                    Value = value,
                    Created = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                if (DeyiKeys.MarkingConfig.IsDebug)
                    _logger.E(Helper.Format(ex));
            }
        }

        public void Clear(CacheType cacheType)
        {
            try
            {
                _database.GetCollection<CacheModel>(ModelName).Delete(t => t.Type == (int)cacheType);
            }
            catch { }
        }

        public void ClearGroupCode()
        {
            try
            {
                var collection = _database.GetCollection<CacheModel>(ModelName);
                var list =
                    collection.Find(t => t.Type == (int)CacheType.GroupCode).ToList()
                        .Where(t => t.Value.Length == 7);
                foreach (var model in list)
                {
                    collection.Delete(model.Id);
                }
            }
            catch { }
        }

        public ScannerModel GetScanner(long userId, string paperId)
        {
            try
            {
                var col = _database.GetCollection<ScannerModel>(ModelName);
                return col.FindOne(t => t.UserId == userId && t.PaperId == paperId);
            }
            catch
            {
                return null;
            }
        }

        public void ClearScanner(long userId, string paperId)
        {
            try
            {
                var col = _database.GetCollection<ScannerModel>(ModelName);
                col.Delete(t => t.UserId == userId && t.PaperId == paperId);
            }
            catch { }
        }

        public IEnumerable<string> SaveScanner(long userId, string paperId, ObservableCollection<PaperMarkedInfo> list,
            IList<MPictureInfo> pictures)
        {
            try
            {
                var col = _database.GetCollection<ScannerModel>(ModelName);
                var item = col.FindOne(t => t.UserId == userId && t.PaperId == paperId);
                var scannerList = XmlHelper.XmlSerialize(list, Encoding.UTF8);
                var picture = pictures.ToJson();
                if (item == null)
                {
                    col.Insert(new ScannerModel
                    {
                        Id = CombHelper.Guid16(),
                        UserId = userId,
                        PaperId = paperId,
                        ScannerList = scannerList,
                        Pictures = picture
                    });
                    return new List<string>();
                }
                var removed = new List<string>();
                var oldList = XmlHelper.XmlDeserialize<ObservableCollection<PaperMarkedInfo>>(item.ScannerList,
                    Encoding.UTF8);
                if (oldList != null && oldList.Any())
                {
                    foreach (var info in oldList)
                    {
                        if (list.All(t => t.PaperName != info.PaperName))
                            removed.Add(info.PaperName);
                    }
                }
                item.ScannerList = scannerList;
                item.Pictures = picture;
                col.Update(item);
                return removed;

            }
            catch (Exception ex)
            {
                _logger.E(Helper.Format(ex));
                return new List<string>();
            }
        }

        public void Dispose()
        {
            if (_database != null)
            {
                _database.Dispose();
            }
        }
    }
}
