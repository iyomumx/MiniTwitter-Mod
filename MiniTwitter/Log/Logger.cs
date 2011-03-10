using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniTwitter.Log
{
    public class Logger : PropertyChangedBase, IDisposable
    {
        private string logFile;
        private LogFormat _Format;
        private ConcurrentQueue<LogItem> WorkItem;
        protected Task LogTask;
        protected CancellationTokenSource TokenSource = new CancellationTokenSource();
        protected CancellationToken LogTaskToken;

        private static Logger defaultInstance;

        public static Logger Default
        {
            get
            {
                return defaultInstance;
            }
        }

        static Logger()
        {
            defaultInstance = new Logger(Path.Combine(MiniTwitter.Properties.Settings.BaseDirectory, "MiniTwitterLog.log"));
            defaultInstance.Format = LogFormat.PlainText;
            defaultInstance.WorkItem = new ConcurrentQueue<LogItem>();
        }

        public Logger(string logFilePath)
        {
            this.logFile = logFilePath;
            FileInfo file = new FileInfo(logFile);
            if (File.Exists(logFile))
            {
                if (file.Length > 10485760)
                {
                    file.CopyTo(Path.Combine(file.DirectoryName, "MiniTwitterLog.old"), true);
                    file.Delete();
                    var f = file.Create();
                    f.Close();
                    f.Dispose();
                }
            }
            else
            {
                file.Create();
            }
            StartLogTask();
        }

        protected void StartLogTask()
        {
            LogTaskToken = TokenSource.Token;
            LogTask = new Task(() => 
            {
                try
                {
                    if (LogTaskToken.IsCancellationRequested) return;
                    using (StreamWriter sw = new StreamWriter(logFile, true, System.Text.Encoding.Unicode))
                    {
                        try
                        {
                            LogItem cl;
                            while (!LogTaskToken.IsCancellationRequested)
                            {
                                if (!this.WorkItem.IsEmpty)
                                {
                                    if (this.WorkItem.TryDequeue(out cl))
                                    {
                                        sw.WriteLine(cl.ToString(Format));
                                    }
                                }
                                //LogTaskToken.ThrowIfCancellationRequested();
                            }
                        }
                        catch (IOException)
                        {

                        }
                        catch(Exception e)
                        {
                            try
                            {
                                sw.WriteLine(string.Format("遇到错误，记录日志失败，停止记录：{0} 在方法 {1}",e.Message,e.TargetSite.Name));
                            }
                            catch (Exception)
                            {
                            }
                        }
                        finally
                        {
                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
                catch (IOException)
                {
                    //无法写入文件
                }
                catch
                {
                    //其他错误
                }
            }, LogTaskToken);
            LogTask.Start();
        }

        public void AddLogItem(LogItem item)
        {
            WorkItem.Enqueue(item);
        }

        #region Properties

        public LogFormat Format
        {
            get
            {
                return _Format;
            }
            set
            {
                if (_Format!=value)
                {
                    _Format = value;
                    OnPropertyChanged("Format");
                }
            }
        }

        

        #endregion

        #region IDisposable Member
        private bool Disposing = true;

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (LogTask.Status==TaskStatus.Running)
                {
                    TokenSource.Cancel();
                }
                Disposing = false;
            }

        }

        void IDisposable.Dispose()
        {
            Dispose(Disposing);
        }

        #endregion



    }
}
