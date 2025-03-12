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
                if (datos.Length < 16)
                    return "ERROR: Faltan datos.";

                if (!long.TryParse(datos[0], out long codigoBarra))
                    return $"ERROR: Código de barra inválido. Recibido: '{datos[0]}'";

                string plu = datos[1];
                string nombre = datos[2];

                if (!TryParseDecimal(datos[3], out decimal precio)) return "ERROR: Precio inválido.";
                if (!TryParseDecimal(datos[4], out decimal costo)) return "ERROR: Costo inválido.";
                if (!TryParseDecimal(datos[5], out decimal precioPromo)) return "ERROR: Precio promocional inválido.";
                if (!int.TryParse(datos[6], out int iva)) return "ERROR: IVA inválido.";
                bool activo = datos[7] == "1" || datos[7].ToLower() == "true";
                if (!int.TryParse(datos[8], out int existencia)) return "ERROR: Existencia inválida.";
                if (!int.TryParse(datos[9], out int extMinima)) return "ERROR: Ext_Minima inválida.";
                if (!int.TryParse(datos[10], out int extMaxima)) return "ERROR: Ext_Maxima inválida.";
                if (!TryParseDecimal(datos[11], out decimal rentabilidadMinima)) return "ERROR: Rentabilidad mínima inválida.";
                if (!TryParseDecimal(datos[12], out decimal avgVenta)) return "ERROR: Promedio de venta inválido.";
                if (!TryParseDecimal(datos[13], out decimal avgCompra)) return "ERROR: Promedio de compra inválido.";

                if (!TryParseFecha(datos[14], out DateTime ultimaCompra))
                    return "ERROR: Fecha de última compra inválida. Formato esperado: yyyy-MM-dd HH:mm:ss.";
                if (!TryParseFecha(datos[15], out DateTime ultimaVenta))
                    return "ERROR: Fecha de última venta inválida. Formato esperado: yyyy-MM-dd HH:mm:ss.";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"UPDATE Productos 
                                SET PLU = @PLU, Nombre = @Nombre, Precio = @Precio, Costo = @Costo, PrecioPromo = @PrecioPromo, 
                                    IVA = @IVA, Activo = @Activo, Existencia = @Existencia, Ext_Minima = @Ext_Minima, 
                                    Ext_Maxima = @Ext_Maxima, Rentabilidad_Minima = @Rentabilidad_Minima, Avg_Venta = @Avg_Venta, 
                                    Avg_Compra = @Avg_Compra, Ultima_Compra = @Ultima_Compra, Ultima_Venta = @Ultima_Venta 
                                WHERE CodigoBarra = @CodigoBarra";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(new SqlParameter[]
                        {
                        new SqlParameter("@CodigoBarra", codigoBarra),
                        new SqlParameter("@PLU", plu),
                        new SqlParameter("@Nombre", nombre),
                        new SqlParameter("@Precio", precio),
                        new SqlParameter("@Costo", costo),
                        new SqlParameter("@PrecioPromo", precioPromo),
                        new SqlParameter("@IVA", iva),
                        new SqlParameter("@Activo", activo),
                        new SqlParameter("@Existencia", existencia),
                        new SqlParameter("@Ext_Minima", extMinima),
                        new SqlParameter("@Ext_Maxima", extMaxima),
                        new SqlParameter("@Rentabilidad_Minima", rentabilidadMinima),
                        new SqlParameter("@Avg_Venta", avgVenta),
                        new SqlParameter("@Avg_Compra", avgCompra),
                        new SqlParameter("@Ultima_Compra", ultimaCompra),
                        new SqlParameter("@Ultima_Venta", ultimaVenta)
                        });

                        int filasAfectadas = command.ExecuteNonQuery();
                        return filasAfectadas > 0 ? "ACTUALIZADO" : "ERROR: No se pudo actualizar el producto.";
                    }
                }
            }
            catch (SqlException)
            {
                return "ERROR SQL: Ocurrió un problema con la base de datos.";
            }
            catch (Exception)
            {
                return "ERROR GENERAL: Se produjo un error inesperado.";
            }
        }

        private static bool TryParseDecimal(string input, out decimal result)
        {
            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseFecha(string input, out DateTime fecha)
        {
            return DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
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


