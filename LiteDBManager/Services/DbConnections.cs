using LiteDBManager.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDBManager.Services
{
    /// <summary>
    /// Esta clase actúa como contenedor principal de almacenamiento de las conexiones abiertas
    /// con las diferentes bases de datos.
    /// </summary>
    public class DbConnections
    {
        /// <summary>
        /// Lista que contiene todas las conexiones abiertas con una base de datos.
        /// </summary>
        public static List<DbConnection> Connections { get; } = new List<DbConnection>();

        /// <summary>
        /// Contiene una referencia a la conexión activa. La conexión activa
        /// es aquella que está siendo mostrada en la interfaz.
        /// </summary>
        public static DbConnection CurrentConnection { get; set; }
    }
}
