using System.ComponentModel;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Enum
{
    public static class EnumExtension
    {
        /// <summary>
        /// 获取枚举的值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int GetValue<T>(this T t)
            where T : struct
        {
            var type = typeof (T);
            if (!type.IsEnum)
                return default(int);
            try
            {
                return (int) (object) t;
            }
            catch
            {
                return default(int);
            }
        }

        /// <summary>
        /// 获取枚举描述(System.ComponentModel.Description)
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="tp">枚举</param>
        /// <returns></returns>
        public static string GetText<T>(this T tp)
            where T : struct
        {
            try
            {
                var ms = tp.GetType();
                if (!ms.IsEnum)
                    return "枚举类型错误";
                var field = ms.GetFields().FirstOrDefault(t => t.Name == tp + "");
                if (field != null)
                {
                    var desc = field.GetCustomAttributes(true).FirstOrDefault(t => (t as DescriptionAttribute) != null);
                    if (desc != null)
                        return ((DescriptionAttribute) desc).Description;
                    return field.Name;
                }
                return "枚举错误";
            }
            catch
            {
                return "枚举异常";
            }
        }

        private static readonly string[] FontColors = new[] {"Gray", "Black", "Green", "Blue", "Fuchsia", "Red"};
        private const string EnumHtml = "<font color='{0}'>{1}</font>";

        /// <summary>
        /// 获取带样式的枚举描述
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="tp">枚举</param>
        /// <param name="colors">颜色列表</param>
        /// <returns></returns>
        public static string GetEnumCssText<T>(this T tp, string[] colors)
            where T:struct 
        {
            var types = tp.GetType().GetFields().Where(t => t.IsLiteral).ToList();
            int index = types.IndexOf(types.FirstOrDefault(t => t.Name == tp + ""));
            if (index >= 0)
                return string.Format(EnumHtml, colors[(index >= colors.Length ? (colors.Length - 1) : index)],
                                     tp.GetText());
            return "";
        }

        /// <summary>
        /// 获取带样式的枚举描述
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="tp">枚举</param>
        /// <returns></returns>
        public static string GetEnumCssText<T>(this T tp)
            where T : struct
        {
            return tp.GetEnumCssText(FontColors);
        }

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <typeparam name="TV">枚举值类型</typeparam>
        /// <param name="type">枚举值</param>
        /// <returns></returns>
        public static string GetEnumText<T, TV>(this TV type)
            where T : struct
            where TV : struct
        {
            try
            {
                return ((T) (object) type).GetText();
            }
            catch
            {
                return "枚举异常";
            }
        }

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="type">枚举值</param>
        /// <returns></returns>
        public static string GetEnumText<T>(this int type)
            where T : struct
        {
            return type.GetEnumText<T, int>();
        }

        /// <summary>
        /// 获取带样式的枚举描述
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <typeparam name="TV">枚举值类型</typeparam>
        /// <param name="type">枚举值</param>
        /// <returns></returns>
        public static string GetEnumCssText<T, TV>(this TV type)
            where T : struct
            where TV : struct
        {
            try
            {
                return ((T) (object) type).GetEnumCssText();
            }
            catch
            {
                return "枚举异常";
            }
        }

        public static string GetEnumCssText<T>(this int type)
            where T : struct
        {
            return type.GetEnumCssText<T, int>();
        }

        /// <summary>
        /// 枚举转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T ToEnum<T, TV>(this TV type)
            where T : struct
            where TV : struct
        {
            try
            {
                return (T) (object) type;
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 枚举转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this int type)
            where T : struct
        {
            return type.ToEnum<T, int>();
        }
    }
}
