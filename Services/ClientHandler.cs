using SocketService.Database;
using SocketService.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace SocketService.Services
{
    public class ClientHandler
    {
        private TcpClient _client;
        private Dictionary<TcpClient, string> _connectedClients;
        private const string EventSource = "SocketService";

        public ClientHandler(TcpClient client, Dictionary<TcpClient, string> connectedClients)
        {
            _client = client;
            _connectedClients = connectedClients;

            if (!EventLog.SourceExists(EventSource))
            {
                EventLog.CreateEventSource(EventSource, "Application");
            }
        }

        public void HandleClient()
        {
            using (NetworkStream stream = _client.GetStream())
            {
                StreamReader reader = new StreamReader(stream);
                StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                string initialMessage = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(initialMessage) || initialMessage.Contains("|"))
                {
                    writer.WriteLine("ERROR: Nombre de dispositivo inválido.");
                    _client.Close();
                    return;
                }

                string deviceName = initialMessage;
                _connectedClients[_client] = deviceName;
                LogEvent($"Nuevo dispositivo conectado: {deviceName}");

                while (true)
                {
                    try
                    {
                        string request = reader.ReadLine();
                        if (request == null) break;

                        string[] parts = request.Split('|');
                        string action = parts[0];

                        if (action != "LOGIN")
                        {
                            if (parts.Length < 3)
                            {
                                writer.WriteLine("ERROR: Mensaje mal formado.");
                                continue;
                            }

                            string token = parts[1];
                            string usuario = parts[2];

                            if (!JwtHelper.ValidateTokenAndUser(token, usuario))
                            {
                                writer.WriteLine("ERROR: Token inválido.");
                                continue;
                            }
                        }

                        string response = ProcessRequest(parts);
                        writer.WriteLine(response);
                    }
                    catch (Exception ex)
                    {
                        LogEvent($"Error al manejar cliente {deviceName}: {ex.Message}", EventLogEntryType.Error);
                        break;
                    }
                }

                _connectedClients.Remove(_client);
                LogEvent($"Dispositivo desconectado: {deviceName}");
            }
        }


        private string ProcessRequest(string[] parts)
        {
            string action = parts[0];

            switch (action)
            {
                case "CONSULTAR":
                    return DatabaseHelper.ConsultarProducto(long.Parse(parts[3]));
                case "CREAR":
                    return DatabaseHelper.CrearProducto(parts.Skip(3).ToArray());
                case "ACTUALIZAR":
                    return DatabaseHelper.ActualizarProducto(parts.Skip(3).ToArray());
                case "ELIMINAR":
                    return DatabaseHelper.EliminarProducto(long.Parse(parts[3]));
                case "LISTAR":
                    return DatabaseHelper.ListarProductos();
                case "LOGIN":
                    return JwtHelper.Login(parts);
                default:
                    return "ERROR: Acción no reconocida.";
            }
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
