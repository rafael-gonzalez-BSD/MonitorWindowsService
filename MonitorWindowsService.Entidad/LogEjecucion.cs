using System;

namespace MonitorWindowsService.Entidad
{
    public class LogEjecucion
    {
        public string Evento { get; set; }
        public int ProcesoId { get; set; }
        public string Proceso { get; set; }
        public DateTime Fecha { get; set; }
        public string ServerName { get; set; }
    }
}