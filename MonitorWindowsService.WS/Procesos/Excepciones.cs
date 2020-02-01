using MonitorWindowsService.WS.Datos.Implementacion;
using MonitorWindowsService.WS.Entidad;
using MonitorWindowsService.WS.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MonitorWindowsService.WS.Procesos
{
    public class Excepciones
    {
        private readonly ExcepcionDao _dao;
        private readonly Log _eventLog;

        private int SistemaId = 1;

        public Excepciones()
        {
            _dao = new ExcepcionDao();
            _eventLog = new Log("Proceso de Excepciones", "Servicio de Monitor de Procesos");
        }

        public void Start_Visitas()
        {
            _eventLog.CrearLog("Inicio del servicio de Exepciones");
            try
            {
                //string path = Path.GetFullPath(Path.Combine("System.AppDomain.CurrentDomain.BaseDirectory", "..\\..\\..\\Logs"));
                //VisitarDirectorio(path);

                IEnumerable<Configuracion> configActivos = _dao.Consultar<Configuracion>(null);
                if (configActivos.Any())
                {
                    foreach (Configuracion item in configActivos)
                    {
                        SistemaId = item.SistemaId;
                        if (item.RutaLog.Substring(0,7).Contains("http://") || item.RutaLog.Substring(0, 8).Contains("https://"))
                        {
                            VisitarRuta(item.RutaLog);
                        }
                        else 
                        {
                            VisitarDirectorio(item.RutaLog);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _eventLog.CrearLog("Error interno del servicio de excepciones. " + ex.Message + ". " + ex.InnerException?.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        private void VisitarRuta(string RutaLog)
        {
            _eventLog.CrearLog("Buscando archivos de log en la ruta: " + RutaLog);
            try
            {
                List<string> files = FileSystemScanner.UrlDirectoryDownload(RutaLog, out string mensaje);
                foreach (string filename in files.Where(x => x.Contains(".txt")))
                {
                    _eventLog.CrearLog("Leyendo el archivo: " + filename);
                    string urlFile = Path.Combine(RutaLog, filename);
                    List<string> lines = FileSystemScanner.GetLogFile(urlFile, out string mensajeArchivo);
                    List<LogError> logErrors = FileSystemScanner.MapLog(lines);

                    if (logErrors.Count > 0)
                    {
                        logErrors.ForEach(RegistrarExcepcion);
                    }
                }
            }
            catch (Exception ex)
            {
                string error = string.Format("Hubo un problema con el proceso. {0}. {1}.", ex.Message, ex.InnerException?.ToString());
                _eventLog.CrearLog(error);
            }
        }

        private void VisitarDirectorio(string path)
        {
            _eventLog.CrearLog("Buscando archivos de log en directorio: " + path);
            try
            {
                List<string> files = FileSystemScanner.PathDirectoryDownload(path, out string mensaje);
                foreach (string filename in files)
                {
                    _eventLog.CrearLog("Leyendo el archivo: " + filename);
                    // Metodo para archivos no divididos con comas
                    //List<string> lines = File.ReadAllLines(filename).ToList();
                    //List<LogError> logErrors = FileSystemScanner.MapLog(lines);
                    // Metodo para archivos divididos por comas.
                    string fileText = File.ReadAllText(filename);
                    
                    List<LogError> logErrors = FileSystemScanner.MapLogText(fileText);
                    if (logErrors.Count > 0)
                    {
                        logErrors.ForEach(RegistrarExcepcion);
                    }
                }
            }
            catch (Exception ex)
            {
                string error = string.Format("Hubo un problema con el proceso. {0}. {1}.", ex.Message, ex.InnerException?.ToString());
                _eventLog.CrearLog(error);
            }
        }

        private void RegistrarExcepcion(LogError logError)
        {
            Dictionary<string, dynamic> parametros = MapearLogDiccionario(logError);            
        }

        private Dictionary<string, dynamic> MapearLogDiccionario(LogError logError)
        {
            Dictionary<string, dynamic> P = new Dictionary<string, dynamic>();
            
            string jsonString = JsonConvert.SerializeObject(logError);
            Excepcion m = new Excepcion
            {
                Error = logError.Error,
                ErrorDescripcion = logError.ErrorDescription,
                ErrorNumero = logError.ErrorNumber,
                FechaOcurrencia = logError.FechaRegistro,
                LogText = jsonString,
                Pagina = logError.Page,
                Servidor = logError.ServerName,
                SistemaId = SistemaId,
                UsuarioCreacionId = 100
            };

            var props = m.GetType().GetProperties();

            foreach (PropertyInfo prop in props)
            {
                string Key = prop.Name.ToString();
                dynamic Value = m.GetType().GetProperty(Key).GetValue(m, null);

                P.Add(Key, Value);
            }

            return P;
        }
    }
}