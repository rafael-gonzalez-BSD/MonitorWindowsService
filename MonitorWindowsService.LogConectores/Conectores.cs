using MonitorWindowsService.Datos.Implementacion;
using MonitorWindowsService.Entidad;
using MonitorWindowsService.Utils;
using System;
using System.Collections.Generic;
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
            RespuestaModel res;

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
                _eventLog.CrearLog("Error interno del servicio de excepciones. " + ex.Message + ". " + ex.InnerException?.ToString(), System.Diagnostics.EventLogEntryType.Error);
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
                AlertaDescripcion = peticionModel.Descripcion,
                ConectorDetalleDescripcion = peticionModel.Descripcion,
                ConectorConfiguracionId = config.ConfiguracionId,
                EjecucionSatisfactoria = (peticionModel.Clave == 1 || peticionModel.Clave == 2),
                ConectorDetalleRespuestaId = peticionModel.Clave,
                UsuarioCreacionId = 1
            };
        }
    }
}