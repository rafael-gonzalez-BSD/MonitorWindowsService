using System;

namespace MonitorWindowsService.WS.Entidad
{
    public class LogEjecucion
    {
        public int ProcesoId { get; set; }
        public string Proceso { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string ServerName { get; set; }
    }
}