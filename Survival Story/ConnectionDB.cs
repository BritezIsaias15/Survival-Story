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
        private static string stringConexion = "Server=localhost;Port=3307;Database=supervivencia;User=root;Pwd=;";

        // NUEVO MÉTODO: Registra al jugador antes de guardar la partida

        //Inserts
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