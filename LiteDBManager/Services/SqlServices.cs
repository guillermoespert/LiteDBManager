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
        /// <returns>true si correcto o false en caso contrario.</returns>
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

        /// <summary>
        /// Elimina un documento dado de la colección activa
        /// </summary>
        /// <param name="bsonDoc">Documento a eliminar</param>
        /// <returns>true si correcto o false en caso contrario.</returns>
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

        /// <summary>
        /// Crea una nueva colección con el nombre especificado en la
        /// base de datos activa.
        /// </summary>
        /// <param name="collectionName">Nombre de la colección</param>
        /// <returns>true si correcto o false en caso contrario.</returns>
        public static bool CreateNewColection(string collectionName)
        {
            string create = string.Format("INSERT INTO {0}:INT VALUES {{}}", collectionName); //Crea una colección con un registro
            string delete = string.Format("DELETE {0}", collectionName); //->Vacíar la colección creada con INSERT
            string commit = "COMMIT"; //->Valida los cambios y los vuelve permanentes

            var ldb = DbConnections.CurrentConnection.LiteDatabase;

            try
            {
                ldb.Execute(create);
                ldb.Execute(delete);
                ldb.Execute(commit);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Se ha producido un error: \n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        /// <summary>
        /// Elimina la colección especificada de la base de datos activa.
        /// </summary>
        /// <param name="collectionName">Colección a eliminar</param>
        /// <returns>true si tuvo éxito o false en caso contrario.</returns>
        public static bool DeleteCollection(string collectionName)
        {
            try
            {
                var db = DbConnections.CurrentConnection.LiteDatabase;
                db.Execute("DROP COLLECTION " + collectionName);
                db.Execute("COMMIT");

                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Se ha producido un error: \n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        /// <summary>
        /// Renombra una colección
        /// </summary>
        /// <param name="collectionName">Nombre actual de la colección</param>
        /// <param name="newName">Nuevo nombre de la colección</param>
        /// <returns>Devuelve true en caso de éxito o false en caso contrario.</returns>
        public static bool RenameCollection(string collectionName, string newName)
        {
            try
            {
                var db = DbConnections.CurrentConnection.LiteDatabase;

                return db.RenameCollection(collectionName, newName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Se ha producido un error: \n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        /// <summary>
        /// Vacía una colección existente
        /// </summary>
        /// <param name="collectionName">Nombre de la colección a vaciar.</param>
        /// <returns>Devuelve true en caso de éxito o false en caso contrario.</returns>
        public static bool EmptyCollection(string collectionName)
        {
            string delete = string.Format("DELETE {0}", collectionName); //->Vacíar la colección
            string commit = "COMMIT"; //->Valida los cambios y los vuelve permanentes

            var ldb = DbConnections.CurrentConnection.LiteDatabase;

            try
            {
                ldb.Execute(delete);
                ldb.Execute(commit);

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
