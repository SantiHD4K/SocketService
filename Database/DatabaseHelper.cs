using System;
using System.Configuration;
using System.Data.SqlClient;
namespace SocketService.Database {

    public static class DatabaseHelper
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString;

        public static string ConsultarProducto(long codigoBarra)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
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
                    }
                }
            }
            return "NO_ENCONTRADO";
        }

        public static string CrearProducto(string[] datos)
        {
            try
            {
                if (datos.Length < 7) return "ERROR: Faltan datos.";

                long codigoBarra = long.Parse(datos[1]);
                string nombre = datos[2];
                decimal precio = decimal.Parse(datos[3]);
                decimal costo = decimal.Parse(datos[4]);
                decimal precioPromo = decimal.Parse(datos[5]);
                int iva = int.Parse(datos[6]);

                using (SqlConnection connection = new SqlConnection(ConnectionString))
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
                        return command.ExecuteNonQuery() > 0 ? "CREADO" : "ERROR: No se pudo crear el producto.";
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }
    }


}


