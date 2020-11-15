using LiteDBManager.UIElements.DocumentViewer;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LiteDBManager.UIElements
{
    /// <summary>
    /// Lógica de interacción para DocumentViewerContainer.xaml
    /// </summary>
    public partial class DocumentViewerContainer : UserControl
    {
        public DocumentViewerContainer()
        {
            InitializeComponent();
        }

        public void LoadCollections(IList<BsonValue> values)
        {
            foreach(var value in values)
            {
                DocumentViewerControl dv = new DocumentViewerControl(value);
                dv.Margin = new Thickness(5, 10, 5, 0);
                stpDocumentsContainer.Children.Add(dv);
            }
        }
    }
}
