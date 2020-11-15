using LiteDBManager.Structures;
using LiteDBManager.UIElements;
using LiteDB;
using System.Windows;
using System.Windows.Media;

namespace LiteDBManager.Services
{
    /// <summary>
    /// Esta clase representa una interfaz de comunicación o façade con la clase de la
    /// ventana principal.
    /// Provee de métodos que realizan operaciones sobre la interfaz de usuario y sobre
    /// estructuras de datos auxiliares.
    /// </summary>
    public class MainService
    {
        public static MainWindow MainWindow { get { return Application.Current.MainWindow as MainWindow; } }

        /// <summary>
        /// Realiza todas las operaciones para crear una nueva conexión con una base de datos.
        /// Actualiza todos los elementos de la IU así como cualquier estructura de datos.
        /// Este método necesita un ConnectionString para añadir la nueva conexión.
        /// No importa si la operación es de apertura de archivo existente o bien de
        /// la creación de una nueva base de datos.
        /// Las conexiones creadas con este método se insertan en la lista de conexiones.
        /// </summary>
        /// <param name="connString">Objeto de metadatos de conexión</param>
        public static void AddNewConnection(ConnectionString connString)
        {
            var conn = new DbConnection(connString);
            DbConnections.Connections.Add(conn);

            AddNewConnection(conn);
        }

        /// <summary>
        /// Realiza todas las operaciones para crear una nueva conexión con una base de datos.
        /// Actualiza todos los elementos de la IU así como cualquier estructura de datos.
        /// Este método necesita un ConnectionString para añadir la nueva conexión.
        /// No importa si la operación es de apertura de archivo existente o bien de
        /// la creación de una nueva base de datos.
        /// </summary>
        /// <param name="dbConnection">Conexión a base de datos</param>
        public static void AddNewConnection(DbConnection dbConnection)
        {
            var main = Application.Current.MainWindow as MainWindow;

            DbConnections.CurrentConnection = dbConnection;

            DbConnectionControl dbcon = new DbConnectionControl(dbConnection);

            main.stpConnections.Children.Add(dbcon);
            main.expConnections.IsExpanded = true;
        }

        /// <summary>
        /// Actualiza el control de lista de conexiones presente en la
        /// ventana principal. Refleja los cambios realizados en las 
        /// colecciones de una conexión.
        /// </summary>
        public static void UpdateCollections()
        {
            var main = Application.Current.MainWindow as MainWindow;

            main?.stpCollections.Children.Clear();
            
            foreach(var collection in DbConnections.CurrentConnection.LiteDatabase.GetCollectionNames())
            {
                DbCollectionControl dbcol = new DbCollectionControl(collection);

                main.stpCollections.Children.Add(dbcol);

                main.expCollections.IsExpanded = true;
            }
        }

        /// <summary>
        /// Pone todas la conexiones con fondo blanco.
        /// Un fondo verde significa conexión activa. Sin embargo,
        /// este fondo diferenciado solo tiene una utilidad visual
        /// para marcar la conexión activa en ese momento.
        /// </summary>
        public static void UnselectAllConnections() //-> Visual
        {
            var main = Application.Current.MainWindow as MainWindow;

            foreach(var child in main.stpConnections.Children )
            {
                var connection = child as DbConnectionControl;
                
                if(connection != null)
                {
                    connection.ConnectionOpened = false;
                    connection.Background = Brushes.White;
                }
            }
        }

        /// <summary>
        /// Elimina una conexión de la lista de conexiones y reestablece la
        /// interfaz recargando una nueva conexión existente o limpia
        /// completamente la interfaz si no hay más conexiones activas.
        /// </summary>
        /// <param name="dbconnection">Conexión que debe ser eliminada</param>
        public static void RemoveConnection(DbConnection dbconnection)
        {
            var main = Application.Current.MainWindow as MainWindow;

            DbConnections.Connections.Remove(dbconnection);
            DbConnections.CurrentConnection = null;
            main.stpConnections.Children.Clear();
            UnselectAllConnections();
            
            foreach(var connection in DbConnections.Connections)
            {
                AddNewConnection(connection);
            }

            if (DbConnections.CurrentConnection != null)
                UpdateCollections();
            else
            {
                main.stpCollections.Children.Clear();
                main.frmDbManager.Content = null;
            }
        }
    }
}
