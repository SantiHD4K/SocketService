using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocketService.Security
{
    public static class JwtHelper
    {
        private static string SecretKey = ConfigurationManager.AppSettings["JwtSecretKey"];

        public static string GenerateJwtToken(string usuario, string dispositivo)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var signinCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("usuario", usuario),
                new Claim("dispositivo", dispositivo),
                new Claim("fecha", DateTime.Now.ToString("o"))
            };

            var tokenOptions = new JwtSecurityToken(
                issuer: "TuServidor",
                audience: "TuCliente",
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                signingCredentials: signinCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public static bool ValidateTokenAndUser(string token, string usuario)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Expiracion, Usuario FROM Sesiones WHERE Token = @Token";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Token", token);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                DateTime expiracion = reader.GetDateTime(0);
                                string usuarioAsociado = reader.GetString(1);

                                if (DateTime.Now <= expiracion && usuarioAsociado == usuario)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validando el token: {ex.Message}");
                return false;
            }
        }
        public static string Login(string[] datos)
        {
            try
            {
                string usuario = datos[1];
                string dispositivo = datos[2];

                string token = GenerateJwtToken(usuario, dispositivo);

                DateTime fechaHora = DateTime.Now;
                DateTime expiracion = fechaHora.AddHours(12);

                string connectionString = ConfigurationManager.ConnectionStrings["SQLServer"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Sesiones (Usuario, Dispositivo, Token, FechaHora, Expiracion) VALUES (@Usuario, @Dispositivo, @Token, @FechaHora, @Expiracion)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Usuario", usuario);
                        command.Parameters.AddWithValue("@Dispositivo", dispositivo);
                        command.Parameters.AddWithValue("@Token", token);
                        command.Parameters.AddWithValue("@FechaHora", fechaHora);
                        command.Parameters.AddWithValue("@Expiracion", expiracion);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return $"LOGIN_EXITOSO|{token}";
                        }
                        else
                        {
                            Console.WriteLine("Error al insertar la sesión, filas afectadas: 0");
                            return "ERROR: No se insertaron filas.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al realizar el login: {ex.Message}\n{ex.StackTrace}");
                return "ERROR: " + ex.Message;
            }
        }

    }
}
