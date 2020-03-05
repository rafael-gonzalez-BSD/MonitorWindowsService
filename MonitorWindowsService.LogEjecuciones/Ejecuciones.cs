using MonitorWindowsService.Datos.Base;
using MonitorWindowsService.Datos.Implementacion;
using MonitorWindowsService.Entidad;
using MonitorWindowsService.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace MonitorWindowsService.LogEjecuciones
{
    public class Ejecuciones : Disposable
    {
        private readonly EjecucionDao _dao;
        private readonly EjecucionConfiguracionLecturaDao _daoLectura;
        private readonly MonitorConfiguracionDao _daoConfig;
        private readonly Log _eventLog;
        private RespuestaModel m;

        public Ejecuciones()
        {
            m = new RespuestaModel();
            _dao = new EjecucionDao();
            _daoLectura = new EjecucionConfiguracionLecturaDao();
            _daoConfig = new MonitorConfiguracionDao();
            _eventLog = new Log("Proceso de Ejecuciones", "Servicio de Monitor de Procesos");
        }

        public void Start_Visitas()
        {
            RespuestaModel res = new RespuestaModel();
            List<string> filenames;
            bool existenLeidos;
            List<LogEjecucion> logErrors;
            _eventLog.CrearLog("Inicio del servicio de Ejecuciones");
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
                            logErrors = VisitarRuta(item.RutaLog, item.ConfiguracionId, out filenames, out existenLeidos);
                            if (logErrors.Count > 0)
                            {
                                res = RegistrarEjecucion(logErrors, item.SistemaId, item.ConfiguracionId, filenames[filenames.Count - 1]);
                            }
                        }
                        else
                        {
                            logErrors = VisitarDirectorio(item.RutaLog, item.ConfiguracionId, out filenames, out existenLeidos);
                            if (logErrors.Count > 0)
                            {
                                res = RegistrarEjecucion(logErrors, item.SistemaId, item.ConfiguracionId, filenames[filenames.Count - 1]);
                            }
                        }

                        if (res.Satisfactorio && filenames.Count > 0)
                        {
                            if (existenLeidos)
                            {
                                ActualizarArchivosLeidos(3, filenames, logErrors.Count, item.ConfiguracionId, false, out res);
                            }
                            else
                            {
                                RegistrarArchivosLeidos(1, filenames, logErrors.Count, item.ConfiguracionId, false, out res);
                            }                            
                        }
                        ActualizarVisitaConfiguracion(4, item.ConfiguracionId, 1, false, out m);
                    }
                }
            }
            catch (Exception ex)
            {
                string InnerExcepcionResult = (ex.InnerException != null ? ex.InnerException?.ToString() : "").Trim();
                string ErrorResult = ("Error en el proceso: " + ex.Message + ". " + InnerExcepcionResult).Trim();
                LogExcepcion logEx = GenerarLogExcepcion("Proceso de Ejecuciones", ErrorResult, ErrorResult, "750", InnerExcepcionResult, "Proceso de Ejecuciones");
                m = RegistrarExcepcion(logEx, null);
                _eventLog.CrearLog("Error interno del servicio de ejecuciones. " + ex.Message + ". " + ex.InnerException?.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        private List<LogEjecucion> VisitarRuta(string RutaLog, int EjecucionConfiguracionId, out List<string> filenames, out bool existenLeidos)
        {
            List<LogEjecucion> logErrors = new List<LogEjecucion>();
            filenames = new List<string>();
            existenLeidos = false;
            _eventLog.CrearLog("Buscando archivos de log en la ruta: " + RutaLog);
            try
            {
                List<string> files = FileSystemScanner.UrlDirectoryDownload(RutaLog, out string mensaje);
                foreach (string filename in files.Where(x => x.Contains(".txt")))
                {
                    _eventLog.CrearLog("Leyendo el archivo: " + filename);
                    string[] filenameArray = filename.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    string filenameI = filenameArray[filenameArray.Length - 1];
                    if (filenameI != "LogEjec.txt" && ValidarFechaArchivo(filenameI))
                    {
                        _eventLog.CrearLog("Obteniendo la bitacora de logs leidos de: " + filename);

                        Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                            { "Opcion", 2 },
                            { "EjecucionConfiguracionId", EjecucionConfiguracionId },
                            { "EjecucionConfiguracionLecturaDescripcion", filenameI},
                            {"Baja", false }
                        };

                        List<EjecucionConfiguracionLectura> lectura = _daoLectura.Consultar<EjecucionConfiguracionLectura>(P).ToList();

                        string urlFile = Path.Combine(RutaLog, filenameI);
                        string fileText = FileSystemScanner.GetLogFile(urlFile, out string mensajeArchivo);
                        logErrors.AddRange(FileSystemScanner.MapLogText<LogEjecucion>(fileText));

                        if (lectura.Any())
                        {
                            existenLeidos = true;
                            logErrors = logErrors.OrderBy(x => x.Fecha).Skip(lectura[0].NumeroRegistros).ToList();
                        }

                        filenames.Add(filenameI);
                    }
                }
            }
            catch (Exception ex)
            {
                string InnerExcepcionResult = (ex.InnerException != null ? ex.InnerException?.ToString() : "").Trim();
                string ErrorResult = ("Error en el proceso: " + ex.Message + ". " + InnerExcepcionResult).Trim();
                LogExcepcion logEx = GenerarLogExcepcion("Proceso de Ejecuciones", ErrorResult, ErrorResult, "750", InnerExcepcionResult, "Proceso de Ejecuciones");
                m = RegistrarExcepcion(logEx, null);
                logErrors = new List<LogEjecucion>();
                string error = string.Format("Hubo un problema con el proceso. {0}. {1}.", ex.Message, ex.InnerException?.ToString());
                _eventLog.CrearLog(error);
            }

            return logErrors;
        }

        private List<LogEjecucion> VisitarDirectorio(string path, int EjecucionConfiguracionId, out List<string> filenames, out bool existenLeidos)
        {
            existenLeidos = false;
            List<LogEjecucion> logErrors = new List<LogEjecucion>();
            filenames = new List<string>();
            _eventLog.CrearLog("Buscando archivos de log en directorio: " + path);
            try
            {
                List<string> files = FileSystemScanner.PathDirectoryDownload(path, out string mensaje);
                foreach (string filename in files.Where(x => x.Contains(".txt")))
                {
                    string[] filenameArray = filename.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
                    string filenameI = filenameArray[filenameArray.Length - 1];
                    if (filenameI != "LogEjec.txt" && ValidarFechaArchivo(filenameI))
                    {
                        _eventLog.CrearLog("Obteniendo la bitacora de logs leidos de: " + filename);

                        Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                            { "Opcion", 2 },
                            { "EjecucionConfiguracionId", EjecucionConfiguracionId },
                            { "EjecucionConfiguracionLecturaDescripcion", filenameI},
                            {"Baja", false }
                        };

                        List<EjecucionConfiguracionLectura> lectura = _daoLectura.Consultar<EjecucionConfiguracionLectura>(P).ToList();

                        _eventLog.CrearLog("Leyendo el archivo: " + filename);
                        string fileText = File.ReadAllText(filename);
                        logErrors.AddRange(FileSystemScanner.MapLogText<LogEjecucion>(fileText));

                        if (lectura.Any())
                        {
                            existenLeidos = true;
                            logErrors = logErrors.OrderBy(x => x.Fecha).Skip(lectura[0].NumeroRegistros).ToList();
                        }

                        filenames.Add(filenameI);
                    }
                }
            }
            catch (Exception ex)
            {
                string InnerExcepcionResult = (ex.InnerException != null ? ex.InnerException?.ToString() : "").Trim();
                string ErrorResult = ("Error en el proceso: " + ex.Message + ". " + InnerExcepcionResult).Trim();
                LogExcepcion logEx = GenerarLogExcepcion("Proceso de Ejecuciones", ErrorResult, ErrorResult, "750", InnerExcepcionResult, "Proceso de Ejecuciones");
                m = RegistrarExcepcion(logEx, null);
                logErrors = new List<LogEjecucion>();
                string error = string.Format("Hubo un problema con el proceso. {0}. {1}.", ex.Message, ex.InnerException?.ToString());
                _eventLog.CrearLog(error);
            }

            return logErrors;
        }

        private RespuestaModel RegistrarEjecucion(List<LogEjecucion> logErrors, int SistemaId, int EjecucionConfiguracionId, string filename)
        {
            List<Ejecucion> list = MapearLogs(logErrors, SistemaId, EjecucionConfiguracionId, filename);

            try
            {
                foreach (Ejecucion ejecucion in list.OrderBy(x => x.FechaOcurrencia))
                {
                    Dictionary<string, dynamic> P = ejecucion.AsDictionary();
                    m = _dao.Insertar<RespuestaModel>(P);
                }
            }
            catch (Exception ex)
            {
                string InnerExcepcionResult = (ex.InnerException != null ? ex.InnerException?.ToString() : "").Trim();
                string ErrorResult = ("Error en el proceso: " + ex.Message + ". " + InnerExcepcionResult).Trim();
                LogExcepcion logEx = GenerarLogExcepcion("Proceso de Ejecuciones", ErrorResult, ErrorResult, "750", InnerExcepcionResult, "Proceso de Ejecuciones");
                m = RegistrarExcepcion(logEx, null);
                m.Id = 0;
                m.ErrorId = -2;
                m.Satisfactorio = false;
                m.Datos = null;
                m.Mensaje = ex.Message + ". " + ex.InnerException;
            }

            return m;
        }
        private RespuestaModel RegistrarExcepcion(LogExcepcion logEx, int? SistemaId)
        {
            List<LogExcepcion> logErrors = new List<LogExcepcion> { logEx };
            if (!SistemaId.HasValue)
            {
                string Identificador = ConfigurationManager.AppSettings["SistemaDefault"];
                Dictionary<string, dynamic> PConfig = new Dictionary<string, dynamic> {
                    {"Identificador", Identificador }
                };
                var monConfig = _daoConfig.ConsultarPor<MonitorConfiguracion>(PConfig);
                if (monConfig != null)
                {
                    SistemaId = Convert.ToInt32(monConfig.Valor);
                }
            }
            List<Excepcion> list = MapearLogs(logErrors, SistemaId.Value);

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

        private void ActualizarVisitaConfiguracion(int Opcion, int EjecucionConfiguracionId, int UsuarioModificacionId, bool Baja, out RespuestaModel res)
        {
            res = new RespuestaModel();
            Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                { "EjecucionConfiguracionId", EjecucionConfiguracionId },
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
                string InnerExcepcionResult = (ex.InnerException != null ? ex.InnerException?.ToString() : "").Trim();
                string ErrorResult = ("Error en el proceso: " + ex.Message + ". " + InnerExcepcionResult).Trim();
                LogExcepcion logEx = GenerarLogExcepcion("Proceso de Ejecuciones", ErrorResult, ErrorResult, "750", InnerExcepcionResult, "Proceso de Ejecuciones");
                m = RegistrarExcepcion(logEx, null);
                res.Id = 0;
                res.ErrorId = -2;
                res.Satisfactorio = false;
                res.Datos = null;
                res.Mensaje = ex.Message + ". " + ex.InnerException;
            }
        }

        private void RegistrarArchivosLeidos(int Opcion, List<string> filenames, int NumeroRegistros, int EjecucionConfiguracionId, bool Baja, out RespuestaModel res)
        {
            res = new RespuestaModel();
            try
            {
                Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                    { "Opcion", Opcion },
                    { "EjecucionConfiguracionId", EjecucionConfiguracionId },
                    { "EjecucionConfiguracionLecturaDescripcion", filenames[filenames.Count - 1]},
                    { "NumeroRegistros", NumeroRegistros},
                    {"UsuarioCreacionId", 1 },
                    {"Baja", Baja }
                };

                res = _daoLectura.Insertar<RespuestaModel>(P);
            }
            catch (Exception ex)
            {
                string InnerExcepcionResult = (ex.InnerException != null ? ex.InnerException?.ToString() : "").Trim();
                string ErrorResult = ("Error en el proceso: " + ex.Message + ". " + InnerExcepcionResult).Trim();
                LogExcepcion logEx = GenerarLogExcepcion("Proceso de Ejecuciones", ErrorResult, ErrorResult, "750", InnerExcepcionResult, "Proceso de Ejecuciones");
                m = RegistrarExcepcion(logEx, null);
            }
        }

        private void ActualizarArchivosLeidos(int Opcion, List<string> filenames, int NumeroRegistros, int EjecucionConfiguracionId, bool Baja, out RespuestaModel res)
        {
            res = new RespuestaModel();
            try
            {
                Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                    { "Opcion", Opcion },
                    { "EjecucionConfiguracionId", EjecucionConfiguracionId },
                    { "EjecucionConfiguracionLecturaDescripcion", filenames[filenames.Count - 1]},
                    { "NumeroRegistros", NumeroRegistros},
                    {"UsuarioModificacionId", 1 },
                    {"Baja", Baja }
                };
                res = _daoLectura.Actualizar<RespuestaModel>(P);
            }
            catch (Exception ex)
            {
                string InnerExcepcionResult = (ex.InnerException != null ? ex.InnerException?.ToString() : "").Trim();
                string ErrorResult = ("Error en el proceso: " + ex.Message + ". " + InnerExcepcionResult).Trim();
                LogExcepcion logEx = GenerarLogExcepcion("Proceso de Ejecuciones", ErrorResult, ErrorResult, "750", InnerExcepcionResult, "Proceso de Ejecuciones");
                m = RegistrarExcepcion(logEx, null);
            }
        }

        private bool ValidarFechaArchivo(string archivo)
        {
            string FechaHoy = DateTime.Now.Date.ToString("yyyyMMdd");
            string FechaArchivo = archivo.Substring(7, 8);
            if (FechaArchivo == FechaHoy)
            {
                return true;
            }

            return false;
        }

        private static List<Ejecucion> MapearLogs(List<LogEjecucion> logErrors, int SistemaId, int EjecucionConfiguracionId, string filename)
        {
            List<Ejecucion> list = new List<Ejecucion>();
            logErrors.ForEach(x =>
            {
                list.Add(new Ejecucion()
                {
                    EjecucionTipoId = x.Evento == "Inicio" ? 1 : 2,
                    EjecucionDetalleDescripcion = x.Proceso,
                    FechaOcurrencia = x.Fecha,
                    ProcesoId = x.ProcesoId,
                    Servidor = x.ServerName,
                    SistemaId = SistemaId,
                    UsuarioCreacionId = 1,
                    EjecucionConfiguracionId = EjecucionConfiguracionId,
                    NombreArchivo = filename
                });
            });
            return list;
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

        private static LogExcepcion GenerarLogExcepcion(string Aplicacion, string Error, string ErrorDescription, string ErrorNumber, string InnerException, string Page)
        {
            return new LogExcepcion()
            {
                Aplicacion = Aplicacion,
                BrowserType = "",
                CookiesInfo = "",
                Error = Error, // <----- Aqui ponerle el texto personalizado
                ErrorDescription = ErrorDescription,  // <----- Aqui ponerle el texto personalizado
                ErrorNumber = ErrorNumber, // error grupal de Ejecuciones
                FechaRegistro = DateTime.Now,
                InnerException = InnerException,
                LocalStorage = "",
                Page = Page,
                PageReferrer = "",
                ProcesoId = -1,
                QueryString = "",
                ServerName = "",
                SessionInfo = "",
                SiglaRed = ""

            };
        }
    }
}