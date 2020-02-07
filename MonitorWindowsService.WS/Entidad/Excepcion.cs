using System;

namespace MonitorWindowsService.WS.Entidad
{
    public class Excepcion
    {
        public DateTime FechaOcurrencia { get; set; }

        public string Servidor { get; set; }

        public string Error { get; set; }

        public string ErrorNumero { get; set; }

        public string ErrorDescripcion { get; set; }

        public int SistemaId { get; set; }

        public string Pagina { get; set; }

        public string LogText { get; set; }

        public int UsuarioCreacionId { get; set; }
    }
}