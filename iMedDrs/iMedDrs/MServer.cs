using System;
using System.IO;
using System.Net;
using System.Security;
using Newtonsoft.Json;

namespace iMedDrs
{
    public class ResultsList
    {
        public string[] List { get; set; }
    }

    public class DataList
    {
        public string[] List { get; set; }
    }

    public class MServer
    {
        private readonly string baseurl;
        private DataList data;

        public MServer(string baseurl)
        {
            if (baseurl != "")
            {
                this.baseurl = baseurl;
                data = new DataList();
            }
        }

        [SecurityCritical]
        public string[] ProcessMessage(string[] message, string method)
        {
            ResultsList result = new ResultsList();
            string rdata = "";
            string error = "No response from server";
            string url = baseurl;
            string json = "";
            switch (method)
            {
                case "GET":
                    for (int i = 0; i < message.Length; i++)
                        url += "/" + message[i];
                    break;
                default:
                    url += "/" + message[0];
                    data.List = message;
                    json = JsonConvert.SerializeObject(data);
                    break;
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
                request.Proxy = null;
                request.Method = method;
                request.ContentType = "application/json";
                if (method != "GET")
                {
                    StreamWriter stream = new StreamWriter(request.GetRequestStream());
                    stream.Write(json);
                    stream.Flush();
                }
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                rdata = reader.ReadToEnd();
                result = JsonConvert.DeserializeObject<ResultsList>(rdata);
            }
            catch (Exception ex) { error = ex.Message; }
            if (result.List == null)
                result.List = new string[] { "2", "nak", error };
            return result.List;
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

