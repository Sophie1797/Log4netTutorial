using log4net;
using log4net.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Log4netTutorial
{
    public class Logger
    {
        private readonly ConcurrentQueue<FlashLogMessage> _que;// 记录消息Queue
        private readonly ManualResetEvent _mre;// 信号
        private ILog _log;// 日志
        private static Logger _flashLog = new Logger();// 日志

        public Logger()
        {
            var configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config"));
            if (!configFile.Exists)
            {
                throw new Exception("未配置log4net配置文件！");
            }          
            XmlConfigurator.Configure(configFile);// 设置日志配置文件路径

            this._que = new ConcurrentQueue<FlashLogMessage>();
            this._mre = new ManualResetEvent(false);
            this._log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public static void GetLogger(string type)
        {
            Instance()._log = LogManager.GetLogger(type);
        }
        /// <summary>
        /// 实现单例
        /// </summary>
        /// <returns></returns>
        public static Logger Instance()
        {
            return _flashLog;
        }

        /// <summary>
        /// 另一个线程记录日志，只在程序初始化时调用一次
        /// </summary>
        public void Register()
        {
            Thread t = new Thread(new ThreadStart(WriteLog));
            t.IsBackground = false;
            t.Start();
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="message">日志文本</param>
        /// <param name="level">等级</param>
        /// <param name="ex">Exception</param>
        public void EnqueueMessage(string message, FlashLogLevel level, Exception ex = null)
        {
            if ((level == FlashLogLevel.Debug && _log.IsDebugEnabled)
             || (level == FlashLogLevel.Error && _log.IsErrorEnabled)
             || (level == FlashLogLevel.Fatal && _log.IsFatalEnabled)
             || (level == FlashLogLevel.Info && _log.IsInfoEnabled)
             || (level == FlashLogLevel.Warn && _log.IsWarnEnabled))
            {
                _que.Enqueue(new FlashLogMessage
                {
                    Message = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff") + "]\r\n" + message,
                    Level = level,
                    Exception = ex
                });
                // 通知线程往磁盘中写日志
                _mre.Set();
            }
        }
        /// <summary>
        /// 从队列中写日志至磁盘
        /// </summary>
        private void WriteLog()
        {
            while (true)
            {
                // 等待信号通知
                _mre.WaitOne();
                FlashLogMessage msg;
                // 判断是否有内容需要如磁盘 从列队中获取内容，并删除列队中的内容
                while (_que.Count > 0 && _que.TryDequeue(out msg))
                {
                    // 判断日志等级，然后写日志
                    switch (msg.Level)
                    {
                        case FlashLogLevel.Debug: _log.Debug(msg.Message, msg.Exception); break;
                        case FlashLogLevel.Info:_log.Info(msg.Message, msg.Exception);break;
                        case FlashLogLevel.Error:_log.Error(msg.Message, msg.Exception);break;
                        case FlashLogLevel.Warn:_log.Warn(msg.Message, msg.Exception);break;
                        case FlashLogLevel.Fatal:_log.Fatal(msg.Message, msg.Exception);break;
                    }
                }
                // 重新设置信号
                _mre.Reset();
                Thread.Sleep(1);
            }
        }
        public static void Debug(string msg, Exception ex = null)
        {
            Instance().EnqueueMessage(msg, FlashLogLevel.Debug, ex);
        }

        public static void Error(string msg, Exception ex = null)
        {
            Instance().EnqueueMessage(msg, FlashLogLevel.Error, ex);
        }

        public static void Fatal(string msg, Exception ex = null)
        {
            Instance().EnqueueMessage(msg, FlashLogLevel.Fatal, ex);
        }

        public static void Info(string msg, Exception ex = null)
        {
            Instance().EnqueueMessage(msg, FlashLogLevel.Info, ex);
        }

        public static void Warn(string msg, Exception ex = null)
        {
            Instance().EnqueueMessage(msg, FlashLogLevel.Warn, ex);
        }
        //public void Info(string loginfo)
        //{
        //    this._log.Info(loginfo);
        //}

        //public void Debug(string loginfo)
        //{
        //    this._log.Debug(loginfo);
        //}

        //public void Warn(string loginfo)
        //{
        //    this._log.Warn(loginfo);
        //}

        //public void Error(string loginfo)
        //{
        //    this._log.Error(loginfo);
        //}


    }
    /// <summary>
    /// 日志等级
    /// </summary>
    public enum FlashLogLevel
    {
        Debug,
        Info,
        Error,
        Warn,
        Fatal
    }
    /// <summary>
    /// 日志内容
    /// </summary>
    public class FlashLogMessage
    {
        public string Message { get; set; }
        public FlashLogLevel Level { get; set; }
        public Exception Exception { get; set; }
    }

}




 