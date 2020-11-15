using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LiteDBManager.UIElements.DocumentViewer
{
    public struct FoldingGroup
    {
        public int LineStart { get; set; }
        public int LinesCount { get; set; }
        public Visibility Visibility { get; set; }
    }
}
