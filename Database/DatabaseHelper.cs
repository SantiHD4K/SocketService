using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
namespace SocketService.Database
{

    public static class DatabaseHelper
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString;

        public static string ListarProductos()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Productos";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            StringBuilder result = new StringBuilder();
                            while (reader.Read())
                            {
                                string producto = $"{reader["CodigoBarra"]}|{reader["PLU"]}|{reader["Nombre"]}|{reader["Precio"]}|{reader["Costo"]}|{reader["PrecioPromo"]}|{reader["IVA"]}|{reader["Activo"]}|{reader["Existencia"]}|{reader["Ext_Minima"]}|{reader["Ext_Maxima"]}|{reader["Rentabilidad_Minima"]}|{reader["Avg_Venta"]}|{reader["Avg_Compra"]}|{reader["Ultima_Compra"]}|{reader["Ultima_Venta"]}";
                                result.AppendLine(producto);
                            }
                            return result.ToString().Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

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
                            return $"{reader["CodigoBarra"]}|{reader["PLU"]}|{reader["Nombre"]}|{reader["Precio"]}|{reader["Costo"]}|{reader["PrecioPromo"]}|{reader["IVA"]}|{reader["Activo"]}|{reader["Existencia"]}|{reader["Ext_Minima"]}|{reader["Ext_Maxima"]}|{reader["Rentabilidad_Minima"]}|{reader["Avg_Venta"]}|{reader["Avg_Compra"]}|{reader["Ultima_Compra"]}|{reader["Ultima_Venta"]}";
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
                if (datos.Length < 16) return "ERROR: Faltan datos. Recibidos: " + datos.Length;
                string plu = datos[1];
                string nombre = datos[2];

                if (!long.TryParse(datos[0], out long codigoBarra))
                    return $"ERROR: Código de barra inválido. Recibido: '{datos[0]}'";

                if (!decimal.TryParse(datos[3], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precio))
                    return "ERROR: Precio inválido.";
                if (!decimal.TryParse(datos[4], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costo))
                    return "ERROR: Costo inválido.";
                if (!decimal.TryParse(datos[5], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precioPromo))
                    return "ERROR: Precio promocional inválido.";

                if (!int.TryParse(datos[6], out int iva)) return "ERROR: IVA inválido.";

                bool activo = datos[7] == "1" || datos[7].ToLower() == "true";

                if (!int.TryParse(datos[8], out int existencia)) return "ERROR: Existencia inválida.";
                if (!int.TryParse(datos[9], out int extMinima)) return "ERROR: Ext_Minima inválida.";
                if (!int.TryParse(datos[10], out int extMaxima)) return "ERROR: Ext_Maxima inválida.";

                if (!decimal.TryParse(datos[11], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rentabilidadMinima))
                    return "ERROR: Rentabilidad mínima inválida.";
                if (!decimal.TryParse(datos[12], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal avgVenta))
                    return "ERROR: Promedio de venta inválido.";
                if (!decimal.TryParse(datos[13], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal avgCompra))
                    return "ERROR: Promedio de compra inválido.";

                if (!DateTime.TryParseExact(datos[14], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ultimaCompra))
                    return "ERROR: Fecha de última compra inválida.";
                if (!DateTime.TryParseExact(datos[15], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ultimaVenta))
                    return "ERROR: Fecha de última venta inválida.";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO Productos 
                    (CodigoBarra, PLU, Nombre, Precio, Costo, PrecioPromo, IVA, Activo, Existencia, Ext_Minima, Ext_Maxima, Rentabilidad_Minima, Avg_Venta, Avg_Compra, Ultima_Compra, Ultima_Venta) 
                    VALUES 
                    (@CodigoBarra, @PLU, @Nombre, @Precio, @Costo, @PrecioPromo, @IVA, @Activo, @Existencia, @Ext_Minima, @Ext_Maxima, @Rentabilidad_Minima, @Avg_Venta, @Avg_Compra, @Ultima_Compra, @Ultima_Venta)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CodigoBarra", codigoBarra);
                        command.Parameters.AddWithValue("@PLU", plu);
                        command.Parameters.AddWithValue("@Nombre", nombre);
                        command.Parameters.AddWithValue("@Precio", precio);
                        command.Parameters.AddWithValue("@Costo", costo);
                        command.Parameters.AddWithValue("@PrecioPromo", precioPromo);
                        command.Parameters.AddWithValue("@IVA", iva);
                        command.Parameters.AddWithValue("@Activo", activo);
                        command.Parameters.AddWithValue("@Existencia", existencia);
                        command.Parameters.AddWithValue("@Ext_Minima", extMinima);
                        command.Parameters.AddWithValue("@Ext_Maxima", extMaxima);
                        command.Parameters.AddWithValue("@Rentabilidad_Minima", rentabilidadMinima);
                        command.Parameters.AddWithValue("@Avg_Venta", avgVenta);
                        command.Parameters.AddWithValue("@Avg_Compra", avgCompra);
                        command.Parameters.AddWithValue("@Ultima_Compra", ultimaCompra);
                        command.Parameters.AddWithValue("@Ultima_Venta", ultimaVenta);

