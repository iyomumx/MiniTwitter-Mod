using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Xml.Linq;
using System.Net;

namespace MiniTwitter.Net
{
    class imglyUploader
    {
        private delegate void UpdateDelegate(string path, string token, string message, AsyncOperation asyncOp);

        public void UploadAsync(string path, string token, string message)
        {
            var worker = new UpdateDelegate(UploadInternal);
            var asyncOp = AsyncOperationManager.CreateOperation(null);
            worker.BeginInvoke(path, token, message, asyncOp, null, null);
        }

        public void UploadInternal(string path, string token, string message, AsyncOperation asyncOp)
        {
            //送信先のURL
            string url = "http://img.ly/api/2/upload.xml";

            //区切り文字列
            string boundary = System.Environment.TickCount.ToString();

            //WebRequestの作成
            var req = (HttpWebRequest)WebRequest.Create(url);

            if (MiniTwitter.Properties.Settings.Default.UseProxy)
            {
                if (MiniTwitter.Properties.Settings.Default.UseIEProxy)
                {
                    req.Proxy = System.Net.WebRequest.GetSystemWebProxy();
                }
                else
                {
                    int pn = 0;
                    if (!(int.TryParse(MiniTwitter.Properties.Settings.Default.ProxyPortNumber, out pn)))
                    {
                        var p = new System.Net.WebProxy(MiniTwitter.Properties.Settings.Default.ProxyAddress, pn);
                        if (!(string.IsNullOrEmpty(MiniTwitter.Properties.Settings.Default.ProxyUsername) || string.IsNullOrEmpty(MiniTwitter.Properties.Settings.Default.ProxyPassword)))
                        {
                            p.Credentials = new System.Net.NetworkCredential(MiniTwitter.Properties.Settings.Default.ProxyUsername, MiniTwitter.Properties.Settings.Default.ProxyPassword);
                        }
                        req.Proxy = p;
                    }

                }

            }
            //メソッドにPOSTを指定
            req.Method = "POST";

            //ContentTypeを設定
            req.ContentType = "multipart/form-data; boundary=" + boundary;

            req.Headers["X-Verify-Credentials-Authorization"] = token;
            req.Headers["X-Auth-Service-Provider"] = "https://api.twitter.com/1/account/verify_credentials.json";

            //POST送信するデータを作成
            string postData = "";
            postData = "--" + boundary + "\r\n" +
                "Content-Disposition: form-data; name=\"message\"\r\n\r\n" + message + "\r\n" +
                "--" + boundary + "\r\n" +
                "Content-Disposition: form-data; name=\"media\"; filename=\"" + Path.GetFileName(path) + "\"\r\n" +
                "Content-Type: application/octet-stream\r\n" +
                "Content-Transfer-Encoding: binary\r\n\r\n";

            //バイト型配列に変換
            var enc = Encoding.UTF8;

            byte[] startData = enc.GetBytes(postData);
            postData = "\r\n--" + boundary + "--\r\n";
            byte[] endData = enc.GetBytes(postData);
            FileStream fs = null;
            try
            {
                //送信するファイルを開く
                fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                //POST送信するデータの長さを指定
                req.ContentLength = startData.Length + endData.Length + fs.Length;

                //データをPOST送信するためのStreamを取得
                var reqStream = req.GetRequestStream();

                //送信するデータを書き込む
                reqStream.Write(startData, 0, startData.Length);

                //ファイルの内容を送信
                byte[] readData = new byte[0x10000];
                int readSize = 0;
                while (true)
                {
                    readSize = fs.Read(readData, 0, readData.Length);
                    if (readSize == 0)
                    {
                        break;
                    }
                    reqStream.Write(readData, 0, readSize);
                }
                reqStream.Write(endData, 0, endData.Length);
                reqStream.Close();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }



            //サーバーからの応答を受信するためのWebResponseを取得
            var res = (HttpWebResponse)req.GetResponse();

            //応答データを受信するためのStreamを取得
            using (var resStream = res.GetResponseStream())
            {
                var xd = XDocument.Load(new XmlTextReader(resStream));

                try
                {
                    var imgurl = xd.Root.Element("url").Value;

                    asyncOp.PostOperationCompleted(p => UploadCompleted(this, new imglyUploadCompletedEventArgs(imgurl, null, false, null)), null);
                }
                catch (Exception e)
                {
                    asyncOp.PostOperationCompleted(p => UploadCompleted(this, new imglyUploadCompletedEventArgs(null, e, false, null)), null);
                }
            }
        }

        public event EventHandler<imglyUploadCompletedEventArgs> UploadCompleted;
    }

    public class imglyUploadCompletedEventArgs : AsyncCompletedEventArgs
    {
        public imglyUploadCompletedEventArgs(string url, Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            MediaUrl = url;
        }

        public string MediaUrl { get; private set; }
    }
}
