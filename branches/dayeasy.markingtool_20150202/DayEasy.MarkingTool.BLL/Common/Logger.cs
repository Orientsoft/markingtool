using System;
using System.Diagnostics;
using System.IO;
using log4net;
using log4net.Config;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary>
    /// 得一科技通用日志类
    /// </summary>
    public sealed class Logger
    {
        static Logger()
        {
            const string path = "config\\log4net.config";
            var config = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            if (File.Exists(config))
            {
                var fi = new FileInfo(config);
                XmlConfigurator.Configure(fi);
            }
        }

        private readonly ILog _logger;

        private Logger(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 日志类实例化方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Logger L<T>()
        {
            var logger = LogManager.GetLogger(typeof (T));
            return new Logger(logger);
        }

        /// <summary>
        /// 日志类实例化方法
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Logger L(string name = "")
        {
            var logger = LogManager.GetLogger(name);
            return new Logger(logger);
        }

        private static string Format(string msg)
        {
            var f = new StackFrame(2, true);
            return string.Format("Method:{1}({0}) {3}:{2}{5}Msg:{4}{5}",
                                 f.GetFileName(), f.GetMethod().DeclaringType,
                                 f.GetFileLineNumber(), f.GetMethod().Name, msg, Environment.NewLine);
        }

        /// <summary>
        /// 信息类日志
        /// </summary>
        /// <param name="msg"></param>
        public void I(string msg)
        {
            if (_logger.IsInfoEnabled) _logger.Info(Format(msg));
        }

        /// <summary>
        /// 调试类日志
        /// </summary>
        /// <param name="msg"></param>
        public void D(string msg)
        {
            if (_logger.IsDebugEnabled) _logger.Debug(Format(msg));
        }

        /// <summary>
        /// 警告类日志
        /// </summary>
        /// <param name="msg"></param>
        public void W(string msg)
        {
            if (_logger.IsWarnEnabled) _logger.Warn(Format(msg));
        }

        /// <summary>
        /// 错误类日志
        /// </summary>
        /// <param name="msg"></param>
        public void E(string msg)
        {
            if (_logger.IsErrorEnabled) _logger.Error(Format(msg));
        }

        public void E(string msg, Exception ex)
        {
            if (_logger.IsErrorEnabled) _logger.Error(Format(msg), ex);
        }

        /// <summary>
        /// 致命错误日志
        /// </summary>
        /// <param name="msg"></param>
        public void F(string msg)
        {
            if (_logger.IsFatalEnabled) _logger.Fatal(Format(msg));
        }

        public void F(string msg, Exception ex)
        {
            if (_logger.IsFatalEnabled) _logger.Fatal(Format(msg), ex);
        }
    }
}
