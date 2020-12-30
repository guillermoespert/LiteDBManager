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

namespace LiteDBManager.UIElements.DocumentViewer
{
    /// <summary>
    /// Lógica de interacción para DocumentViewerLineGroup.xaml
    /// </summary>
    public partial class DocumentViewerLineGroup : UserControl, DocumentEditorComponent
    {
        public KeyValuePair<string, BsonValue> ChildDocument { get; private set; }

        public int DocumentNestingLevel { get; private set; }

        public int LineCounter { get; private set; }

        public bool IsInEditionMode 
        {
            get { return isInEditionMode; } 
        }

        public bool IsGroupCollapsed 
        { 
            get { return isGroupFold; }
            set
            {
                isGroupFold = value;
                FoldAllChildren(isGroupFold);
            }
        }

        public event EventHandler<EventArgs> LinesAdded;

        public DocumentViewerLineGroup(KeyValuePair<string, BsonValue> childDocument, int nestingLevel, int lineCounter)
        {
            ChildDocument = childDocument;
            DocumentNestingLevel = nestingLevel;
            LineCounter = lineCounter;

            InitializeComponent();
        }

        public int LoadDocument()
        {
            if (ChildDocument.Value is BsonDocument)
            {
                var doc = ChildDocument.Value as BsonDocument;
                var tabLength = DocumentViewerLine.DEFAULT_TAB_LENGTH;
                int bracketsTab = (DocumentNestingLevel - 1 > 0) ? (DocumentNestingLevel - 1) * tabLength : tabLength;

                isGroupFold = true;

                //Insertar la línea de comienzo
                var startLine = new DocumentViewerLine(ChildDocument, LineCounter, LineType.NestedObjectOpening, bracketsTab);
                startLine.IsGroupCollapsed = true;
                startLine.FoldActionRequest += FoldableStartLine_FoldActionRequest;
                startLine.InnerDocumentDelete += StartLine_InnerDocumentDelete;
                startLine.AddNewLine += Children_AddNewLine;
                startLine.UpdatedLineType += Children_UpdatedLineType;
                stpLinesContainer.Children.Add(startLine);

                foreach (var line in doc)
                {
                    if (line.Value is BsonDocument)
                    {
                        var childDoc = new DocumentViewerLineGroup(line, DocumentNestingLevel + 1, ++LineCounter);
                        LineCounter = childDoc.LoadDocument();
                        childDoc.LinesAdded += ChildDoc_LinesAdded;
                        stpLinesContainer.Children.Add(childDoc);
                    }
                    else
                    {
                        //Insertar las líneas de datos
                        var docline = new DocumentViewerLine(line, ++LineCounter, DocumentNestingLevel * tabLength);
                        docline.AddNewLine += Children_AddNewLine;
                        docline.UpdatedLineType += Children_UpdatedLineType;
                        docline.Visibility = (DocumentNestingLevel > 1) ? Visibility.Collapsed : Visibility.Visible;
                        docline.IsGroupCollapsed = true;
                        stpLinesContainer.Children.Add(docline);
                    }
                }

                //Insertar la línea de cierre
                DocumentViewerLine dvl = new DocumentViewerLine(LineType.Closing, ++LineCounter, bracketsTab);
                dvl.Visibility = (DocumentNestingLevel > 1) ? Visibility.Collapsed : Visibility.Visible;
                dvl.IsGroupCollapsed = true;
                stpLinesContainer.Children.Add(dvl);
            }

            return LineCounter;
        }

        private void ChildDoc_LinesAdded(object sender, EventArgs e)
        {
            LinesAdded?.Invoke(this, new EventArgs());
        }

        private void Children_UpdatedLineType(object sender, LineEventArgs e)
        {
            var index = stpLinesContainer.Children.IndexOf(sender as UIElement);

            if(index > -1)
            {
                var newDoc = new BsonDocument();
                var value = new BsonValue("");
                newDoc.Add("newValue", value);
                var newLine = new KeyValuePair<string, BsonValue>("newDocument", newDoc);
                var childDoc = new DocumentViewerLineGroup(newLine, DocumentNestingLevel + 1, ++LineCounter);
                LineCounter = childDoc.LoadDocument();
                childDoc.LinesAdded += ChildDoc_LinesAdded;
                ((DocumentEditorComponent)childDoc).ActivateEdition();
                stpLinesContainer.Children.Remove(sender as UIElement);
                stpLinesContainer.Children.Insert(index, childDoc);

                LinesAdded?.Invoke(this, new EventArgs());
            }
        }

