using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Data;

namespace Entradas //Código sin uso, modificar y copiar para las distintas tablas de la base d datos
{
    internal static class ConnectDB
    {
        private static string stringConexion = "Server=localhost;Port=3306;Database=survival;User=root;Pwd=;";

        // NUEVO MÉTODO: Registra al jugador antes de guardar la partida
        
        public struct playerData
        {
            public int id { get; set; }
            public string name { get; set; }
            public int axisX { get; set;}
            public int axisY { get; set; }
        }
        public struct sessionData
        {
            public int id { get; set; }
            public int id_player { get; set; }
            public int survived_days { get; set; }
            public float time_day { get; set; }
        }

        
        public static bool tablaVacia(string nombreTabla)
        {
            string query = $"SELECT COUNT(*) FROM {nombreTabla};";

            using (MySqlConnection conexion = new MySqlConnection(stringConexion))
            {
                conexion.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                {
                    long totalFilas = Convert.ToInt64(cmd.ExecuteScalar());
                    return totalFilas == 0;
                }
            }
        }

        public static void Jugador(playerData player)
        {
            string updateQuery = @"
                UPDATE player
                SET axisX = @axisX, axisY = @axisY
                WHERE name = @name;";

            string insertQuery = @"
                INSERT INTO player (name, axisX, axisY)
                VALUES (@name, @axisX, @axisY);";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conexion))
                    {
                        updateCmd.Parameters.AddWithValue("@name", player.name);
                        updateCmd.Parameters.AddWithValue("@axisX", player.axisX);
                        updateCmd.Parameters.AddWithValue("@axisY", player.axisY);

                        int datos = updateCmd.ExecuteNonQuery();

                        // Si no existía el jugador, recién ahí lo insertamos
                        if (datos == 0)
                        {
                            using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conexion))
                            {
                                insertCmd.Parameters.AddWithValue("@name", player.name);
                                insertCmd.Parameters.AddWithValue("@axisX", player.axisX);
                                insertCmd.Parameters.AddWithValue("@axisY", player.axisY);
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL en Jugador ({ex.Number}): {ex.Message}");
            }
        }

        public static int ObtenerIdJugador(string nombre)
        {
            string query = "SELECT id FROM player WHERE name = @name LIMIT 1;";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@name", nombre);
                        object resultado = cmd.ExecuteScalar();

                        if (resultado != null && resultado != DBNull.Value)
                        {
                            return Convert.ToInt32(resultado);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL al obtener id de jugador ({ex.Number}): {ex.Message}");
            }

            return 0;
        }
        public static void Sesion(sessionData session)
        {
            string updateQuery = @"
                UPDATE session
                SET survived_days = @sessionDay,
                    time_day = @sessionTime
                WHERE id_player = @idPlayer;";

            string insertQuery = @"
                INSERT INTO session (id_player, survived_days, time_day)
                VALUES (@idPlayer, @sessionDay, @sessionTime);";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conexion))
                    {
                        updateCmd.Parameters.AddWithValue("@idPlayer", session.id_player);
                        updateCmd.Parameters.AddWithValue("@sessionDay", session.survived_days);
                        updateCmd.Parameters.AddWithValue("@sessionTime", session.time_day);

                        int datos = updateCmd.ExecuteNonQuery();

                        if (datos == 0)
                        {
                            using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conexion))
                            {
                                insertCmd.Parameters.AddWithValue("@idPlayer", session.id_player);
                                insertCmd.Parameters.AddWithValue("@sessionDay", session.survived_days);
                                insertCmd.Parameters.AddWithValue("@sessionTime", session.time_day);

                                insertCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Nueva sesión creada para id_player={session.id_player}");
                                return;
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"Sesión actualizada para id_player={session.id_player}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL en Sesion ({ex.Number}): {ex.Message}");
            }
        }

        public static playerData? CargarJugador(string nombre)
        {
            string query = @"
                SELECT id, name, axisX, axisY
                FROM player
                WHERE name = @name
                LIMIT 1;";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@name", nombre);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new playerData
                                {
                                    id = reader.GetInt32("id"),
                                    name = reader.GetString("name"),
                                    axisX = reader.GetInt32("axisX"),
                                    axisY = reader.GetInt32("axisY")
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL al cargar jugador ({ex.Number}): {ex.Message}");
            }

