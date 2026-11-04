using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace Entradas
{
    internal class ConexionBD
    {
        private string stringConexion = "Server=localhost;Port=3306;Database=supervivencia;User=root;Pwd=;";

        // NUEVO MÉTODO: Registra al jugador antes de guardar la partida
        public void RegistrarJugador(int jugadorId, string nombreJugador, string nombreUsuarioJugador, string emailJugador, string telefonoJugador)
        {
            // Ajusta los nombres de las columnas ('jugador_id', 'nombre') según tu tabla real
            string query = "INSERT INTO jugadores (nombre, nombre_usuario, email, teléfono)" + 
                           "VALUES (@nombre, @nombre_usuario, @email, @teléfono)";

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
                    System.Diagnostics.Debug.WriteLine($"El jugador con ID {jugadorId} ya estaba registrado. Continuamos...");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error al registrar jugador: " + ex.Message);
                }
            }
        }
    }
}