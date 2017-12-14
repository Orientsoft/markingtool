using System;
using System.Collections.Generic;

namespace DayEasy.MarkingTool.BLL
{
    /// <summary> 单例辅助 </summary>
    public class Singleton
    {
        static Singleton()
        {
            Singletons = new Dictionary<Type, object>();
        }

        private static readonly IDictionary<Type, object> Singletons;

        public static IDictionary<Type, object> AllSingletons
        {
            get { return Singletons; }
        }
    }

    /// <summary> 单例泛型辅助 </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : Singleton
    {
        private static T _instance;

        public static T Instance
        {
            get { return _instance; }
            set
            {
                _instance = value;
                AllSingletons[typeof (T)] = value;
            }
        }
    }

    /// <summary> 单例泛型列表辅助 </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonList<T> : Singleton<IList<T>>
    {
        static SingletonList()
        {
            Singleton<IList<T>>.Instance = new List<T>();
        }
        public new static IList<T> Instance
        {
            get { return Singleton<IList<T>>.Instance; }
        }
    }
}