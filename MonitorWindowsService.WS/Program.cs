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
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ProcesoLog()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}