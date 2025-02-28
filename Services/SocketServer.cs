using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketService.Services
{
    public class SocketServer
    {
        private TcpListener _server;
        private bool _isRunning;
        private Dictionary<TcpClient, string> _connectedClients = new Dictionary<TcpClient, string>();
        private const string EventSource = "SocketService";

        public void Start()
        {
            try
            {
                if (!EventLog.SourceExists(EventSource))
                {
                    EventLog.CreateEventSource(EventSource, "Application");
                }

                string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
                int port = int.Parse(ConfigurationManager.AppSettings["Port"]);

                _server = new TcpListener(IPAddress.Parse(ipAddress), port);
                _server.Start();
                _isRunning = true;

                LogEvent($"Servidor iniciado en {ipAddress}:{port}");

                while (_isRunning)
                {
                    TcpClient client = _server.AcceptTcpClient();
                    ClientHandler clientHandler = new ClientHandler(client, _connectedClients);
                    Thread clientThread = new Thread(clientHandler.HandleClient);
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                LogEvent($"Error en el servidor: {ex.Message}", EventLogEntryType.Error);
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _server?.Stop();
            LogEvent("Servidor detenido.");
        }

        private void LogEvent(string message, EventLogEntryType type = EventLogEntryType.Information)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = EventSource;
                eventLog.WriteEntry(message, type);
            }
        }
    }
}
