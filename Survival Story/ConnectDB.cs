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
        private static string stringConexion = "Server=localhost;Port=3306;Database=survival;User=root;Pwd=;";

        // NUEVO MÉTODO: Registra al jugador antes de guardar la partida
        public static void RegistrarJugador(int id, string name)
        {
            // Ajusta los nombres de las columnas ('jugador_id', 'nombre') según tu tabla real
            string query = "INSERT INTO player (id, name)" +
                           "VALUES (@id_player, @playerName)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_player", id);
                        comando.Parameters.AddWithValue("@playerName", name);

                        comando.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Se ha registrado el jugador {name}");
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
        /*
        public static void RegistrarEstado(int id, int id_session, int playerHealth, int playerEnergy, int playerHunger, string playerMoral)
        {
            // Ajusta los nombres de las columnas ('jugador_id', 'nombre') según tu tabla real
            string query = "INSERT INTO player (id, id_session, health, energy, hunger, moral)" +
                           "VALUES (@id_player, @id_session, @playerHealth, @playerEnergy, @playerHunger, @playerMoral)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", idPlayer);
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
    
        public static (float x, float y, int dia) CargarPartida(int jugadorId)
        {
            string query = "SELECT player_x, player_y, current_day FROM player_save WHERE id = @id;";
    
        // Valores por defecto por si la consulta no devuelve nada
            float x = 10 * 71; // Tu posición inicial por defecto
            float y = 10 * 71;
            int dia = 1;

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", jugadorId);
                        using (MySqlDataReader lector = comando.ExecuteReader())
                        {
                            if (lector.HasRows && lector.Read())
                            {
                                x = lector.GetFloat("player_x");
                                y = lector.GetFloat("player_y");
                                dia = lector.GetInt32("current_day");
                                System.Diagnostics.Debug.WriteLine("Datos de partida cargados desde la BD.");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al cargar partida: " + ex.Message);
            }

            return (x, y, dia);
        }*/
    /*
    var recurso = ConnectDB.AsignarVariablesRecurso(5);
    string nombre = recurso.nombre;
    int cantidad = recurso.cantidad;
     */
    }
}