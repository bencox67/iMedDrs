using System.IO;

namespace iMedDrs
{
    public class PServer
    {
        public PServer()
        {
        }

        public string ReadFromFile(string sFile)
        {
            string sResult = "";
            if (File.Exists(sFile))
            {
                try
                {
                    StreamReader sr = new StreamReader(sFile);
                    if (sr != null)
                    {
                        while (!sr.EndOfStream)
                            sResult = sr.ReadLine();
                        sr.Close();
                    }
                }
                catch { }
            }
            return sResult;
        }

        public bool WriteToFile(string sFile, string sText, bool bAppend)
        {
            bool bResult = false;
            try
            {
                if (sText != "")
                {
                    StreamWriter sw = new StreamWriter(sFile, bAppend);
                    if (sw != null)
                    {
                        sw.WriteLine(sText);
                        sw.Flush();
                        sw.Close();
                        bResult = true;
                    }
                }
            }
            catch { }
            return bResult;
        }

        public string RememberMe(string sPath, string sName, bool bRemember)
        {
            string sResult = "";
            string sFile = sPath + "/RememberMe.txt";
            try
            {
                if (bRemember)
                {
                    if (sName == "")
                        sResult = ReadFromFile(sFile);
                    else
                        WriteToFile(sFile, sName, false);
                }
                else
                {
                    if (File.Exists(sFile))
                        File.Delete(sFile);
                }
            }
            catch { }
            return sResult;
        }
    }
}
