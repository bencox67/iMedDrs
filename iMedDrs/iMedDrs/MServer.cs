using System;
using System.IO;
using System.Net;
using System.Security;
using System.Text;
using Newtonsoft.Json;

namespace iMedDrs
{
    public class MServer
    {
        private readonly string baseurl;

        public MServer(string baseurl)
        {
            if (baseurl != "")
            {
                this.baseurl = baseurl;
            }
        }

        [SecurityCritical]
        public string[] ProcessMessage(string[] message, string method, string json)
        {
            string[] list = null;
            string error = "No response from server";
            string url = baseurl;
            foreach (var item in message)
            {
                url += "/" + item;
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new System.Uri(url));
                request.Proxy = null;
                request.Method = method;
                request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                request.ContentType = "application/json";
                if (method != "GET")
                {
                    StreamWriter stream = new StreamWriter(request.GetRequestStream());
                    stream.Write(json);
                    stream.Flush();
                }
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string rdata = reader.ReadToEnd();
                list = new string[] { "ack", rdata };
                response.Close();
                response.Dispose();
            }
            catch (Exception ex) { error = ex.Message; }
            list ??= new string[] { "nak", error };
            return list;
        }

        public string PostFile(string language, string path, string file)
        {
            string result = "Recording saved";
            WebClient client = new WebClient()
            { Credentials = new NetworkCredential("ben", "Wingnut30!") };
            try { byte[] responseArray = client.UploadFile("ftp://beaconcanada.com/MigsVoice/" + language + "/" + file, path + "/" + file); }
            catch (Exception e) { result = e.Message; }
            return result;
        }
    }
}

