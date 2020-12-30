using LiteDB;
using LiteDBManager.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LiteDBManager.UIElements.DocumentViewer
{
    /// <summary>
    /// Lógica de interacción para DocumentViewerLiner.xaml
    /// </summary>
    public partial class DocumentViewerLine : UserControl, DocumentEditorComponent
    {
        #region Declaración de propiedades y eventos

        /// <summary>
        /// Ancho del tabulador para las llaves de apertura y cierre
        /// del documento.
        /// </summary>
        public const int MAIN_DOCUMENT_BRACKETS_TAB_LENGTH = 5;

        /// <summary>
        /// Ancho del tabulador para cada nivel de indentación de 
        /// las líneas de código.
        /// </summary>
        public const int DEFAULT_TAB_LENGTH = 20;

        /// <summary>
        /// Valor bruto original de la línea
        /// </summary>
        public KeyValuePair<string, BsonValue> RawLine { get; private set; }

        /// <summary>
        /// Clave de línea. Se corresponde con la el nombre del objeto BSON.
        /// </summary>
        public string LineKey { get; private set; }

        /// <summary>
        /// Valor de la línea. Se corresponde con el valor BSON.
        /// </summary>
        public BsonValue LineValue { get; private set; }

        /// <summary>
        /// Bandera que indica si la linea actual es de apertura o cierre
        /// de documento.
        /// </summary>
        public bool IsEndingOrStartingLine { get; set; }

        /// <summary>
        /// Valor de indentación de la línea
        /// </summary>
        public int LineTabLength { get; private set; }

        /// <summary>
        /// Bandera que indica si la variable puede ser editada
        /// </summary>
        public bool IsVariableEditable { get; private set; } = true;

        /// <summary>
        /// Bandera que indica si el valor puede ser editado
        /// </summary>
        public bool IsValueEditable { get; set; } = true;

        /// <summary>
        /// Número de la línea con respecto al documento.
        /// </summary>
        public int LineNumber
        {
            get { return lineNumber; }
            set
            {
                lineNumber = value;

                if (tbkLineNumber != null)
                    tbkLineNumber.Text = lineNumber.ToString() ?? "0";
            }
        }

        /// <summary>
        /// Activa o desactiva el modo de edición.
        /// Este valor no sobre escribe los valores de IsVariableEditable
        /// y IsValueEditable que siguen teniéndose en cuenta para activar
        /// o no finalmente la edición en la línea y de qué manera.
        /// </summary>
        public bool IsInEditionMode
        {
            get { return editionMode; }
        }

        /// <summary>
        /// Bandera que indica si el grupo de cierre está abierto o cerrado.
        /// </summary>
        public bool IsGroupCollapsed
        {
            get { return foldGroupStatus; }
            set
            {
                foldGroupStatus = value;

                if (foldGroupStatus)
                    imgFoldingCaret.Source = new BitmapImage(new Uri(@"/LiteDBManager;component/Resources/icons8-forward-24.png", UriKind.Relative));
                else
                    imgFoldingCaret.Source = new BitmapImage(new Uri(@"/LiteDBManager;component/Resources/icons8-expand-arrow-24.png", UriKind.Relative));
            }
        }

        /// <summary>
        /// Evento que se produce cuando se pulsa sobre el botón de apertura/cierre
        /// del grupo de cierre.
        /// </summary>
        public event EventHandler<FoldEventArgs> FoldActionRequest;

        /// <summary>
        /// Evento que se produce cuando el usuario presiona el botón
        /// añadir línea sobre el documento en modo edición.
        /// </summary>
        public event EventHandler<LineEventArgs> AddNewLine;

        /// <summary>
        /// Evento que se produce cuando el usuario elimina un documento
        /// anidado. 
        /// </summary>
        public event EventHandler<LineEventArgs> InnerDocumentDelete;

        public event EventHandler<LineEventArgs> UpdatedLineType;

        /// <summary>
        /// Define el tipo de línea usado
        /// </summary>
        public LineType LineType { get; private set; }

        /// <summary>
        /// Define el tipo de datos usado para la representación de los datos
        /// </summary>
        public BsonType ValueType { get; private set; }

        /// <summary>
        /// Valor temporal que contiene los cambios realizados por el usuario
        /// en el nombre del objeto.
        /// </summary>
        public string EditingKey { get; private set; }

        /// <summary>
        /// Valor temporal que contiene los cambios realizados por el usuario
        /// en el valor del objeto.
        /// </summary>
        public BsonValue EditingValue { get; private set; }

        /// <summary>
        /// Indica si la linea actual es una línea perteneciente a un objeto
        /// anidado.
        /// </summary>
        public bool IsNestedDocumentChildren { get; set; } = false;

        #endregion

        #region Constructores
        /// <summary>
        /// Constructor dedicado para las líneas que tengan valores exclusivamente
        /// y que sean hijos de un documento.
        /// </summary>
        /// <param name="line">Par de valores con la clave conteniendo el nombre del objecto y
        /// como valor el BsonValue que define el contenido del objeto BSON.</param>
        public DocumentViewerLine(KeyValuePair<string, BsonValue> line, int lineNumber) : this(line, lineNumber, DEFAULT_TAB_LENGTH)
        { }

        /// <summary>
        /// Constructor dedicado para las líneas que tengan valores exclusivamente
        /// y que sean hijos de un documento.
        /// </summary>
        /// <param name="line">Par de valores con la clave conteniendo el nombre del objecto y
        /// como valor el BsonValue que define el contenido del objeto BSON.</param>
        /// <param name="tabLength">Longitud del tabulado para esta linea.</param>
        public DocumentViewerLine(KeyValuePair<string, BsonValue> line, int lineNumber, int tabLength) : this(line.Key, line.Value, lineNumber, tabLength)
        { }

        /// <summary>
        /// Constructor dedicado para crear líneas de apertura y cierre del documento
        /// principal. También puede ser usada para el cierre de subdocumentos.
        /// </summary>
        /// <param name="lineType">Definición del tipo de línea</param>
        public DocumentViewerLine(LineType lineType, int lineNumber) : this(lineType, lineNumber, MAIN_DOCUMENT_BRACKETS_TAB_LENGTH)
        { }

        /// <summary>
        /// Constructor dedicado para crear líneas de apertura y cierre del documento
        /// principal. También puede ser usada para el cierre de subdocumentos.
        /// </summary>
        /// <param name="lineType">Definición del tipo de línea</param>
        /// <param name="tabLength">Longitud del tabulado para esta linea.</param>
        public DocumentViewerLine(LineType lineType, int lineNumber, int tabLength)
        {
            InitializeComponent();

            LineType = lineType;
            LineTabLength = tabLength;
            LineNumber = lineNumber;

            if (lineType == LineType.Opening)
                tbxVariable.Text = "{";
            else if (lineType == LineType.Closing || lineType == LineType.NestedObjectClosing)
                tbxVariable.Text = "}";

            tbkDoubleDot.Visibility = Visibility.Hidden;
            tbxValue.Visibility = Visibility.Hidden;
            IsEndingOrStartingLine = true;
            tbxVariable.Margin = new Thickness(tabLength, 0, 0, 0);
            cbxObjectType.SelectedItem = BsonType.String;
            txtObjectType.Text = cbxObjectType.SelectedItem.ToString();

            HideValueTypeSelection();
            HideQuotationMarks();

            tbxValue.IsEnabled = false;
            tbxVariable.IsEnabled = false;
            IsVariableEditable = false;
            IsValueEditable = false;
        }

        /// <summary>
        /// Constructor dedicado para crear líneas de apertura y cierre de documentos
        /// hijo del documento principal o de otros subdocumentos.
        /// </summary>
        /// <param name="line">Par de valores con la clave conteniendo el nombre del objecto y
        /// como valor el BsonValue que define el contenido del objeto BSON.</param>
        /// <param name="lineType">Definición del tipo de línea</param>
        public DocumentViewerLine(KeyValuePair<string, BsonValue> line, int lineNumber, LineType lineType) : this(line, lineNumber, lineType, DEFAULT_TAB_LENGTH)
        { }

        /// <summary>
        /// Constructor dedicado para crear líneas de apertura y cierre de documentos
        /// hijo del documento principal o de otros subdocumentos.
        /// </summary>
        /// <param name="line">Par de valores con la clave conteniendo el nombre del objecto y
        /// como valor el BsonValue que define el contenido del objeto BSON.</param>
        /// <param name="lineType">Definición del tipo de línea</param>
        /// <param name="tabLength">Longitud del tabulado para esta linea.</param>
        public DocumentViewerLine(KeyValuePair<string, BsonValue> line, int lineNumber, LineType lineType, int tabLength)
        {
            InitializeComponent();

            RawLine = line;
            LineKey = line.Key;
            LineValue = line.Value;
            LineType = lineType;
            LineTabLength = tabLength;
            LineNumber = lineNumber;

            if (lineType == LineType.NestedObjectOpening)
            {
                tbxVariable.Text = line.Key;
                tbxValue.Text = "{";
                tbkDoubleDot.Visibility = Visibility.Visible;
                tbxValue.Visibility = Visibility.Visible;
                IsVariableEditable = true;
                IsValueEditable = false;
                btnFolding.Visibility = Visibility.Visible;
                
            }
            else if (lineType == LineType.NestedObjectClosing)
            {
                tbxVariable.Text = "}";
                tbkDoubleDot.Visibility = Visibility.Hidden;
                tbxValue.Visibility = Visibility.Hidden;
                IsVariableEditable = false;
                IsValueEditable = false;
            }

            SetLineValueType();
            IsEndingOrStartingLine = true;
            tbxVariable.Margin = new Thickness(tabLength, 0, 0, 0);
            HideValueTypeSelection();
        }

        public DocumentViewerLine(string key, BsonValue value, int lineNumber, int tabLength)
        {
            InitializeComponent();

            LineKey = key;
            LineValue = value;
            LineTabLength = tabLength;
            LineType = LineType.DataDisplay;
            LineNumber = lineNumber;

            tbxVariable.Margin = new Thickness(tabLength, 0, 0, 0);
            tbxVariable.Text = LineKey;
            tbxValue.Text = GetLineValue();

            if (tbxValue.Text.Length <= 0)
                tbxValue.Background = Brushes.LightPink;

            if (tbxVariable.Text.Length <= 0)
                tbxVariable.Background = Brushes.LightPink;

            SetLineValueType();
            ShowValueTypeSelection();
            HideValueTypeSelection();

            //Desactivar la edición si se trata del identificador de documento
            if (key.Equals("_id"))
            {
                tbxValue.IsReadOnly = true;
                tbxVariable.IsReadOnly = true;
                IsVariableEditable = false;
                IsValueEditable = false;
            }
            else
            {
                tbxValue.IsReadOnly = false;
                tbxVariable.IsReadOnly = false;
                IsVariableEditable = true;
                IsValueEditable = true;
            }
        }

        #endregion

        #region Eventos de comportamiento del editor de variable

        // Gestiona los eventos de teclado en el control.
        // Para la tecla Enter, actualiza el valor y libera el foco.
        // Para la tecla Esc, cancela la edición y libera el foco.
        private void tbxVariable_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsVariableEditable)
            {
                if (e.Key == Key.Enter)
                {
                    EditingKey = tbxVariable.Text;
                    IsFocusLiberated = true;
                    Keyboard.ClearFocus();

                    if (tbxVariable.Text.Length > 0)
                        tbxVariable.Background = Brushes.Transparent;
                    else
                        tbxVariable.Background = Brushes.LightPink;
                }
                else if (e.Key == Key.Escape)
                {
                    tbxVariable.Text = EditingKey ?? LineKey;
                    IsFocusLiberated = true;
                    Keyboard.ClearFocus();

                    if (tbxVariable.Text.Length > 0)
                        tbxVariable.Background = Brushes.Transparent;
                    else
                        tbxVariable.Background = Brushes.LightPink;
                }
            }
        }

        // Permite cambiar el comportamiento del control al capturar el evento doble clic
        // sobre el control. Si el control no tiene el foco, se le asigna. Si el control ya
        // tiene el foco, entonces el control se comporta de modo normal para un evento
        // doble clic.
        private void tbxVariable_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsInEditionMode)
            {
                if (e.ChangedButton == MouseButton.Left && IsVariableEditable)
                {
                    if (!tbxVariable.IsFocused)
                    {
                        tbxVariable.Focus();
                    }
                }
                else
                {
                    if (IsVariableEditable)
                    { 
                        e.Handled = true;
                    }
                }
            }
        }

        // Previene que el evento clic sobre el textbox desencadene la toma del foco por parte
        // de este control. Si el control ya tiene el foco, entonces no bloquea la propagación
        // del evento al control.
        private void tbxVariable_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsInEditionMode)
                if ((!tbxVariable.IsFocused && e.ChangedButton == MouseButton.Left) || !IsVariableEditable)
                    e.Handled = true;
        }

        //Actualiza el valor del registro cuando el evento no se ha producido en relación
        //con un evento gestionado.
        private void tbxVariable_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsVariableEditable && !IsFocusLiberated)
            {
                EditingKey = tbxVariable.Text;
            }

            if (tbxVariable.Text.Length > 0)
                tbxVariable.Background = Brushes.Transparent;
            else
                tbxVariable.Background = Brushes.LightPink;

            IsFocusLiberated = false;
        }

        /// <summary>
        /// Cambia el cursor del ratón cuando pasa sobre el control según el estado
        /// de edición.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxVariable_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsInEditionMode || tbxVariable.IsReadOnly)
                tbxVariable.Cursor = Cursors.Arrow;
            else
                tbxVariable.Cursor = Cursors.IBeam;
        }

        #endregion

        #region Eventos del comportamiento del editor de valor

        private void tbxValue_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsInEditionMode)
                if ((!tbxValue.IsFocused && e.ChangedButton == MouseButton.Left) || !IsValueEditable)
                    e.Handled = true;
        }

        private void tbxValue_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsInEditionMode)
            {
                if (e.ChangedButton == MouseButton.Left && IsValueEditable)
                {
                    if (!tbxValue.IsFocused)
                    {
                        tbxValue.Focus();
                    }
                }
                else
                {
                    if (IsVariableEditable)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void tbxValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsValueEditable && !IsFocusLiberated)
            {
                EditingValue = UpdateLineValue();
            }

            IsFocusLiberated = false;
        }

        private void tbxValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsValueEditable)
            {
                if (e.Key == Key.Enter)
                {
                    EditingValue = UpdateLineValue();
                    IsFocusLiberated = true;
                    Keyboard.ClearFocus();
                }
                else if (e.Key == Key.Escape)
                {
                    tbxValue.Text = GetLineValue(EditingValue ?? LineValue);
                    IsFocusLiberated = true;
                    Keyboard.ClearFocus();
                }
            }
        }

        private void tbxValue_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsInEditionMode || tbxValue.IsReadOnly)
                tbxValue.Cursor = Cursors.Arrow;
            else
                tbxValue.Cursor = Cursors.IBeam;
        }

        private BsonValue UpdateLineValue()
        {
            switch (ValueType)
            {
                case BsonType.Array:
                    return ParseValueAsArray();
                case BsonType.Binary:
                    return ParseValueAsBinary();
                case BsonType.Boolean:
                    return ParseValueAsBoolean();
                case BsonType.DateTime:
                    return ParseValueAsDateTime();
                case BsonType.Decimal:
                    return ParseValueAsDecimal();
                case BsonType.Document:
                    return ParseValueAsDocument();
                case BsonType.Double:
                    return ParseValueAsDouble();
                case BsonType.Guid:
                    return ParseValueAsGuid();
                case BsonType.Int32:
                    return ParseValueAsInt32();
                case BsonType.Int64:
                    return ParseValueAsInt64();
                case BsonType.MaxValue:
                    return ParseValueAsMaxValue();
                case BsonType.MinValue:
                    return ParseValueAsMinValue();
                case BsonType.Null:
                    return ParseValueAsNull();
                case BsonType.ObjectId:
                    return ParseValueAsObjectId();
                case BsonType.String:
                    return ParseValueAsString();
                default:
                    throw new Exception("Data type unsuported");
            }
        }

        private BsonValue ParseValueAsArray()
        {
            return new BsonValue();
        }

        private BsonValue ParseValueAsBinary()
        {
            byte b1 = byte.Parse("1");
            byte[] value = new byte[1];
            value[0] = b1;
            var newValue = new BsonValue(value);
            tbxValue.Text = newValue.ToString();

            return newValue;
        }

        private BsonValue ParseValueAsBoolean()
        {
            bool value;

            if (bool.TryParse(tbxValue.Text, out value))
            {
                tbxValue.Background = Brushes.Transparent;
                return new BsonValue(value);
            }
            else
                tbxValue.Background = Brushes.LightPink;

            return new BsonValue();
        }

        private BsonValue ParseValueAsDateTime()
        {
            DateTime parsedValue;

            if (DateTime.TryParse(tbxValue.Text, out parsedValue))
            {
                tbxValue.Background = Brushes.Transparent;
                return new BsonValue(parsedValue);
            }
            else
                tbxValue.Background = Brushes.LightPink;

            return new BsonValue();
        }

        private BsonValue ParseValueAsDecimal()
        {
            decimal value;
            CultureInfo culture = DbConnections.CurrentConnection.GetCultureInfoFromDB();
            var stringValue = ParseStringDecimalValueToCulture(culture, tbxValue.Text);

            if (decimal.TryParse(stringValue, NumberStyles.Number, culture, out value))
            {
                tbxValue.Background = Brushes.Transparent;
                return new BsonValue(value);
            }
            else
                tbxValue.Background = Brushes.LightPink;

            return new BsonValue();
        }

        private BsonValue ParseValueAsDocument()
        {
            return new BsonValue();
        }

        private BsonValue ParseValueAsDouble()
        {
            double value;
            CultureInfo culture = DbConnections.CurrentConnection.GetCultureInfoFromDB();
            var stringValue = ParseStringDecimalValueToCulture(culture, tbxValue.Text);

            if (double.TryParse(stringValue, NumberStyles.Number, culture, out value))
            {
                tbxValue.Background = Brushes.Transparent;
                return new BsonValue(value);
            }
            else
                tbxValue.Background = Brushes.LightPink;

            return new BsonValue();
        }

        private BsonValue ParseValueAsGuid()
        {
            Guid value;

            if(Guid.TryParse(tbxValue.Text, out value))
            {
                tbxValue.Background = Brushes.Transparent;
                return new BsonValue(value);
            }
            else
                tbxValue.Background = Brushes.LightPink;

            return new BsonValue();
        }

        private BsonValue ParseValueAsInt32()
        {
            int value;

            if (int.TryParse(tbxValue.Text, out value))
            {
                tbxValue.Background = Brushes.Transparent;
                return new BsonValue(value);
            }
            else
                tbxValue.Background = Brushes.LightPink;

            return new BsonValue();
        }

        private BsonValue ParseValueAsInt64()
        {
            long value;

            if (long.TryParse(tbxValue.Text, out value))
            {
                tbxValue.Background = Brushes.Transparent;
                return new BsonValue(value);
            }
            else
                tbxValue.Background = Brushes.LightPink;

            return new BsonValue();
        }

        private BsonValue ParseValueAsMaxValue()
        {
            var newValue = BsonValue.MaxValue;
            tbxValue.Text = newValue.ToString();

            return newValue;
        }

        private BsonValue ParseValueAsMinValue()
        {
            var newValue = BsonValue.MinValue;
            tbxValue.Text = newValue.ToString();

            return newValue;
        }

        private BsonValue ParseValueAsNull()
        {
            if (tbxValue.Text.ToLower().Trim().Equals("null"))
            {
                tbxValue.Background = Brushes.Transparent;
                return new BsonValue();
            }
            else
                tbxValue.Background = Brushes.LightPink;

            return new BsonValue();
        }

        private BsonValue ParseValueAsObjectId()
        {
            ObjectId value;
            BsonValue newValue;

            if (string.IsNullOrWhiteSpace(tbxValue.Text))
            {
                value = ObjectId.NewObjectId();
                newValue = new BsonValue(value);
                tbxValue.Text = newValue.ToString();
                tbxValue.Background = Brushes.Transparent;
            }
            else
            {
                try 
                {
                    value = new ObjectId(tbxValue.Text);
                    newValue = new BsonValue(value);
                    tbxValue.Background = Brushes.Transparent;
                }
                catch(Exception ex)
                {
                    tbxValue.Background = Brushes.LightPink;
                    newValue = new BsonValue();
                }
            }

            return newValue;
        }

        private BsonValue ParseValueAsString()
        {
            var value = tbxValue.Text;
            var newValue = new StringBuilder();

            //Parsear la cadena para eliminar las comillas
            //Solo se conservarán las comillas escapadas
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '"')
                {
                    if (i == 0 || (i > 0 && value[i - 1] != '\\'))
                    {
                        continue;
                    }
                }

                newValue.Append(value[i]);
            }

            tbxValue.Background = Brushes.Transparent;
            return new BsonValue(newValue.ToString());
        }

        #endregion

        #region Eventos de los componentes de selección del tipo de datos

        private void cbxObjectType_DropDownClosed(object sender, EventArgs e)
        {
            editObjectType.Visibility = Visibility.Hidden;
            showObjectType.Visibility = Visibility.Visible;

            EditingValue = UpdateLineValue();
        }

        private void cbxObjectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxObjectType.SelectedItem != null && txtObjectType != null)
            {
                txtObjectType.Text = cbxObjectType.SelectedItem.ToString();
                ValueType = (BsonType)cbxObjectType.SelectedItem;
                editObjectType.Visibility = Visibility.Hidden;
                showObjectType.Visibility = Visibility.Visible;

                if (ValueType == BsonType.String)
                {
                    ShowQuotationMarks();
                }
                else if(ValueType == BsonType.Document)
                {
                    LineEventArgs args = new LineEventArgs();
                    args.LineNumber = LineNumber;
                    args.NewLineBsonType = ValueType;

                    UpdatedLineType?.Invoke(this, args);
                }
                else
                {
                    HideQuotationMarks();
                }
            }
        }

        private void cbxObjectType_MouseMove(object sender, MouseEventArgs e)
        {
            if (cbxObjectType.Visibility == Visibility.Visible)
            {
                cbxObjectType.IsDropDownOpen = true;
            }
        }

        private void txtObjectType_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showObjectType.Visibility = Visibility.Hidden;
            editObjectType.Visibility = Visibility.Visible;
            e.Handled = true;
            //LineClicked?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Eventos de línea (añadir, eliminar, folding, mouse over...)

        private void controlGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (IsInEditionMode)
            {
                Visibility btnAdd = Visibility.Visible;
                Visibility btnDel = Visibility.Visible;

                if(LineType != LineType.DeletedLine)
                {
                    if(LineType == LineType.Opening)
                    {
                        btnDel = Visibility.Hidden;
                    }
                    else if(LineType == LineType.NestedObjectClosing)
                    {
                        btnDel = Visibility.Hidden;
                    }
                    else if(LineType == LineType.Closing)
                    {
                        btnDel = Visibility.Hidden;
                        btnAdd = Visibility.Hidden;
                    }
                }
                else
                {
                    if (typeBeforeDelete == LineType.Opening)
                    {
                        btnDel = Visibility.Hidden;
                        btnAdd = Visibility.Hidden;
                    }
                    else if (typeBeforeDelete == LineType.NestedObjectClosing)
                    {
                        btnDel = Visibility.Hidden;
                    }
                    else if (typeBeforeDelete == LineType.Closing)
                    {
                        btnDel = Visibility.Hidden;
                        btnAdd = Visibility.Hidden;
                    }
                    else if(typeBeforeDelete == LineType.NestedObjectOpening)
                    {
                        btnAdd = Visibility.Hidden;
                    }
                    else if(IsNestedDocumentChildren)
                    {
                        btnDel = Visibility.Hidden;
                        btnAdd = Visibility.Hidden;
                    }
                }

                btnAddLine.Visibility = btnAdd;
                btnDeleteLine.Visibility = btnDel;

                stpEditButtons.Visibility = Visibility.Visible;
            }
        }

        private void controlGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            controlBorder.BorderThickness = new Thickness(0, 0, 0, 0);
            stpEditButtons.Visibility = Visibility.Collapsed;
        }

        private void btnAddLine_MouseMove(object sender, MouseEventArgs e)
        {
            controlBorder.BorderBrush = Brushes.CornflowerBlue;
            controlBorder.BorderThickness = new Thickness(0, 0, 0, 2);
        }

        private void btnAddLine_MouseLeave(object sender, MouseEventArgs e)
        {
            controlBorder.BorderThickness = new Thickness(0, 0, 0, 0);
        }

        private void btnDeleteLine_MouseMove(object sender, MouseEventArgs e)
        {
            if ((LineType == LineType.DataDisplay || LineType == LineType.NestedObjectOpening) && IsInEditionMode)
                controlGrid.Background = new SolidColorBrush(Color.FromArgb(25, 255, 0, 0));
        }

        private void btnDeleteLine_MouseLeave(object sender, MouseEventArgs e)
        {
            if (LineType != LineType.DeletedLine)
                controlGrid.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
        }

        //Boton del agrupado de líneas
        private void btnFolding_Click(object sender, RoutedEventArgs e)
        {
            var args = new FoldEventArgs();

            FoldActionRequest?.Invoke(this, args);
        }

        private void btnAddLine_Click(object sender, RoutedEventArgs e)
        {
            var args = new LineEventArgs();
            args.LineNumber = LineNumber;

            AddNewLine?.Invoke(this, args);
        }

        private void btnDeleteLine_Click(object sender, RoutedEventArgs e)
        {
            if (LineType == LineType.NestedObjectOpening || (LineType == LineType.DeletedLine && typeBeforeDelete == LineType.NestedObjectOpening))
            {
                var args = new LineEventArgs();
                args.LineNumber = LineNumber;
                args.OriginalLineType = LineType;

                InnerDocumentDelete?.Invoke(this, args);
            }
            else
            {
                if (!LineKey.Equals("_id"))
                    SetLineAsDeleted();
            }
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        /// Cancela la última modificación realizada y vuelve
        /// al valor original. Si se han realizado sucesivas 
        /// modificaciones, siempre se vuelve al valor original.
        /// </summary>
        public void CancelEdition()
        {
            if (IsVariableEditable)
                tbxVariable.Text = LineKey;

            if (IsValueEditable)
                tbxValue.Text = GetLineValue();

            if (LineType == LineType.DeletedLine)
            {
                LineType = typeBeforeDelete;
                controlGrid.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            }

            editionMode = false;
            HideValueTypeSelection();
        }

        /// <summary>
        /// Valida los cambios actuales y actualiza el valor de la línea
        /// al valor de la última modificación. Esta operación es necesaria
        /// antes de actualizar la BD.
        /// </summary>
        public void CommitEdition()
        {
            if (IsVariableEditable)
                LineKey = EditingKey ?? LineKey;

            if (IsValueEditable)
                LineValue = EditingValue ?? LineValue;
        }

        public void SetAsDeleted()
        {
            SetLineAsDeleted();
        }

        //Activa o desactiva el modo edición
        public void ActivateEdition()
        {
            editionMode = true;

            if (IsVariableEditable || IsValueEditable)
            {
                tbkLineNumber.Visibility = Visibility.Visible;
                showObjectType.Visibility = Visibility.Visible;
                editObjectType.Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                tbkLineNumber.Visibility = Visibility.Visible;
                return;
            }
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Devuelve el valor de la línea actual aplicándole los formatos
        /// necesarios para adaptar el valor al formato de la cultura 
        /// de la base de datos.
        /// </summary>
        /// <returns>Una cadena formateada según la cultura de la base de datos.</returns>
        private string GetLineValue()
        {
            return GetLineValue(LineValue);
        }

        /// <summary>
        /// Devuelve el valor de la línea actual aplicándole los formatos
        /// necesarios para adaptar el valor al formato de la cultura 
        /// de la base de datos.
        /// </summary>
        /// <param name="value">Valor que se desea obtener formateado</param>
        /// <returns>Una cadena formateada según la cultura de la base de datos</returns>
        private string GetLineValue(BsonValue value)
        {
            CultureInfo culture = DbConnections.CurrentConnection.GetCultureInfoFromDB();

            if (LineValue.Type == BsonType.Double)
                return value.AsDouble.ToString(culture);

            else if (LineValue.Type == BsonType.Decimal)
                return value.AsDecimal.ToString(culture);

            else if (LineValue.Type == BsonType.Document)
                return "";

            else if (LineValue.Type == BsonType.Int64)
                return value.AsInt64.ToString();

            return value.ToString();
        }

        /// <summary>
        /// Analiza el texto dado en busca de los signos de puntuación decimales y los 
        /// compara con los signos de puntuación usados en la cultura de la base de datos.
        /// Si el signo usado difiere del signo declarado en la cultura de la BD es sutituido
        /// por el de la base de datos. Esto permite al usuario usar libremente el signo de 
        /// puntuación que prefiera y mantiene la consistencia de los datos cuando se realiza
        /// la conversión de tipo de datos. De este modo se consigue separar la puntuación 
        /// mostrada en el editor de aquella que debe ser usada en la BD.
        /// </summary>
        /// <param name="culture">Cultura de la BD</param>
        /// <param name="text">Texto a verificar</param>
        /// <returns>Texto adaptado a la cultura pasada</returns>
        private string ParseStringDecimalValueToCulture(CultureInfo culture, string text)
        {
            if (text.Contains(","))
            {
                if (!culture.NumberFormat.NumberDecimalSeparator.Equals(","))
                    return text.Replace(",", culture.NumberFormat.NumberDecimalSeparator);

                return text;
            }
            else if (text.Contains("."))
            {
                if (!culture.NumberFormat.NumberDecimalSeparator.Equals("."))
                    return text.Replace(".", culture.NumberFormat.NumberDecimalSeparator);

                return text;
            }

            return text;
        }

        private void SetLineValueType()
        {
            if (LineValue != null)
            {
                ValueType = LineValue.Type;
            }
            else if (LineValue == null && LineType == LineType.DataDisplay)
            {
                ValueType = BsonType.String;
            }
            else
                return;

            cbxObjectType.SelectedItem = ValueType;
            txtObjectType.Text = ValueType.ToString();
        }

        private void HideValueTypeSelection()
        {
            editObjectType.Visibility = Visibility.Hidden;
            showObjectType.Visibility = Visibility.Hidden;
        }

        private void ShowValueTypeSelection()
        {
            editObjectType.Visibility = Visibility.Hidden;
            showObjectType.Visibility = Visibility.Visible;
        }

        //Método auxiliar para mostrar los signos gráficos comillas dobles
        //para representar un tipo de datos cadena
        private void ShowQuotationMarks()
        {
            var value = tbxValue.Text;

            if (value.Length > 0)
            {
                if (value[0] != '"')
                    value = string.Format("{0}{1}", '"', value);

                //Final de cadena sin comillas
                if (value.Length == 1 || (value.Length > 1 && value[value.Length - 1] != '"'))
                    value = string.Format("{0}{1}", value, '"');

                //Final de cadena con comilla, verificación que esté escapada
                else if (value.Length > 1 && value[value.Length - 1] == '"' && value[value.Length - 2] == '\\')
                    value = string.Format("{0}{1}", value, '"');
            }
            else
                value = "\"\"";

            tbxValue.Text = value;
        }

        //Método auxiliar para ocultar los signos gráficos comillas dobles
        //para representar un tipo de datos cadena
        private void HideQuotationMarks()
        {
            var newValue = new StringBuilder();
            var value = tbxValue.Text;

            for (int i = 0; i < value.Length; i++)
            {
                if ((i == 0 && value[i] == '"') || (i > 0 && value[i] == '"' && value[i - 1] != '\\'))
                    continue;

                newValue.Append(value[i]);
            }

            tbxValue.Text = newValue.ToString();
        }

        private void SetLineAsDeleted()
        {
            if (LineType != LineType.DeletedLine)
            {
                typeBeforeDelete = LineType;
                LineType = LineType.DeletedLine;
                controlGrid.Background = new SolidColorBrush(Color.FromArgb(40, 255, 0, 0));
            }
            else if(LineType == LineType.DeletedLine)
            {
                LineType = typeBeforeDelete;
                controlGrid.Background = Brushes.White;
            }
        }

        #endregion

        #region Variables privadas
        //Permite saber si el foco ha sido liberado debido a un evento gestionado
        private bool IsFocusLiberated = false;
        private bool editionMode = false;
        private bool foldGroupStatus = false;
        private int lineNumber = 0;
        private LineType typeBeforeDelete;
        #endregion
    }
}