                        return command.ExecuteNonQuery() > 0 ? "CREADO" : "ERROR: No se pudo crear el producto.";
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }


        public static string ActualizarProducto(string[] datos)
        {
            try
            {
                if (datos.Length < 16) return "ERROR: Faltan datos.";
                string plu = datos[1];
                string nombre = datos[2];

                if (!long.TryParse(datos[0], out long codigoBarra))
                    return $"ERROR: Código de barra inválido. Recibido: '{datos[0]}'";

                if (!decimal.TryParse(datos[3], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precio))
                    return "ERROR: Precio inválido.";
                if (!decimal.TryParse(datos[4], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costo))
                    return "ERROR: Costo inválido.";
                if (!decimal.TryParse(datos[5], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precioPromo))
                    return "ERROR: Precio promocional inválido.";

                if (!int.TryParse(datos[6], out int iva)) return "ERROR: IVA inválido.";

                bool activo = datos[7] == "1" || datos[7].ToLower() == "true";

                if (!int.TryParse(datos[8], out int existencia)) return "ERROR: Existencia inválida.";
                if (!int.TryParse(datos[9], out int extMinima)) return "ERROR: Ext_Minima inválida.";
                if (!int.TryParse(datos[10], out int extMaxima)) return "ERROR: Ext_Maxima inválida.";

                if (!decimal.TryParse(datos[11], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rentabilidadMinima))
                    return "ERROR: Rentabilidad mínima inválida.";
                if (!decimal.TryParse(datos[12], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal avgVenta))
                    return "ERROR: Promedio de venta inválido.";
                if (!decimal.TryParse(datos[13], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal avgCompra))
                    return "ERROR: Promedio de compra inválido.";

                if (!DateTime.TryParseExact(datos[14], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ultimaCompra))
                    return "ERROR: Fecha de última compra inválida.";
                if (!DateTime.TryParseExact(datos[15], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ultimaVenta))
                    return "ERROR: Fecha de última venta inválida.";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"UPDATE Productos 
                    SET PLU = @PLU, Nombre = @Nombre, Precio = @Precio, Costo = @Costo, PrecioPromo = @PrecioPromo, IVA = @IVA, Activo = @Activo, Existencia = @Existencia, Ext_Minima = @Ext_Minima, Ext_Maxima = @Ext_Maxima, Rentabilidad_Minima = @Rentabilidad_Minima, Avg_Venta = @Avg_Venta, Avg_Compra = @Avg_Compra, Ultima_Compra = @Ultima_Compra, Ultima_Venta = @Ultima_Venta 
                    WHERE CodigoBarra = @CodigoBarra";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CodigoBarra", codigoBarra);
                        command.Parameters.AddWithValue("@PLU", plu);
                        command.Parameters.AddWithValue("@Nombre", nombre);
                        command.Parameters.AddWithValue("@Precio", precio);
                        command.Parameters.AddWithValue("@Costo", costo);
                        command.Parameters.AddWithValue("@PrecioPromo", precioPromo);
                        command.Parameters.AddWithValue("@IVA", iva);
                        command.Parameters.AddWithValue("@Activo", activo);
                        command.Parameters.AddWithValue("@Existencia", existencia);
                        command.Parameters.AddWithValue("@Ext_Minima", extMinima);
                        command.Parameters.AddWithValue("@Ext_Maxima", extMaxima);
                        command.Parameters.AddWithValue("@Rentabilidad_Minima", rentabilidadMinima);
                        command.Parameters.AddWithValue("@Avg_Venta", avgVenta);
                        command.Parameters.AddWithValue("@Avg_Compra", avgCompra);
                        command.Parameters.AddWithValue("@Ultima_Compra", ultimaCompra);
                        command.Parameters.AddWithValue("@Ultima_Venta", ultimaVenta);

                        return command.ExecuteNonQuery() > 0 ? "ACTUALIZADO" : "ERROR: No se pudo actualizar el producto.";
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public static string EliminarProducto(long codigoBarra)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Productos WHERE CodigoBarra = @CodigoBarra";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CodigoBarra", codigoBarra);
                        return command.ExecuteNonQuery() > 0 ? "ELIMINADO" : "ERROR: No se pudo eliminar el producto.";
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


