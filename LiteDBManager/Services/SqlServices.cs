using LiteDB;
using LiteDBManager.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LiteDBManager.Services
{
    public class SqlServices
    {
        /// <summary>
        /// Añade un nuevo documento vacío a la colección actual
        /// </summary>
        /// <param name="idType"></param>
        /// <returns></returns>
        public static bool AddEmptyDocument(DbIdTypes idType)
        {
            try
            {
                var query = string.Format("INSERT INTO {0}:{1} VALUES {{}}",
                    DbConnections.CurrentConnection.EditingCollection,
                    idType.ToString());

                DbConnections.CurrentConnection.LiteDatabase.Execute(query);

                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Se ha producido un error: \n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        public static bool DeleteDocument(BsonDocument bsonDoc)
        {
            try
            {
                var id = "";

                foreach (var value in bsonDoc)
                {
                    if (value.Key.Equals("_id"))
                    {
                        id = value.Value.ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(id))
                    return false;
                else
                {
                    id = id.Replace("\\", "");
                }

                var query = string.Format("DELETE {0} WHERE _id = {1}",
                    DbConnections.CurrentConnection.EditingCollection,
                    id);

                DbConnections.CurrentConnection.LiteDatabase.Execute(query);
                DbConnections.CurrentConnection.DocumentManagementPages[DbConnections.CurrentConnection.EditingCollection].LoadDocuments();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Se ha producido un error: \n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }
    }
}
