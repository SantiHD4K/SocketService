using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace SocketService
{
    public partial class SocketService : ServiceBase
    {
        private TcpListener _server;
        private bool _isRunning;
        private Dictionary<TcpClient, string> _connectedClients = new Dictionary<TcpClient, string>();

        public SocketService()
        {
            InitializeComponent();
            this.ServiceName = "SocketService";
        }

        protected override void OnStart(string[] args)
        {
            _isRunning = true;
            Thread serverThread = new Thread(StartServer);
            serverThread.Start();
        }

        protected override void OnStop()
        {
            _isRunning = false;
            if (_server != null)
            {
                _server.Stop();
            }
        }


        private void StartServer()
        {
            try
            {
                string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
                int port = int.Parse(ConfigurationManager.AppSettings["Port"]);

                _server = new TcpListener(IPAddress.Parse(ipAddress), port);
                _server.Start();

                while (_isRunning)
                {
                    TcpClient client = _server.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Error en el servidor: {ex.Message}", EventLogEntryType.Error);
            }
        }
        private void HandleClient(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            {
                StreamReader reader = new StreamReader(stream);
                StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                string deviceName = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(deviceName))
                {
                    writer.WriteLine("ERROR: Nombre inválido.");
                    client.Close();
                    return;
                }

                _connectedClients[client] = deviceName;
                EventLog.WriteEntry($"Nuevo dispositivo conectado: {deviceName}", EventLogEntryType.Information);

                while (_isRunning)
                {
                    try
                    {
                        string request = reader.ReadLine();
                        if (request == null) break;

                        string[] parts = request.Split('|');
                        string action = parts[0];

                        if (action == "CONSULTAR")
                        {
                            long codigoBarra = long.Parse(parts[1]);
                            string response = ConsultarProducto(codigoBarra);
                            writer.WriteLine(response);
                        }
                        else if (action == "CREAR")
                        {
                            string response = CrearProducto(parts);
                            writer.WriteLine(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry($"Error al manejar cliente {deviceName}: {ex.Message}", EventLogEntryType.Error);
                        break;
                    }
                }
                _connectedClients.Remove(client);
                EventLog.WriteEntry($"Dispositivo desconectado: {deviceName}", EventLogEntryType.Information);
            }
        }


        private string ConsultarProducto(long codigoBarra)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Productos WHERE CodigoBarra = @CodigoBarra";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CodigoBarra", codigoBarra);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return $"{reader["CodigoBarra"]}|{reader["Nombre"]}|{reader["Precio"]}|{reader["Costo"]}|{reader["PrecioPromo"]}|{reader["IVA"]}";
                        }
                        else
                        {
                            return "NO_ENCONTRADO";
                        }
                    }
                }
            }
        }

        private string CrearProducto(string[] datos)
        {
            try
            {
                long codigoBarra = long.Parse(datos[1]);
                string nombre = datos[2];
                decimal precio = decimal.Parse(datos[3]);
                decimal costo = decimal.Parse(datos[4]);
                decimal precioPromo = decimal.Parse(datos[5]);
                int iva = int.Parse(datos[6]);

                string connectionString = ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Productos (CodigoBarra, Nombre, Precio, Costo, PrecioPromo, IVA) VALUES (@CodigoBarra, @Nombre, @Precio, @Costo, @PrecioPromo, @IVA)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CodigoBarra", codigoBarra);
                        command.Parameters.AddWithValue("@Nombre", nombre);
                        command.Parameters.AddWithValue("@Precio", precio);
                        command.Parameters.AddWithValue("@Costo", costo);
                        command.Parameters.AddWithValue("@PrecioPromo", precioPromo);
                        command.Parameters.AddWithValue("@IVA", iva);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return "CREADO";
                        }
                        else
                        {
                            EventLog.WriteEntry("Error al insertar el producto, filas afectadas: 0", EventLogEntryType.Error);
                            return "ERROR: No se insertaron filas.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Error al crear el producto: {ex.Message}, StackTrace: {ex.StackTrace}", EventLogEntryType.Error);
                return "ERROR: " + ex.Message;
            }
        }

    }
}