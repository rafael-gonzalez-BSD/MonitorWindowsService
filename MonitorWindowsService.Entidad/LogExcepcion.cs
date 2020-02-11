using System;

namespace MonitorWindowsService.Entidad
{
    public class LogExcepcion
    {
        public DateTime FechaRegistro { get; set; }
        public int? ProcesoId { get; set; }
        public string Error { get; set; }
        public string ErrorNumber { get; set; }
        public string ErrorDescription { get; set; }
        public string SiglaRed { get; set; }
        public string Aplicacion { get; set; }
        public string QueryString { get; set; }
        public string ServerName { get; set; }
        public string InnerException { get; set; }
        public string Page { get; set; }
        public string PageReferrer { get; set; }
        public string BrowserType { get; set; }
        public string SessionInfo { get; set; }
        public string CookiesInfo { get; set; }
        public string LocalStorage { get; set; }
    }
}