        private void Children_AddNewLine(object sender, LineEventArgs e)
        {
            var line = sender as DocumentViewerLine;

            if(line != null)
            {
                var index = stpLinesContainer.Children.IndexOf(line);

                if(index > -1)
                {
                    BsonValue value = new BsonValue("");
                    DocumentViewerLine newLine = new DocumentViewerLine("newVariable", value, e.LineNumber + 1, DocumentNestingLevel * DocumentViewerLine.DEFAULT_TAB_LENGTH);
                    newLine.UpdatedLineType += Children_UpdatedLineType;
                    newLine.AddNewLine += Children_AddNewLine;
                    newLine.InnerDocumentDelete += StartLine_InnerDocumentDelete;
                    ((DocumentEditorComponent)newLine).ActivateEdition();

                    stpLinesContainer.Children.Insert(index + 1, newLine);

                    LinesAdded?.Invoke(this, new EventArgs());
                }
            }
        }

        private void StartLine_InnerDocumentDelete(object sender, LineEventArgs e)
        {
            DeleteGroup();
        }

        private void FoldableStartLine_FoldActionRequest(object sender, FoldEventArgs e)
        {
            IsGroupCollapsed = !IsGroupCollapsed;
        }

        public void FoldAllChildren(bool status)
        {
            for (int i = 0; i < stpLinesContainer.Children.Count; i++)
            {
                var child = stpLinesContainer.Children[i];

                if (child is DocumentEditorComponent)
                {
                    var elm = child as DocumentEditorComponent;
                    Visibility newVisibility = status ? Visibility.Collapsed : Visibility.Visible;
                    elm.IsGroupCollapsed = status;

                    if (i != 0)
                    {
                        child.Visibility = newVisibility;
                    }
                }
            }
        }

        public void ActivateEdition()
        {
            foreach (var child in stpLinesContainer.Children)
            {
                if (child is DocumentEditorComponent)
                {
                    var line = child as DocumentEditorComponent;

                    if (line != null)
                    {
                        line.ActivateEdition();
                    }
                }
            }

            isInEditionMode = true;
        }

        public void CancelEdition()
        {
            foreach (var child in stpLinesContainer.Children)
            {
                if (child is DocumentEditorComponent)
                {
                    var line = child as DocumentEditorComponent;

                    if (line != null)
                    {
                        line.CancelEdition();
                    }
                }
            }

            isInEditionMode = false;
        }

        public void DeleteGroup()
        {
            foreach (var child in stpLinesContainer.Children)
            {
                if (child is DocumentViewerLine)
                {
                    var line = child as DocumentViewerLine;
                    line.SetAsDeleted();
                }
                else if (child is DocumentViewerLineGroup)
                {
                    var group = child as DocumentViewerLineGroup;
                    group.DeleteGroup();
                }
            }
        }

        public KeyValuePair<string, BsonValue> ParseDocument()
        {
            Dictionary<string, BsonValue> docLines = new Dictionary<string, BsonValue>();
            string docKey = "";

            //Desactivar el modo edición
            foreach (var child in stpLinesContainer.Children)
            {
                if (child is DocumentViewerLine)
                {
                    var line = child as DocumentViewerLine;

                    line.CommitEdition();

                    if (line.LineType == LineType.DataDisplay)
                    {
                        docLines.Add(line.LineKey, line.LineValue);
                    }
                    else if (line.LineType == LineType.NestedObjectOpening)
                    {
                        docKey = line.LineKey;
                    }

                    line.CancelEdition();
                }
                else if(child is DocumentViewerLineGroup)
                {
                    var group = child as DocumentViewerLineGroup;
                    var childDoc = group.ParseDocument();
                    docLines.Add(childDoc.Key, childDoc.Value);
                }
            }

            if(!string.IsNullOrWhiteSpace(docKey))
            {
                return new KeyValuePair<string, BsonValue>(docKey, new BsonDocument(docLines));
            }

            throw new Exception("Key no puede estar vacío");
        }

        /// <summary>
        /// Actualiza los números de línea del grupo.
        /// El número pasado como parámetro se usa para numerar la primera línea
        /// del grupo. Posteriormente se incrementa este valor para cada línea 
        /// del grupo. Una vez actualizadas todas la líneas, se devuelve el 
        /// valor de la última línea + 1.
        /// </summary>
        /// <param name="lineNumberStart">Número de la primera línea</param>
        /// <returns>Número de la última línea + 1</returns>
        public int UpdateLineNumbers(int lineNumberStart)
        {
            foreach(var child in stpLinesContainer.Children)
            {
                if(child is DocumentViewerLine)
                {
                    var line = child as DocumentViewerLine;
                    line.LineNumber = lineNumberStart;
                    lineNumberStart++;
                }
                else if(child is DocumentViewerLineGroup)
                {
                    var group = child as DocumentViewerLineGroup;
                    lineNumberStart = group.UpdateLineNumbers(lineNumberStart);
                }
            }

            return lineNumberStart;
        }

        #region Variables privadas

        private bool isInEditionMode;
        private bool isGroupFold;

        #endregion
    }
}
