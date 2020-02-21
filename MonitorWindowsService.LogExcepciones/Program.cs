using System.ServiceProcess;

namespace MonitorWindowsService.LogExcepciones
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
            Excepciones excepciones = new Excepciones();
            excepciones.Start_Visitas();

#endif
        }
    }
}