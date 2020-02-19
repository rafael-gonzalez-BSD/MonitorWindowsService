using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MonitorWindowsService.Utils
{
    public static class FileSystemScanner
    {
        public static List<string> UrlDirectoryDownload(string url, out string mensaje)
        {
            mensaje = "";
            HttpWebResponse res = null;
            List<string> files = new List<string>();

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";

            try
            {
                res = (HttpWebResponse)req.GetResponse();
                using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();

                    Regex regEx = new Regex(@"href\s*=\s*(?:[""'](?<filename>[^""']*[.txt])[""']|(?<filename>[.txt]\S+))", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));
                    MatchCollection matches = regEx.Matches(html);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Success)
                            {
                                files.Add(match.Groups["filename"].Value);
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                mensaje = string.Format("La ruta {0} no existe: {1}. {2}", url, ex.Message, ex.InnerException);
                files = new List<string>();
            }
            finally
            {
                if (res != null)
                {
                    res.Close();
                }
            }

            return files;
        }

        public static List<string> PathDirectoryDownload(string path, out string mensaje)
        {
            mensaje = "";
            List<string> files;
            try
            {
                files = Directory.GetFiles(path).ToList();
            }
            catch (Exception ex)
            {
                mensaje = string.Format("El archivo {0} no existe: {1}. {2}", path, ex.Message, ex.InnerException);
                files = new List<string>();
            }

            return files;
        }

        public static string GetLogFile(string urlFile, out string mensaje)
        {
            mensaje = "";
            string completeFile;
            HttpWebResponse res = null;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(urlFile);
            req.Method = "GET";

            try
            {
                res = (HttpWebResponse)req.GetResponse();
                using (StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    completeFile = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                mensaje = string.Format("El archivo {0} no existe: {1}. {2}", urlFile, ex.Message, ex.InnerException);
                completeFile = "";
            }
            finally
            {
                if (res != null)
                {
                    res.Close();
                }
            }

            return completeFile;
        }

        public static List<T> MapLogText<T>(string fileText)
        {
            string completeFile = string.Format("[{0}]", fileText);

            return JsonConvert.DeserializeObject<List<T>>(completeFile);
        }
    }
}