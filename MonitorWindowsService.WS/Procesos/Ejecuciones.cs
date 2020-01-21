using System.Diagnostics;

namespace MonitorWindowsService.WS.Procesos
{
    public class Ejecuciones
    {
        private readonly EventLog _eventLog;

        public Ejecuciones(EventLog eventLog)
        {
            _eventLog = eventLog;
        }

        public void Start_Visitas()
        {
            _eventLog.WriteEntry("Inicio del servicio de Ejecuciones");
            throw new System.NotSupportedException();
        }
    }
}