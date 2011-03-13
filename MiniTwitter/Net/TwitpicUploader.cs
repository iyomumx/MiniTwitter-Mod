using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using MiniTwitter.Properties;

namespace MiniTwitter.Net
{
    public class TwitpicUploader
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
            string url = "http://api.twitpic.com/2/upload.xml";

            //区切り文字列
            string boundary = System.Environment.TickCount.ToString();

            //WebRequestの作成
            var req = (HttpWebRequest)WebRequest.Create(url);

            //メソッドにPOSTを指定
            req.Method = "POST";

            //ContentTypeを設定
            req.ContentType = "multipart/form-data; boundary=" + boundary;

            req.Headers["X-Verify-Credentials-Authorization"] = token;
            req.Headers["X-Auth-Service-Provider"] = "https://api.twitter.com/1/account/verify_credentials.json";

            var key = "YOUR TWITPIC KEY";

            //POST送信するデータを作成
            string postData = "";
            postData = "--" + boundary + "\r\n" +
                "Content-Disposition: form-data; name=\"key\"\r\n\r\n" + key + "\r\n" +
                "--" + boundary + "\r\n" +
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

            //送信するファイルを開く
            var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

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
            fs.Close();
            fs.Dispose();
            reqStream.Write(endData, 0, endData.Length);
            reqStream.Close();

            //サーバーからの応答を受信するためのWebResponseを取得
            var res = (HttpWebResponse)req.GetResponse();

            //応答データを受信するためのStreamを取得
            using (var resStream = res.GetResponseStream())
            {
                var xd = XDocument.Load(new XmlTextReader(resStream));

                try
                {
                    var imgurl = xd.Root.Element("url").Value;

                    asyncOp.PostOperationCompleted(p => UploadCompleted(this, new TwitpicUploadCompletedEventArgs(imgurl, null, false, null)), null);
                }
                catch (Exception e)
                {
                    asyncOp.PostOperationCompleted(p => UploadCompleted(this, new TwitpicUploadCompletedEventArgs(null, e, false, null)), null);
                }
            }
        }

        public event EventHandler<TwitpicUploadCompletedEventArgs> UploadCompleted;
    }

    public class TwitpicUploadCompletedEventArgs : AsyncCompletedEventArgs
    {
        public TwitpicUploadCompletedEventArgs(string url, Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            MediaUrl = url;
        }

        public string MediaUrl { get; private set; }
    }
}
