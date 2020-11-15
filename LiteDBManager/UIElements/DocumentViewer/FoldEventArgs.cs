using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LiteDBManager.UIElements.DocumentViewer
{
    public class FoldEventArgs : EventArgs
    {
        public int FoldingGroup { get; set; }
    }
}
