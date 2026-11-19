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

        //Inserts
        public static int RegistrarJugador(string playerName)
        {
            string selectSql = "SELECT id FROM player WHERE name = @name LIMIT 1;";
            string insertSql = "INSERT INTO player (name) VALUES (@name); SELECT LAST_INSERT_ID();";

            using (MySqlConnection conexion = new MySqlConnection(stringConexion))
            {
                conexion.Open();

                using (MySqlCommand select = new MySqlCommand(selectSql, conexion))
                {
                    select.Parameters.AddWithValue("@name", playerName);
                    object resultado = select.ExecuteScalar();

                    if (resultado != null && resultado != DBNull.Value)
                    {
                        System.Diagnostics.Debug.WriteLine($"Sobrescribiendo el jugador {playerName}...");
                        return Convert.ToInt32(resultado);
                    }
                }

                // Si no existe, creamos el jugador y recuperamos su nuevo id
                using (MySqlCommand insert = new MySqlCommand(insertSql, conexion))
                {
                    System.Diagnostics.Debug.WriteLine($"Creando el {playerName}...");
                    insert.Parameters.AddWithValue("@name", playerName);
                    object newId = insert.ExecuteScalar();
                    return Convert.ToInt32(newId);
                }
            }
        }
        public static void GuardarSesion(string playerName, int sessionDay, float sessionTime)
        {
            try
            {
                int playerId = RegistrarJugador(playerName);
                GuardarSesionPorId(playerId, playerId, sessionDay, sessionTime);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al guardar sesión por nombre: " + ex.Message);
            }
        }

        public static void GuardarSesionPorId(int sessionId, int playerId, int sessionDay, float sessionTime)
        {
            string query = @"
                INSERT INTO session (id, id_player, survived_days, time_day)
                VALUES (@sessionId, @playerId, @sessionDay, @sessionTime)
                ON DUPLICATE KEY UPDATE
                    survived_days = VALUES(survived_days),
                    time_day = VALUES(time_day);";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@sessionId", sessionId);
                        cmd.Parameters.AddWithValue("@playerId", playerId);
                        cmd.Parameters.AddWithValue("@sessionDay", sessionDay);
                        cmd.Parameters.AddWithValue("@sessionTime", sessionTime);

                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Sesión sobrescrita/guardada para playerId={playerId} (días: {sessionDay}, tiempo: {sessionTime}).");
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en base de datos al guardar sesión por ID: " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al guardar sesión por ID: " + ex.Message);
            }
        }

        public static (bool encontrada, int survivedDays, float timeDay) CargarSesion(string playerName)
        {
            string cleanName = playerName?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(cleanName))
                return (false, 1, 0f);

            string query = @"
                SELECT s.survived_days, s.time_day
                FROM session s
                INNER JOIN player p ON p.id = s.id_player
                WHERE p.name = @name
                LIMIT 1;";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@name", cleanName);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int days = reader.IsDBNull(reader.GetOrdinal("survived_days"))
                                    ? 1
                                    : reader.GetInt32("survived_days");

                                float time = reader.IsDBNull(reader.GetOrdinal("time_day"))
                                    ? 0f
                                    : reader.GetFloat("time_day");

                                return (true, days, time);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al cargar sesión: " + ex.Message);
            }

            return (false, 1, 0f);
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