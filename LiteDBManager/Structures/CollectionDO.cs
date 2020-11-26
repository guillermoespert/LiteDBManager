using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDBManager.Structures
{
    public class CollectionDO
    {
        public string CollectionName { get; set; }
        public List<string> Documents { get; set; } = new List<string>();
    }
}
