using LiteDBManager.UIElements;
using LiteDB;
using LiteDB.Engine;
using System.Collections.Generic;
using System.Globalization;
using LiteDBManager.UIElements.Pages;

namespace LiteDBManager.Structures
{
    public class DbConnection
    {
        public DbConnection(ConnectionString connString)
        {
            ConnectionData = connString;
            liteDatabase = new LiteDatabase(connString);
        }

        /// <summary>
        /// Define los parámetros utilizados para conectarse a una base de datos
        /// </summary>
        public ConnectionString ConnectionData { get; private set; }

        /// <summary>
        /// Instancia del motor de LiteDB para el ConnectionString actual
        /// </summary>
        public LiteDatabase LiteDatabase 
        { 
            get
            {
                lock(objectLock)
                {
                    return liteDatabase;
                }
            }
            set
            {
                lock(objectLock)
                {
                    liteDatabase = value;
                }
            }
        }

        /// <summary>
        /// Página usada para el despliege de la lista de colecciones para la base de datos
        /// </summary>
        public CollectionsManagementPage CollectionManagementPage 
        { 
            get
            {
                if(collectionsManagement == null)
                {
                    collectionsManagement = new CollectionsManagementPage();
                }

                return collectionsManagement;
            }
        }

        /// <summary>
        /// Página usada para la gestión de la base de datos basada en comandos
        /// SQL.
        /// </summary>
        public CommandManagementPage CommandManagementPage
        {
            get
            {
                if(commandManagement == null)
                {
                    commandManagement = new CommandManagementPage();
                }

                return commandManagement;
            }
        }

        /// <summary>
        /// Diccionario que contiene una lista de páginas para las colecciones abiertas.
        /// El diccionario se estructura como colección -> página.
        /// </summary>
        public Dictionary<string, DocumentManagementPage> DocumentManagementPages { get; } = new Dictionary<string, DocumentManagementPage>();

        /// <summary>
        /// Indica la colección que está siendo editada actualmente
        /// </summary>
        public string EditingCollection { get; set; }

        /// <summary>
        /// Devuelve la información de idioma de la base de datos
        /// </summary>
        /// <returns></returns>
        public CultureInfo GetCultureInfoFromDB()
        {
            var collation = LiteDatabase.Pragma(Pragmas.COLLATION);
            var split = ((string)collation.RawValue).Split('/');
            CultureInfo culture = CultureInfo.InvariantCulture;

            if (split.Length == 2)
            {
                culture = new CultureInfo(split[0], false);
            }

            return culture;
        }

        private CollectionsManagementPage collectionsManagement;
        private CommandManagementPage commandManagement;
        private LiteDatabase liteDatabase;
        private object objectLock = new object();
    }
}
