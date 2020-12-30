using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDBManager.UIElements.DocumentViewer
{
    public interface DocumentEditorComponent
    {
        bool IsGroupCollapsed { get; set; }
        void ActivateEdition();
        void CancelEdition();
    }
}
