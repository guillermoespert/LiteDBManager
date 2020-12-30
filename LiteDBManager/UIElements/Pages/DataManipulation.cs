using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDBManager.UIElements.Pages
{
    public interface DataManipulation
    {
        void ImportSelected();

        void ExportSelected();

        void AddItem();
    }
}
