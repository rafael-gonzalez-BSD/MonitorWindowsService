using System;

namespace MonitorWindowsService.WS.Entidad
{
    public class Ejecucion
    {
        public int UsuarioCreacionId { get; set; }

        public string EjecucionDetalleDescripcion { get; set; }

        public string Servidor { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public int EjecucionTipoId { get; set; }

        public int ProcesoId { get; set; }

        public int SistemaId { get; set; }
    }
}