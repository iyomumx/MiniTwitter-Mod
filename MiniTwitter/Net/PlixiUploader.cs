using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Xml.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace MiniTwitter.Net
{
	/// <summary>
	/// Simply a class that describes a set of geo coordinates
	/// </summary>
    [DataContract(Namespace = "http://api.plixi.com")]
    [Serializable]
	public class GeoLocation
	{
		public GeoLocation() { }
		public GeoLocation(object Latitude, object Longitude)
		{
			if (!((Latitude as String) == String.Empty))
			{
				try
				{
					this.Latitude = Convert.ToDouble(Latitude);
					this.Longitude = Convert.ToDouble(Longitude);
				}
				catch { }
			}
		}
		[DataMember]
		public double Latitude;
		[DataMember]
		public double Longitude;
	}

	[Serializable]
	[DataContract(Namespace = "http://api.plixi.com")]
	public class UploadPost
	{
		[DataMember]
		public string MimeType;

		[DataMember]
		public string UserName;

		[DataMember]
		public string Password;

		[DataMember]
		public string ApiKey;

		[DataMember]
		public string Message;

		[DataMember]
		public string Tags;

		[DataMember]
		public GeoLocation Location;

	}
	class PlixiUploader
	{
		private delegate void UpdateDelegate(string path, string token, string message, AsyncOperation asyncOp);

		public static byte[] SerializeToBytes<T>(T DataContactObject, bool IsJson)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				if (IsJson)
				{
					DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
					serializer.WriteObject(stream, DataContactObject);
				}
				else
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(T));
					serializer.WriteObject(stream, DataContactObject);
				}
				byte[] bytes = new byte[stream.Length];
				stream.Position = 0;
				stream.Read(bytes, 0, bytes.Length);
				return bytes;
			}
		}

		public void UploadAsync(string path, string token, string message)
		{
			var worker = new UpdateDelegate(UploadInternal);
			var asyncOp = AsyncOperationManager.CreateOperation(null);
			worker.BeginInvoke(path, token, message, asyncOp, null, null);
		}

		public void UploadInternal(string path, string token, string message, AsyncOperation asyncOp)
		{
			string mime = "";
			if ( Path.GetExtension(path).Equals(".png"))
			{
				mime = "image/png";
			}
			else if (Path.GetExtension(path).Equals(".jpg"))
			{
				mime = "image/jpg";
			}
			else if (Path.GetExtension(path).Equals(".jpeg"))
			{
				mime = "image/jpg";
			}
			else
			{
				Exception e = new ApplicationException("File Type Not Match!");
				asyncOp.PostOperationCompleted(p => UploadCompleted(this, new PlixiUploadCompletedEventArgs(null, e, false, null)), null);
				return;
			}
			//送信先のURL
			string url = "http://api.plixi.com/api/tpapi.svc/upload2";
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
			var key = "7ddf581b03e0ac5d637ae8c103470311";//TODO:Plixi API Key
			UploadPost post = new UploadPost
			{
				UserName = MiniTwitter.Properties.Settings.Default.PlixiUsername,
				Password = MiniTwitter.Properties.Settings.Default.PlixiPassword,
				ApiKey = key,
				Message = message,
				Tags = "",
				MimeType = mime,
				Location = null
			};
			req.Method = "POST";
			var enc = Encoding.UTF8;
			byte[] postBytes = SerializeToBytes<UploadPost>(post, false);
			byte[] fileBytes = File.ReadAllBytes(path);
			byte[] encodedBytes = new byte[postBytes.Length + fileBytes.Length];
			Array.Copy(postBytes, encodedBytes, postBytes.Length);
			Array.Copy(fileBytes, 0, encodedBytes, postBytes.Length, fileBytes.Length);
			req.ContentLength = encodedBytes.Length;
			req.Timeout = 1000 * 60 * 3;
			req.Headers.Add("TPAPIImageSize", fileBytes.Length.ToString());
			using (Stream requestStream = req.GetRequestStream())
			{
				int len = 1024 * 64;
				for (int i = 0; i < encodedBytes.Length; i += len)
				{
					int writeLen = len;
					if ((i + len) > encodedBytes.Length)
					{
						writeLen = encodedBytes.Length - i;
					}
					requestStream.Write(encodedBytes, i, writeLen);
				}
			}
			//サーバーからの応答を受信するためのWebResponseを取得
			var res = (HttpWebResponse)req.GetResponse();

			//応答データを受信するためのStreamを取得
			using (var resStream = res.GetResponseStream())
			{
				var xd = XDocument.Load(new XmlTextReader(resStream));

				try
				{
					var imgurl = xd.Root.Element("MediaUrl").Value;

					asyncOp.PostOperationCompleted(p => UploadCompleted(this, new PlixiUploadCompletedEventArgs(imgurl, null, false, null)), null);
				}
				catch (Exception e)
				{
					asyncOp.PostOperationCompleted(p => UploadCompleted(this, new PlixiUploadCompletedEventArgs(null, e, false, null)), null);
				}
			}
		}

		public event EventHandler<PlixiUploadCompletedEventArgs> UploadCompleted;
	}

	public class PlixiUploadCompletedEventArgs : AsyncCompletedEventArgs
	{
		public PlixiUploadCompletedEventArgs(string url, Exception error, bool cancelled, object userState)
			: base(error, cancelled, userState)
		{
			MediaUrl = url;
		}

		public string MediaUrl { get; private set; }
	}
}
