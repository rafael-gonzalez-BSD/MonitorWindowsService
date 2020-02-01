using System.ServiceProcess;

namespace MonitorWindowsService.WS
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ProcesoLog()
            };
            ServiceBase.Run(ServicesToRun);
#else
            Procesos.Excepciones excepciones = new Procesos.Excepciones();
            excepciones.Start_Visitas();

#endif
        }
    }
}