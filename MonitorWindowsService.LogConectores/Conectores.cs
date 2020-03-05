using MonitorWindowsService.Datos.Implementacion;
using MonitorWindowsService.Entidad;
using MonitorWindowsService.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MonitorWindowsService.LogConectores
{
    public class Conectores
    {
        private readonly ConectorDao _dao;
        private readonly MonitorConfiguracionDao _daoConfig;
        private readonly Log _eventLog;
        private RespuestaModel m;

        public Conectores()
        {
            m = new RespuestaModel();
            _dao = new ConectorDao();
            _daoConfig = new MonitorConfiguracionDao();
            _eventLog = new Log("Proceso de Conectores", "Servicio de Monitor de Procesos");
        }

        public void Start_Visitas()
        {
            LogExcepcion logEx;
            string ErrorResult = "";
            RespuestaModel res;

            _eventLog.CrearLog("Inicio del servicio de Conectores");
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
                        RespuestaPeticionModel peticionModel = LlamarAPI(item.RutaLog);

                        res = RegistrarConector(peticionModel, item);
                        if (res.Satisfactorio)
                        {
                            ActualizarVisitaConfiguracion(4, item.ConfiguracionId, 1, false, out m);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string InnerExcepcionResult = (ex.InnerException != null ? ex.InnerException?.ToString() : "").Trim();
                ErrorResult = ("Error en el proceso: " + ex.Message + ". " + InnerExcepcionResult).Trim();
                logEx = GenerarLogExcepcion("Proceso de Conectores", ErrorResult, ErrorResult, "850", InnerExcepcionResult, "Proceso de Conectores");                
                m = RegistrarExcepcion(logEx, null);
                _eventLog.CrearLog("Error interno del servicio de conectores. " + ex.Message + ". " + ex.InnerException?.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        private RespuestaPeticionModel LlamarAPI(string RutaLog)
        {
            string mensaje = "";

            RespuestaPeticionModel res;
            _eventLog.CrearLog("Llamando a la ruta: " + RutaLog);
            try
            {
                res = RequestApiExtension.CallApi(RutaLog, out mensaje);
            }
            catch (Exception ex)
            {
                string InnerExcepcionResult = (ex.InnerException != null ? ex.InnerException?.ToString() : "").Trim();
                string ErrorResult = ("Error en el proceso: " + ex.Message + ". " + InnerExcepcionResult).Trim();
                LogExcepcion logEx = GenerarLogExcepcion("Proceso de Conectores", ErrorResult, ErrorResult, "850", InnerExcepcionResult, "Proceso de Conectores");
                m = RegistrarExcepcion(logEx, null);
                string error = string.Format("Hubo un problema con el proceso. {0}. {1}.", ex.Message, ex.InnerException?.ToString());
                _eventLog.CrearLog(error);
                res = new RespuestaPeticionModel();
            }

            return res;
        }

        private RespuestaModel RegistrarConector(RespuestaPeticionModel peticionModel, Configuracion config)
        {
            Conector conector = MapearLlamada(peticionModel, config);

            try
            {
                Dictionary<string, dynamic> P = conector.AsDictionary();
                m = _dao.Insertar<RespuestaModel>(P);
            }
            catch (Exception ex)
            {
                string InnerExcepcionResult = (ex.InnerException != null ? ex.InnerException?.ToString() : "").Trim();
                string ErrorResult = ("Error en el proceso: " + ex.Message + ". " + InnerExcepcionResult).Trim();
                LogExcepcion logEx = GenerarLogExcepcion("Proceso de Conectores", ErrorResult, ErrorResult, "850", InnerExcepcionResult, "Proceso de Conectores");
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

        private void ActualizarVisitaConfiguracion(int Opcion, int ConectorConfiguracionId, int UsuarioModificacionId, bool Baja, out RespuestaModel res)
        {
            res = new RespuestaModel();
            Dictionary<string, dynamic> P = new Dictionary<string, dynamic> {
                { "ConectorConfiguracionId", ConectorConfiguracionId },
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
                LogExcepcion logEx = GenerarLogExcepcion("Proceso de Conectores", ErrorResult, ErrorResult, "850", InnerExcepcionResult, "Proceso de Conectores");
                m = RegistrarExcepcion(logEx, null);
                res.Id = 0;
                res.ErrorId = -2;
                res.Satisfactorio = false;
                res.Datos = null;
                res.Mensaje = ex.Message + ". " + ex.InnerException;
            }
        }

        private static Conector MapearLlamada(RespuestaPeticionModel peticionModel, Configuracion config)
        {
            return new Conector()
            {
                ConectorDetalleDescripcion = peticionModel.Descripcion,
                ConectorConfiguracionId = config.ConfiguracionId,
                EjecucionSatisfactoria = (peticionModel.Clave == 1 || peticionModel.Clave == 2),
                ConectorDetalleRespuestaId = peticionModel.Clave,
                UsuarioCreacionId = 1
            };
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
                ErrorNumber = ErrorNumber, // error grupal de Conectores
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