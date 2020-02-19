using System;

namespace MonitorWindowsService.Entidad
{
    public class Ejecucion
    {
        public int UsuarioCreacionId { get; set; }

        public string EjecucionDetalleDescripcion { get; set; }

        public string Servidor { get; set; }

        public DateTime FechaOcurrencia { get; set; }

        public int EjecucionTipoId { get; set; }

        public int ProcesoId { get; set; }

        public int SistemaId { get; set; }
    }
}