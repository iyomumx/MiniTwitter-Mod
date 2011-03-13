using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MiniTwitter
{
    public static class Serializer<T>
    {
        static Serializer()
        {
            xs.UnknownAttribute += (_, __) => { };
            xs.UnknownElement += (_, __) => { };
            xs.UnknownNode += (_, __) => { };
        }

        private static readonly XmlSerializer xs = new XmlSerializer(typeof(T));

        public static T Deserialize(Stream stream)
        {
            try
            {
                return (T)xs.Deserialize(stream);
            }
            catch
            {
                return default(T);
            }
        }

        public static void Serialize(Stream stream, T o)
        {
            xs.Serialize(stream, o);

            stream.Flush();
        }
    }
}