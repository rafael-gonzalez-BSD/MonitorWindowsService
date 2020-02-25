namespace MonitorWindowsService.Entidad
{
    public class Conector
    {
        public int ConectorConfiguracionId { get; set; }

        public int UsuarioCreacionId { get; set; }

        public string ConectorDetalleDescripcion { get; set; }

        public bool EjecucionSatisfactoria { get; set; }

        public int ConectorDetalleRespuestaId { get; set; }
    }
}