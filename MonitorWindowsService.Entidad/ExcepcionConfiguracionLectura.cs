namespace MonitorWindowsService.Entidad
{
    public class ExcepcionConfiguracionLectura
    {
        public int ExcepcionConfiguracionLecturaId { get; set; }

        public string ExcepcionConfiguracionLecturaDescripcion { get; set; }

        public int NumeroRegistros { get; set; }

        public int UsuarioCreacionId { get; set; }

        public int ExcepcionConfiguracionId { get; set; }
    }
}