using System.Diagnostics;

namespace MonitorWindowsService.Utils
{
    public class Log
    {
        private readonly EventLog eventLog;

        public Log(string source, string log)
        {
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, log);
            }
            eventLog = new EventLog
            {
                Source = source,
                Log = log
            };
        }

        public void CrearLog(string mensaje)
        {
            eventLog.WriteEntry(mensaje);
        }

        public void CrearLog(string mensaje, EventLogEntryType? tipo)
        {
            eventLog.WriteEntry(mensaje, tipo.Value);
        }

        public void CrearLog(string mensaje, EventLogEntryType? tipo, int? eventId)
        {
            eventLog.WriteEntry(mensaje, tipo.Value, eventId.Value);
        }
    }
}