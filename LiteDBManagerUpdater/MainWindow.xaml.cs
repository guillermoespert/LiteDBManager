using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.IO.Compression;
using System.Timers;

namespace LiteDBManagerUpdater
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string UpdateFilePath { get; set; }
        public string MainFilePath { get; set; }

        private Timer closeTimer = new Timer(1500);

        public MainWindow()
        {
            InitializeComponent();
        }

        public void DoUpdate()
        {
            AddStatusLine("Iniciando actualización...", Brushes.Black, FontWeights.Bold);

            AddStatusLine("Verificando archivo descargado...");

            //Comprobar el archivo de descarga
            if (!File.Exists(UpdateFilePath))
            {
                AddStatusLine("Error: el archivo de actualización no existe. No se puede continuar con la actualización.", Brushes.Red, FontWeights.Bold);

                return;
            }

            AddStatusLine("Comprobando que LiteDBManager esté cerrado...");

            //Comprobar procesos
            Process[] processes = Process.GetProcessesByName("LiteDBManager");

            if(processes.Length > 0)
            {
                //Hay procesos activos de la aplicación principal.
            }

            AddStatusLine("Extrayendo archivos...");

            string extractPath = Path.Combine(Path.GetDirectoryName(UpdateFilePath), Path.GetFileNameWithoutExtension(UpdateFilePath));

            try
            {
                ZipFile.ExtractToDirectory(UpdateFilePath, extractPath);
            }
            catch
            {
                AddStatusLine("Se produjo un error durante la extracción de los archivos.", Brushes.Red, FontWeights.Bold);

                AddStatusLine("La actualización ha fallado.", Brushes.Red, FontWeights.Bold);

                return;
            }

            AddStatusLine("Eliminando archivos...");

            foreach (var file in Directory.GetFiles(MainFilePath))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            AddStatusLine("Copiando archivos...");

            //Copiar archivos de la actualización
            if (!FolderCopy(extractPath, MainFilePath)) return;

            AddStatusLine("Actualización finalizada correctamente.", Brushes.Green, FontWeights.Bold);
            closeTimer.AutoReset = false;
            closeTimer.Elapsed += CloseTimer_Elapsed;
            closeTimer.Start();
        }

        private void CloseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            closeTimer.Stop();

            Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }

        private bool FolderCopy(string sourceDirectory, string destDirectory)
        {
            var line = new TextBlock();

            //Información de la carpeta origen
            var infoDir = new DirectoryInfo(sourceDirectory);
            var subdirs = infoDir.GetDirectories();

            //Crear la carpeta de destino si no existe
            Directory.CreateDirectory(destDirectory);

            //Obtener la lista de archivos dentro de la carpeta origen
            var files = infoDir.GetFiles();

            //Copiar archivos
            foreach (var file in files)
            {
                string fileName;
                string pdbFile = "updLiteDBManagerUpdater.pdb";

                if (file.Name.Equals("LiteDBManagerUpdater.pdb"))
                    fileName = pdbFile;
                else
                    fileName = file.Name;

                try
                {
                    file.CopyTo(Path.Combine(destDirectory, fileName));
                }
                catch
                {
                    if (!Path.GetExtension(file.Name).Equals(".pdb"))
                    {
                        AddStatusLine("Se produjo un error durante la copia de los archivos.", Brushes.Red, FontWeights.Bold);

                        AddStatusLine("La actualización ha fallado.", Brushes.Red, FontWeights.Bold);

                        return false;
                    }
                }
            }

            //Copiar subdirectorios
            foreach (var subdir in subdirs)
            {
                if (!FolderCopy(subdir.FullName, Path.Combine(destDirectory, subdir.Name))) return false;
            }

            return true;
        }

        public void AddStatusLine(string text) => AddStatusLine(text, Brushes.Black);

        public void AddStatusLine(string text, Brush color) => AddStatusLine(text, color, FontWeights.Normal);

        public void AddStatusLine(string text, Brush color, FontWeight fontWeight)
        {
            var line = new TextBlock();
            line.Text = text;
            line.FontSize = 14d;
            line.Foreground = color;
            line.FontWeight = fontWeight;
            stpUpdateInfo.Children.Add(line);
        }
    }
}
