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
        private static string stringConexion = "Server=localhost;Port=3307;Database=survival;User=root;Pwd=;";

        // NUEVO MÉTODO: Registra al jugador antes de guardar la partida

        public struct playerData
        {
            public int id { get; set; }
            public string name { get; set; }
            public int axisX { get; set; }
            public int axisY { get; set; }
        }
        public struct playerStateData
        {
            public int id { get; set; }
            public int id_session { get; set; }
            public string moral { get; set; }
            public int energy { get; set; }
            public int health { get; set; }
            public int hunger { get; set; }
        }
        public struct actionLogData
        {
            public int id { get; set; }
            public int id_session { get; set; }
            public int id_actionTypes { get; set; }
            public int day { get; set; }
            public string result { get; set; }
        }


        public struct sesionResultData
        {
            public int id { get; set; }
            public int id_session { get; set; }
            public int total_days { get; set; }
            public int remain_resources { get; set; }
            public int score { get; set; }
        }

        public struct sessionData
        {
            public int id { get; set; }
            public int id_player { get; set; }
            public int survived_days { get; set; }
            public float time_day { get; set; }
        }

        public struct resources
        {
            public int id { get; set; }
            public string name { get; set; }
            public string terrain { get; set; }
        }

        public struct mapResources
        {
            public int id { get; set; }
            public int id_resources { get; set; }
            public int id_session { get; set; }
            public int axisX { get; set; }
            public int axisY { get; set; }
            public int active { get; set; }
        }

        public struct animals
        {
            public int id { get; set; }
            public string name { get; set; }
            public string terrain { get; set; }
        }

        public struct mapAnimals
        {
            public int id { get; set; }
            public int id_animals { get; set; }
            public int id_session { get; set; }
            public int axisX { get; set; }
            public int axisY { get; set; }
            public int active { get; set; }
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

        public static void mapResources(mapResources data)
        {
            string updateQuery = @"
                UPDATE mapResources
                SET = axisX = @axisX,
                      axisY = @axisY,
                      active = @active
                WHERE id_session = @id_session;";

            string insertQuery = @"
                INSERT INTO mapResources (id_resources, id_session, axisX, axisY, active)
                VALUES (@id_resources,@id_session, @axisX, @axisY, @active);";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conexion))
                    {
                        updateCmd.Parameters.AddWithValue("@id_resources", data.id_resources);
                        updateCmd.Parameters.AddWithValue("@id_session", data.id_session);
                        updateCmd.Parameters.AddWithValue("@axisX", data.axisX);
                        updateCmd.Parameters.AddWithValue("@axisY", data.axisY);
                        updateCmd.Parameters.AddWithValue("@active", data.active);

                        int datos = updateCmd.ExecuteNonQuery();

                        if (datos == 0)
                        {
                            using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conexion))
                            {
                                updateCmd.Parameters.AddWithValue("@id_resources", data.id_resources);
                                updateCmd.Parameters.AddWithValue("@id_session", data.id_session);
                                updateCmd.Parameters.AddWithValue("@axisX", data.axisX);
                                updateCmd.Parameters.AddWithValue("@axisY", data.axisY);
                                updateCmd.Parameters.AddWithValue("@active", data.active);

                                insertCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Inserción de recursos a la base de datos.");
                                return;
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"Actualización de recursos en la base de datos.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL en Sesion ({ex.Number}): {ex.Message}");
            }
        }

        public static void playerState(playerStateData session)
        {
            // 1. Se quitó la coma después de @moral
            string updateQuery = @"
        UPDATE playerStateLog
        SET energy = @energy,
            health = @health,
            hunger = @hunger,
            moral = @moral
        WHERE id_session = @id_session;";

            // 2. Revisá si la tabla aquí debe ser 'playerStateLog' en lugar de 'session'
            string insertQuery = @"
        INSERT INTO playerStateLog (id_session, moral, energy, health, hunger)
        VALUES (@id_session, @moral, @energy, @health, @hunger);";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conexion))
                    {
                        // 3. Nombres de propiedades corregidos (energy, health, hunger, moral)
                        updateCmd.Parameters.AddWithValue("@id_session", session.id_session);
                        updateCmd.Parameters.AddWithValue("@energy", session.energy);
                        updateCmd.Parameters.AddWithValue("@health", session.health);
                        updateCmd.Parameters.AddWithValue("@hunger", session.hunger);
                        updateCmd.Parameters.AddWithValue("@moral", session.moral);

                        int datos = updateCmd.ExecuteNonQuery();

                        if (datos == 0)
                        {
                            using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conexion))
                            {
                                insertCmd.Parameters.AddWithValue("@id_session", session.id_session);
                                insertCmd.Parameters.AddWithValue("@energy", session.energy);
                                insertCmd.Parameters.AddWithValue("@health", session.health);
                                insertCmd.Parameters.AddWithValue("@hunger", session.hunger);
                                insertCmd.Parameters.AddWithValue("@moral", session.moral);
                                insertCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Nueva sesión creada para id_player={session.id_session}");
                                return;
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"Sesión actualizada para id_session={session.id_session}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL en Sesion ({ex.Number}): {ex.Message}");
            }
        }

        public static void sesionResult(sesionResultData result)
        {
            string updateQuery = @"
        UPDATE sesionResult
        SET total_days = @total_days,
            remain_resources = @remain_resources,
            score = @score
        WHERE id_session = @id_session;";

            string insertQuery = @"
        INSERT INTO sesionResult (id_session, total_days, remain_resources, score)
        VALUES (@id_session, @total_days, @remain_resources, @score);";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conexion))
                    {
                        updateCmd.Parameters.AddWithValue("@id_session", result.id_session);
                        updateCmd.Parameters.AddWithValue("@total_days", result.total_days);
                        updateCmd.Parameters.AddWithValue("@remain_resources", result.remain_resources);
                        updateCmd.Parameters.AddWithValue("@score", result.score);

                        int datos = updateCmd.ExecuteNonQuery();

                        if (datos == 0)
                        {
                            using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conexion))
                            {
                                insertCmd.Parameters.AddWithValue("@id_session", result.id_session);
                                insertCmd.Parameters.AddWithValue("@total_days", result.total_days);
                                insertCmd.Parameters.AddWithValue("@remain_resources", result.remain_resources);
                                insertCmd.Parameters.AddWithValue("@score", result.score);
                                insertCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Nuevo resultado creado para id_session={result.id_session}");
                                return;
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"Resultado actualizado para id_session={result.id_session}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL en sesionResult ({ex.Number}): {ex.Message}");
            }
        }
        public static void actionLogs(actionLogData log)
        {
            string insertQuery = @"
        INSERT INTO actionLogs (id_session, id_actionTypes, day, result)
        VALUES (@id_session, @id_actionTypes, @day, @result);";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conexion))
                    {
                        insertCmd.Parameters.AddWithValue("@id_session", log.id_session);
                        insertCmd.Parameters.AddWithValue("@id_actionTypes", log.id_actionTypes);
                        insertCmd.Parameters.AddWithValue("@day", log.day);
                        insertCmd.Parameters.AddWithValue("@result", log.result);

                        insertCmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Acción registrada para id_session={log.id_session}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL en actionLogs ({ex.Number}): {ex.Message}");
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

        public static playerStateData? CargarEstadoJugador(int idSession)
        {
            string selectQuery = @"
        SELECT id, id_session, moral, energy, health, hunger
        FROM playerStateLog
        WHERE id_session = @id_session
        LIMIT 1;";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id_session", idSession);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new playerStateData
                                {
                                    id = reader.GetInt32("id"),
                                    id_session = reader.GetInt32("id_session"),
                                    moral = reader.GetString("moral"),
                                    energy = reader.GetInt32("energy"),
                                    health = reader.GetInt32("health"),
                                    hunger = reader.GetInt32("hunger")
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL en CargarEstadoJugador ({ex.Number}): {ex.Message}");
            }

            return null;
        }

        public static mapResources? CargarMapaRecursos(int id_session)
        {
            string selectQuery = @"
            SELECT axisX, axisY, active
            FROM mapresources
            WHERE id_session = @id_session
            LIMIT 1;";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id_session", idSession);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new mapResources
                                {
                                    id = reader.GetInt32("id"),
                                    id_resources = reader.GetInt32("id_resources"),
                                    id_session = reader.GetInt32("id_session"),
                                    axisX = reader.GetInt32("axisX"),
                                    axisY = reader.GetInt32("axisY"),
                                    active = reader.GetInt32("active")
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MySQL en CargarEstadoJugador ({ex.Number}): {ex.Message}");
            }

            return null;
        }
    }
}

        