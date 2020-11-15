using LiteDBManager.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDBManager.Services
{
    public class DbConnections
    {
        /// <summary>
        /// Lista que contiene todas las conexiones abiertas con una base de datos.
        /// </summary>
        public static List<DbConnection> Connections { get; } = new List<DbConnection>();

        public static DbConnection CurrentConnection { get; set; }
    }
}
