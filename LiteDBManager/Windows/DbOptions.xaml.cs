using LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace LiteDBManager.Windows
{
    /// <summary>
    /// Lógica de interacción para DbOptions.xaml
    /// </summary>
    public partial class DbOptions : Window
    {
        private ConnectionString conn = new ConnectionString();

        public ConnectionString DbConnection 
        { 
            get
            {
                return conn;
            }

            set
            {
                conn = value;
                UpdateUi();
            }
        } 
        public DbOptionEditModes EditionMode { get; private set; }
        public bool IsInCreateMode{ get { return EditionMode == DbOptionEditModes.Create; } }

        //TODO : Aviso al usuario de que no se soporta la definición de un tamaño inicial en una BD encriptada

        public DbOptions(DbOptionEditModes edmode)
        {
            EditionMode = edmode;
            InitializeComponent();

            cbxCulture.ItemsSource = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Select(x => x.LCID)
                .Distinct()
                .Where(x => x != 4096)
                .Select(x => CultureInfo.GetCultureInfo(x).Name)
                .ToList();

            var sortOptions = new List<string>();
            sortOptions.Add("");
            sortOptions.AddRange(Enum.GetNames(typeof(CompareOptions)).Cast<string>());

            cbxSort.ItemsSource = sortOptions;
        }

        public new bool? ShowDialog()
        {
            DataContext = DbConnection;
            UpdateUi();
            return base.ShowDialog();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void UpdateUi()
        {
            if(conn.Connection == ConnectionType.Direct)
                rdbDirectConnection.IsChecked = true;
            else
                rdbSharecConnection.IsChecked = true;
        }

        private void rdbDirectConnection_Checked(object sender, RoutedEventArgs e)
        {
            conn.Connection = ConnectionType.Direct;
        }

        private void rdbSharecConnection_Checked(object sender, RoutedEventArgs e)
        {
            conn.Connection = ConnectionType.Shared;
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (EditionMode == DbOptionEditModes.Create)
            {
                var validationResult = VerifyPasswordsCreate();

                if (validationResult && !string.IsNullOrWhiteSpace(txtPassword.Password))
                    DbConnection.Password = txtPassword.Password;
                else if (validationResult && string.IsNullOrWhiteSpace(txtPassword.Password))
                    DbConnection.Password = null;
                else
                    return;
            }

            if (EditionMode == DbOptionEditModes.Connect)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                    DbConnection.Password = null;
                else
                    DbConnection.Password = txtPassword.Password;
            }

            if(cbxCulture.SelectedIndex > 0)
            {
                var collation = cbxCulture.SelectedItem.ToString();

                if (cbxSort.SelectedIndex > 0)
                {
                    collation += "/" + cbxSort.SelectedItem.ToString();
                }

                DbConnection.Collation = new Collation(collation);
            }

            long definedSize = 0L;
            long initialSize = 0L;

            if(long.TryParse(txtInitialSize.Text, out definedSize))
            {
                if (definedSize > 0)
                {
                    //LiteDB acepta la definición de un tamaño inicial de archivo.
                    //El valor debe ser un long múltiplo de 8192. Este valor se guarda
                    //en las propiedades de aplicación para permitir que sea modificado
                    //a través de la ventana de configuración.

                    //A continuación se realiza una primera conversión del tamaño deseado, 
                    //aplicando un factor de multiplicación según sea la unidad elegida.
                    var rawFactor = Math.Pow(1024, (double) cbxSizeMultiplier.SelectedIndex + 1);
                    long rawSize = definedSize * (long)rawFactor;

                    //A continuación se ajusta el tamaño deseado al tamaño más cercano que sea
                    //múltiplo de 8192
                    long adaptedFactor = rawSize / Properties.Settings.Default.PageSize;

                    initialSize = Properties.Settings.Default.PageSize * adaptedFactor;
                }
            }

            DbConnection.InitialSize = initialSize;

            DialogResult = true;
        }

        private bool VerifyPasswordsCreate()
        {
            if ((string.IsNullOrWhiteSpace(txtPassword.Password) && !string.IsNullOrWhiteSpace(txtRePassword.Password)) ||
                (!string.IsNullOrWhiteSpace(txtPassword.Password) && string.IsNullOrWhiteSpace(txtRePassword.Password)))
            {
                MessageBox.Show(this, "Para establecer una contraseña para la base de datos se deben escribir la contraseña deseada y a continuación la verificación.", 
                    "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if ((!string.IsNullOrWhiteSpace(txtPassword.Password) && !string.IsNullOrWhiteSpace(txtRePassword.Password)) &&
                !txtPassword.Password.Equals(txtRePassword.Password))
            {
                MessageBox.Show(this, "Para establecer una contraseña para la base de datos la contraseña y la verificación deben coincidir.",
                    "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }
    }
}