            return null;
        }

        public static sessionData? CargarSesion(int idPlayer)
        {
            string query = @"
                SELECT id, id_player, survived_days, time_day
                FROM session
                WHERE id_player = @idPlayer
                LIMIT 1;";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@idPlayer", idPlayer);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new sessionData
                                {
                                    id = reader.GetInt32("id"),
                                    id_player = reader.GetInt32("id_player"),
                                    survived_days = reader.GetInt32("survived_days"),
                                    time_day = reader.GetFloat("time_day")
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL al cargar sesión ({ex.Number}): {ex.Message}");
            }

            return null;
        }        
        /*
        public static void InsertarPlayerStateLog(int idSession, int health, int energy, int hunger, string moral)
        {
            string query = "INSERT INTO playerStateLog (id_session, health, energy, hunger, moral) " +
                        "VALUES (@id_session, @health, @energy, @hunger, @moral)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_session", idSession);
                        comando.Parameters.AddWithValue("@health", health);
                        comando.Parameters.AddWithValue("@energy", energy);
                        comando.Parameters.AddWithValue("@hunger", hunger);
                        comando.Parameters.AddWithValue("@moral", moral);

                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al guardar estado: " + ex.Message);
            }
        }
        public static void InsertarSesionResult(int idSession, int totalDays, int remainResources, int score)
        {
            string query = "INSERT INTO sesionResult (id_session, total_days, remain_resources, score) " +
                        "VALUES (@id_session, @total_days, @remain_resources, @score)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_session", idSession);
                        comando.Parameters.AddWithValue("@total_days", totalDays);
                        comando.Parameters.AddWithValue("@remain_resources", remainResources);
                        comando.Parameters.AddWithValue("@score", score);

                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al guardar resultado: " + ex.Message);
            }
        }
        public static void InsertarActionLog(int idSession, int idActionTypes, int day, string result)
        {
            string query = "INSERT INTO actionLogs (id_session, id_actionTypes, day, result) " +
                        "VALUES (@id_session, @id_actionTypes, @day, @result)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_session", idSession);
                        comando.Parameters.AddWithValue("@id_actionTypes", idActionTypes);
                        comando.Parameters.AddWithValue("@day", day);
                        comando.Parameters.AddWithValue("@result", result);

                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al guardar acción: " + ex.Message);
            }
        }

        //Selecs
        public static DataTable SelectPlayerStateLog(int idSession)
        {
            string query = "SELECT * FROM playerStateLog WHERE id_session = @id_session";

            DataTable tabla = new DataTable();

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_session", idSession);

                        using (MySqlDataAdapter adaptador = new MySqlDataAdapter(comando))
                        {
                            adaptador.Fill(tabla);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al consultar playerStateLog: " + ex.Message);
            }

            return tabla;
        }
        public static DataTable SelectSesionResult(int idSession)
        {
            string query = "SELECT * FROM sesionResult WHERE id_session = @id_session";

            DataTable tabla = new DataTable();

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_session", idSession);

                        using (MySqlDataAdapter adaptador = new MySqlDataAdapter(comando))
                        {
                            adaptador.Fill(tabla);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al consultar sesionResult: " + ex.Message);
            }

            return tabla;
        }
        public static DataTable SelectActionLogs(int idSession)
        {
            string query = "SELECT * FROM actionLogs WHERE id_session = @id_session";

            DataTable tabla = new DataTable();

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_session", idSession);

                        using (MySqlDataAdapter adaptador = new MySqlDataAdapter(comando))
                        {
                            adaptador.Fill(tabla);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al consultar actionLogs: " + ex.Message);
            }

            return tabla;
        }

        //El delete
        public static void EliminarDatabase()
        {
            string query = "DROP DATABASE supervival";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine("Base de datos eliminada correctamente.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al eliminar la base de datos: " + ex.Message);
            }
        }

    }



    /*
    
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




    /*
     // Modificamos el método para que devuelva un Tuple (varios valores a la vez) o podés usar una clase propia
public static (string nombre, int cantidad) AsignarVariablesRecurso(int id)
{
    string query = "SELECT nombre, cantidad FROM resources WHERE id = @id";
    
    // Variables locales listas para recibir los valores de la consulta
    string nombreAsignado = "No encontrado";
    int cantidadAsignada = 0;

    try
    {
        using (MySqlConnection conexion = new MySqlConnection(stringConexion))
        {
            conexion.Open();
            using (MySqlCommand comando = new MySqlCommand(query, conexion))
            {
                comando.Parameters.AddWithValue("@id", id);
                using (MySqlDataReader lector = comando.ExecuteReader())
                {
                    if (lector.HasRows && lector.Read())
                    {
                        // ACÁ SE ASIGNAN LOS VALORES A LAS VARIABLES
                        nombreAsignado = lector.GetString("nombre");
                        cantidadAsignada = lector.GetInt32("cantidad");
                    }
                }
            }
        }
    }
    catch (MySqlException ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
    }

    // Retornamos las variables con los valores asignados
    return (nombreAsignado, cantidadAsignada);
}

    var recurso = ConnectDB.AsignarVariablesRecurso(5);
    string nombre = recurso.nombre;
    int cantidad = recurso.cantidad;
     */
    }
}
