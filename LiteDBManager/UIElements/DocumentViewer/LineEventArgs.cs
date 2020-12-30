using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDBManager.UIElements.DocumentViewer
{
    public class LineEventArgs : EventArgs
    {
        public int LineNumber { get; set; }
        public string LineKey { get; set; }
        public LineType OriginalLineType { get; set; }
        public BsonType NewLineBsonType { get; set; }
    }
}
