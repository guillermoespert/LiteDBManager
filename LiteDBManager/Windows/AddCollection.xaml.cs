﻿using LiteDBManager.Services;
using LiteDBManager.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Lógica de interacción para AddCollection.xaml
    /// </summary>
    public partial class AddCollection : Window
    {
        public AddCollection()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtCollectionName.Text))
            {
                MessageBox.Show(this, "El nombre para la nueva colección no puede estar vacío.",
                    "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!Regex.IsMatch(txtCollectionName.Text, "^[a-zA-Z0-9_]+$"))
            {
                MessageBox.Show(this, "El nombre para la nueva colección solo puede contener letras, cifras y guión bajo.",
                    "Error de validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            DialogResult = SqlServices.CreateNewColection(txtCollectionName.Text);
        }
    }
}
