using MonitorWindowsService.WS.Datos.Implementacion;
using MonitorWindowsService.WS.Entidad;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MonitorWindowsService.WS.Procesos
{
    public class Excepciones
    {
        private readonly ExcepcionDao _dao;
        private readonly EventLog _eventLog;

        public Excepciones(EventLog eventLog)
        {
            _dao = new ExcepcionDao();
            _eventLog = eventLog;
        }

        public void Start_Visitas()
        {
            _eventLog.WriteEntry("Inicio del servicio de Exepciones");
            try
            {
                IEnumerable<Configuracion> configActivos = _dao.Consultar<Configuracion>(null);
                if (configActivos.Any())
                {
                    foreach (Configuracion item in configActivos)
                    {
                        VisitarLog(item.RutaLog);
                    }
                }
            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry("Error interno del servicio de excepciones. " + ex.Message + ". " + ex.InnerException?.ToString());
            }
        }

        private void VisitarLog(string RutaLog)
        {
            _eventLog.WriteEntry("Buscando archivos de log en la ruta: " + RutaLog);
        }
    }
}