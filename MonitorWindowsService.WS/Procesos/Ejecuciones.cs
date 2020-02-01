using MonitorWindowsService.WS.Utils;

namespace MonitorWindowsService.WS.Procesos
{
    public class Ejecuciones
    {
        private readonly Log _eventLog;

        public Ejecuciones()
        {
            _eventLog = new Log("Proceso de Ejecuciones", "Servicio de Monitor de Procesos");
        }

        public void Start_Visitas()
        {
            _eventLog.CrearLog("Inicio del servicio de Ejecuciones");
        }
    }
}