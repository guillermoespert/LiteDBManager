using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDBManager.Structures
{
    public class CollectionDO
    {
        /// <summary>
        /// Nombre de la colección
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// Contiene la serialización a JSON extendido del documento
        /// BSON.
        /// </summary>
        public List<string> Documents { get; set; } = new List<string>();
    }
}
