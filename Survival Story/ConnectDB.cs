using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace Survival_Story
{
    internal class ConnectDB {

        public void guardarProgreso(string nombrejugador, int nivel)
        {
            string conexion = "Server=localHost;Port=3306;Database=base_datos;Pwd=;";
            string query = "INSERT INTO progreso (nombreJugador, nivel) VALUES (@nombreJugador, @nivel)";

            try
            {
                using (MySqlConnection newConexion = new MySqlConnection(conexion)
                {
                    using (MySqlCommand = n)
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("MySql Error", ex.Message);
            } 
        }
    }
}
