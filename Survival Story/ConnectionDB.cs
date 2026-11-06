using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace Entradas //Código sin uso, modificar y copiar para las distintas tablas de la base d datos
{
    internal static class ConnectDB
    {
        private static string stringConexion = "Server=localhost;Port=3307;Database=supervivencia;User=root;Pwd=;";

        // NUEVO MÉTODO: Registra al jugador antes de guardar la partida
        public static void RegistrarJugador(int id, string nombreJugador, string nombreUsuarioJugador, string emailJugador, int telefonoJugador)
        {
            // Ajusta los nombres de las columnas ('jugador_id', 'nombre') según tu tabla real
            string query = "INSERT INTO player (id, nombre, nombre_usuario, email, teléfono)" +
                           "VALUES (@id, @nombre, @nombre_usuario, @email, @teléfono)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", nombreJugador);
                        comando.Parameters.AddWithValue("@nombre_usuario", nombreUsuarioJugador);
                        comando.Parameters.AddWithValue("@email", emailJugador);
                        comando.Parameters.AddWithValue("@teléfono", telefonoJugador);

                        comando.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Jugador '{nombreJugador}' registrado con éxito.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Si el ID ya existe, MySQL dará error de llave primaria duplicada (código 1062)
                if (ex.Number == 1062)
                {
                    System.Diagnostics.Debug.WriteLine($"Ya existe un jugador con el id '{id}. Continuamos...");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error al registrar jugador: " + ex.Message);
                }
            }
        } 
        public static void ConsultarElemento(int id)
        {
            string query = "SELECT * FROM resources " +
                           "WHERE id = @id";
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        using(MySqlDataReader lector = comando.ExecuteReader())
                        {
                            if(lector.HasRows)
                            {
                                while(lector.Read())
                                {
                                    int resourceId = lector.GetInt32("id");
                                    string nombre = lector.GetString("nombre");
                                    System.Diagnostics.Debug.WriteLine($"Id: {resourceId}, Nombre: {nombre}");
                                }
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error. {ex.Message}");
            }
        }
    }
}