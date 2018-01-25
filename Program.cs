using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Log4netTutorial
{
    class Program
    {
        static void Main(string[] args)
        {
            //Logger l = new Logger();
            /////ILog il = l.getLogger();
            //l.Info("someting");

            //for (int i = 0; i < 10; i++)
            //    l.Info(string.Format("Application is starting{0}", i));
            //l.Info("Hello logging world!");
            Logger.Instance().Register();

            //var loggerDbDebug = LogManager.GetLogger("logger.db.debug");
            //var loggerDbInfo = LogManager.GetLogger("logger.db.info");//创建指定类型的文本日志对象
            //var loggerFileDebug = LogManager.GetLogger("logger.file.debug");
            //var loggerFileOther = LogManager.GetLogger("logger.file.other");
            Logger.GetLogger("logger.file.ALL");
            Logger.Debug("Debug");
            Logger.Debug("Debug", new Exception("testexception"));
            Logger.Info("Info");
            Logger.Fatal("Fatal");
            Logger.Error("Error");
            Logger.Warn("Warn", new Exception("testexception"));

            Console.WriteLine("Hit enter");

            //logger.info("");
        }
    }
}
