using MonitorWindowsService.Datos.Base;
using MonitorWindowsService.Datos.Implementacion;
using MonitorWindowsService.Entidad;
using MonitorWindowsService.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonitorWindowsService.LogExcepciones
{
    public class Excepciones : Disposable
    {
        private readonly ExcepcionDao _dao;
        private readonly ExcepcionConfiguracionLecturaDao _daoLectura;
        private readonly Log _eventLog;
        private RespuestaModel m;

        public Excepciones()
        {
            m = new RespuestaModel();
            _dao = new ExcepcionDao();
            _daoLectura = new ExcepcionConfiguracionLecturaDao();
            _eventLog = new Log("Proceso de Excepciones", "Servicio de Monitor de Procesos");
        }

        public void Start_Visitas()
        {
            RespuestaModel res = new RespuestaModel();
            List<string> filenames;
            bool existenLeidos;
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
                            logErrors = VisitarRuta(item.RutaLog, item.ConfiguracionId, out filenames, out existenLeidos);
                            if (logErrors.Count > 0)
                            {
                                res = RegistrarExcepcion(logErrors, item.SistemaId);
                            }
                        }
                        else
                        {
                            logErrors = VisitarDirectorio(item.RutaLog, item.ConfiguracionId, out filenames, out existenLeidos);
                            if (logErrors.Count > 0)
                            {
                                res = RegistrarExcepcion(logErrors, item.SistemaId);
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
                _eventLog.CrearLog("Error interno del servicio de excepciones. " + ex.Message + ". " + ex.InnerException?.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        private List<LogExcepcion> VisitarRuta(string RutaLog, int ExcepcionConfiguracionId, out List<string> filenames, out bool existenLeidos)
        {
            List<LogExcepcion> logErrors = new List<LogExcepcion>();
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
                    if (filenameI != "LogExce.txt" && ValidarFechaArchivo(filenameI))
                    {
                        _eventLog.CrearLog("Obteniendo la bitacora de logs leidos de: " + filename);

                        Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                            { "Opcion", 2 },
                            { "ExcepcionConfiguracionId", ExcepcionConfiguracionId },
                            { "ExcepcionConfiguracionLecturaDescripcion", filenameI},
                            {"Baja", false }
                        };

                        List<ExcepcionConfiguracionLectura> lectura = _daoLectura.Consultar<ExcepcionConfiguracionLectura>(P).ToList();

                        string urlFile = Path.Combine(RutaLog, filenameI);
                        string fileText = FileSystemScanner.GetLogFile(urlFile, out string mensajeArchivo);
                        logErrors.AddRange(FileSystemScanner.MapLogText<LogExcepcion>(fileText));

                        if (lectura.Any())
                        {
                            existenLeidos = true;
                            logErrors = logErrors.OrderBy(x => x.FechaRegistro).Skip(lectura[0].NumeroRegistros).ToList();
                        }

                        filenames.Add(filenameArray[filenameArray.Length - 1]);
                    }
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

        private List<LogExcepcion> VisitarDirectorio(string path, int ExcepcionConfiguracionId, out List<string> filenames, out bool existenLeidos)
        {
            existenLeidos = false;
            List<LogExcepcion> logErrors = new List<LogExcepcion>();
            filenames = new List<string>();
            _eventLog.CrearLog("Buscando archivos de log en directorio: " + path);
            try
            {
                List<string> files = FileSystemScanner.PathDirectoryDownload(path, out string mensaje);
                foreach (string filename in files.Where(x => x.Contains(".txt")))
                {
                    string[] filenameArray = filename.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
                    string filenameI = filenameArray[filenameArray.Length - 1];
                    if (filenameI != "LogExce.txt" && ValidarFechaArchivo(filenameI))
                    {
                        _eventLog.CrearLog("Obteniendo la bitacora de logs leidos de: " + filename);

                        Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                            { "Opcion", 2 },
                            { "ExcepcionConfiguracionId", ExcepcionConfiguracionId },
                            { "ExcepcionConfiguracionLecturaDescripcion", filenameI},
                            {"Baja", false }
                        };

                        List<ExcepcionConfiguracionLectura> lectura = _daoLectura.Consultar<ExcepcionConfiguracionLectura>(P).ToList();

                        _eventLog.CrearLog("Leyendo el archivo: " + filename);
                        string fileText = File.ReadAllText(filename);
                        logErrors.AddRange(FileSystemScanner.MapLogText<LogExcepcion>(fileText));

                        if (lectura.Any())
                        {
                            existenLeidos = true;
                            logErrors = logErrors.OrderBy(x => x.FechaRegistro).Skip(lectura[0].NumeroRegistros).ToList();
                        }

                        filenames.Add(filenameI);
                    }
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

        private void RegistrarArchivosLeidos(int Opcion, List<string> filenames, int NumeroRegistros, int ExcepcionConfiguracionId, bool Baja, out RespuestaModel res)
        {
            res = new RespuestaModel();
            try
            {
                Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                    { "Opcion", Opcion },
                    { "ExcepcionConfiguracionId", ExcepcionConfiguracionId },
                    { "ExcepcionConfiguracionLecturaDescripcion", filenames[filenames.Count - 1]},
                    { "NumeroRegistros", NumeroRegistros},
                    {"UsuarioCreacionId", 1 },
                    {"Baja", Baja }
                };

                res = _daoLectura.Insertar<RespuestaModel>(P);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void ActualizarArchivosLeidos(int Opcion, List<string> filenames, int NumeroRegistros, int ExcepcionConfiguracionId, bool Baja, out RespuestaModel res)
        {
            res = new RespuestaModel();
            try
            {
                Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                    { "Opcion", Opcion },
                    { "ExcepcionConfiguracionId", ExcepcionConfiguracionId },
                    { "ExcepcionConfiguracionLecturaDescripcion", filenames[filenames.Count - 1]},
                    { "NumeroRegistros", NumeroRegistros},
                    {"UsuarioModificacionId", 1 },
                    {"Baja", Baja }
                };
                res = _daoLectura.Actualizar<RespuestaModel>(P);
            }
            catch (Exception ex)
            {
                throw;
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