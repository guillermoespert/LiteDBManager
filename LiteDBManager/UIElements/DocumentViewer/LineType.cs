using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDBManager.UIElements.DocumentViewer
{
    public enum LineType
    {
        Opening,
        Closing,
        NestedObjectOpening,
        NestedObjectClosing,
        DataDisplay,
        DeletedLine,
    }
}
