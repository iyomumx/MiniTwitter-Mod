using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BiasedBit.MinusEngine;

namespace MiniTwitter.Net
{
    class MinusUploader
    {
        static MinusApi minusApi;
        static MinusUploader()
        {
            minusApi = new MinusApi("");
        }
        static void SignIn(string username, string password)
        {
            minusApi.SignIn(username, password);
        }
        public static void Upload(string file)
        {
            minusApi.UploadItem("", "", file);
        }
    }
}
