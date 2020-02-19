using MonitorWindowsService.Datos.Implementacion;
using MonitorWindowsService.Entidad;
using MonitorWindowsService.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonitorWindowsService.LogConectores
{
    public class Conectores
    {
        private readonly ConectorDao _dao;
        private readonly Log _eventLog;
        private RespuestaModel m;

        public Conectores()
        {
            m = new RespuestaModel();
            _dao = new ConectorDao();
            _eventLog = new Log("Proceso de Conectores", "Servicio de Monitor de Procesos");
        }

        public void Start_Visitas()
        {
            RespuestaModel res = new RespuestaModel();
            List<LogExcepcion> logErrors;
            _eventLog.CrearLog("Inicio del servicio de Exepciones");
            try
            {
                Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                    { "Opcion", 5 }
                };

                IEnumerable<Configuracion> configActivos = _dao.Consultar<Configuracion>(P);
                if (configActivos.Any())
                {
                    foreach (Configuracion item in configActivos)
                    {
                        if (item.RutaLog.Substring(0, 7).Contains("http://") || item.RutaLog.Substring(0, 8).Contains("https://"))
                        {
                            logErrors = VisitarRuta(item.RutaLog);
                            if (logErrors.Count > 0)
                            {
                                res = RegistrarExcepcion(logErrors, item.SistemaId);
                            }
                        }
                        else
                        {
                            logErrors = VisitarDirectorio(item.RutaLog);
                            if (logErrors.Count > 0)
                            {
                                res = RegistrarExcepcion(logErrors, item.SistemaId);
                            }
                        }

                        if (res.Satisfactorio)
                        {
                            ActualizarVisitaConfiguracion(4, item.ConfiguracionId, 1, false, out m);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _eventLog.CrearLog("Error interno del servicio de excepciones. " + ex.Message + ". " + ex.InnerException?.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        private List<LogExcepcion> VisitarRuta(string RutaLog)
        {
            List<LogExcepcion> logErrors = new List<LogExcepcion>();
            _eventLog.CrearLog("Buscando archivos de log en la ruta: " + RutaLog);
            try
            {
                List<string> files = FileSystemScanner.UrlDirectoryDownload(RutaLog, out string mensaje);
                foreach (string filename in files.Where(x => x.Contains(".txt")))
                {
                    _eventLog.CrearLog("Leyendo el archivo: " + filename);
                    string[] filenameArray = filename.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    string urlFile = Path.Combine(RutaLog, filenameArray[filenameArray.Length - 1]);
                    string fileText = FileSystemScanner.GetLogFile(urlFile, out string mensajeArchivo);
                    logErrors.AddRange(FileSystemScanner.MapLogText<LogExcepcion>(fileText));
                }
            }
            catch (Exception ex)
            {
                logErrors = new List<LogExcepcion>();
                string error = string.Format("Hubo un problema con el proceso. {0}. {1}.", ex.Message, ex.InnerException?.ToString());
                _eventLog.CrearLog(error);
            }

            return logErrors;
        }

        private List<LogExcepcion> VisitarDirectorio(string path)
        {
            List<LogExcepcion> logErrors = new List<LogExcepcion>();
            _eventLog.CrearLog("Buscando archivos de log en directorio: " + path);
            try
            {
                List<string> files = FileSystemScanner.PathDirectoryDownload(path, out string mensaje);
                foreach (string filename in files.Where(x => x.Contains(".txt")))
                {
                    _eventLog.CrearLog("Leyendo el archivo: " + filename);
                    string fileText = File.ReadAllText(filename);

                    logErrors.AddRange(FileSystemScanner.MapLogText<LogExcepcion>(fileText));
                }
            }
            catch (Exception ex)
            {
                logErrors = new List<LogExcepcion>();
                string error = string.Format("Hubo un problema con el proceso. {0}. {1}.", ex.Message, ex.InnerException?.ToString());
                _eventLog.CrearLog(error);
            }

            return logErrors;
        }

        private RespuestaModel RegistrarExcepcion(List<LogExcepcion> logErrors, int SistemaId)
        {
            List<Excepcion> list = MapearLogs(logErrors, SistemaId);

            try
            {
                foreach (Excepcion excepcion in list)
                {
                    Dictionary<string, dynamic> P = excepcion.AsDictionary();
                    m = _dao.Insertar<RespuestaModel>(P);
                }
            }
            catch (Exception ex)
            {
                m.Id = 0;
                m.ErrorId = -2;
                m.Satisfactorio = false;
                m.Datos = null;
                m.Mensaje = ex.Message + ". " + ex.InnerException;
            }

            return m;
        }

        private void ActualizarVisitaConfiguracion(int Opcion, int ExcepcionConfiguracionId, int UsuarioModificacionId, bool Baja, out RespuestaModel res)
        {
            res = new RespuestaModel();
            Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                { "ExcepcionConfiguracionId", ExcepcionConfiguracionId },
                { "UsuarioModificacionId", UsuarioModificacionId },
                { "Baja", Baja },
                { "Opcion", Opcion }
            };

            try
            {
                res = _dao.Actualizar<RespuestaModel>(P);
            }
            catch (Exception ex)
            {
                res.Id = 0;
                res.ErrorId = -2;
                res.Satisfactorio = false;
                res.Datos = null;
                res.Mensaje = ex.Message + ". " + ex.InnerException;
            }
        }

        private static List<Excepcion> MapearLogs(List<LogExcepcion> logErrors, int SistemaId)
        {
            List<Excepcion> list = new List<Excepcion>();
            logErrors.ForEach(x =>
            {
                string logText = JsonConvert.SerializeObject(x);
                list.Add(new Excepcion()
                {
                    Error = x.Error,
                    ErrorDescripcion = x.ErrorDescription,
                    ErrorNumero = x.ErrorNumber,
                    FechaOcurrencia = x.FechaRegistro,
                    LogText = logText,
                    Pagina = x.Page,
                    Servidor = x.ServerName,
                    SistemaId = SistemaId,
                    UsuarioCreacionId = 1
                });
            });
            return list;
        }
    }
}