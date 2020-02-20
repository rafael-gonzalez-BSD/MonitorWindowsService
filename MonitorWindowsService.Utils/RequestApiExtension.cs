using MonitorWindowsService.Entidad;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;

namespace MonitorWindowsService.Utils
{
    public static class RequestApiExtension
    {
        public static RespuestaPeticionModel CallApi(string url, out string mensaje)
        {
            mensaje = "";
            HttpWebResponse res = null;
            RespuestaPeticionModel m;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                res = (HttpWebResponse)req.GetResponse();
                using (StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    m = JsonConvert.DeserializeObject<RespuestaPeticionModel>(reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                mensaje = string.Format("La ruta {0} no existe: {1}. {2}", url, ex.Message, ex.InnerException);
                m = new RespuestaPeticionModel();
            }
            finally
            {
                if (res != null)
                {
                    res.Close();
                }
            }

            return m;
        }
    }
}