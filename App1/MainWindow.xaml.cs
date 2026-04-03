using App1.Models;
using App1.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private bool didSelectItem = false;
        private bool didSelectSupplier = false;
        private readonly ProductService _service;
        private List<Product> _products = new();
        private List<Supplier> _suppliers = new();
        private Product _selectedProduct;
        private Supplier _selectedSupplier;

        public MainWindow()
        {
            InitializeComponent();
            _service = new ProductService(
                "Server=localhost;Database=gel_schema;User Id=root;Password=G@gaG3ls;"
            );
            LoadProducts();
            LoadSuppliers();
        }

        private void LoadProducts()
        {
            _products = _service.GetProducts();
            ProductsAutoSuggestBox.ItemsSource = _products;
        }

        private void LoadSuppliers()
        {
            _suppliers = _service.GetSuppliers();
            SuppliersAutoSuggestBox.ItemsSource = _suppliers;
        }

        private void ProductsAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                return;
            didSelectItem = false;
            PiecesPerBoxTextBox.IsEnabled = true;
            SuppliersAutoSuggestBox.IsEnabled = true;
            _selectedSupplier = null;

            sender.ItemsSource = _products
                .Where(p => p.Name.Contains(sender.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();

            ClearInputs();
        }

        private void ProductsAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            _selectedProduct = (Product)args.SelectedItem;
            didSelectItem = true;
            PiecesPerBoxTextBox.IsEnabled = false;
            SuppliersAutoSuggestBox.IsEnabled = false;
            sender.Text = _selectedProduct.Name;

            WidrawalTextBox.Text = _selectedProduct.WithdrawalPrice.ToString();
            RetailTextBox.Text = _selectedProduct.RetailPrice.ToString();
            PiecesPerBoxTextBox.Text = _selectedProduct.Pieces.ToString();

            var supplier = _suppliers.FirstOrDefault(s => s.SupplierId == _selectedProduct.SupplierId);

            if (supplier != null)
            {
                SuppliersAutoSuggestBox.Text = supplier.Name;

                // optional: keep reference if you need it later
                SuppliersAutoSuggestBox.Tag = supplier;
            }
        }

        private void ProductsAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            //string value = sender.Text;
            // Use the value
        }

        private void SuppliersAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                return;

            didSelectSupplier = false;

            sender.ItemsSource = _suppliers
                .Where(p => p.Name.Contains(sender.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();

        }

        private void SuppliersAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            didSelectSupplier = true;
            _selectedSupplier = (Supplier)args.SelectedItem;
        }

        private void SuppliersAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(WidrawalTextBox.Text, out var withdrawal) ||
                !decimal.TryParse(RetailTextBox.Text, out var retail))
            {
                ShowMessageUpdated(false);
                return;
            }

            if (didSelectItem)
            {
                _service.UpdateProduct(_selectedProduct, withdrawal, retail);

                ShowMessageUpdated(true);
                LoadProducts();
            }
            else
            {
                if (!decimal.TryParse(PiecesPerBoxTextBox.Text, out var pieces))
                {
                    return;
                }

                if (didSelectSupplier) // existing supplier
                {
                    _service.AddNewProduct(
                        ProductsAutoSuggestBox.Text, 
                        _selectedSupplier.Name, 
                        _selectedSupplier.SupplierId, 
                        decimal.Parse(WidrawalTextBox.Text), 
                        decimal.Parse(RetailTextBox.Text),
                        int.Parse(PiecesPerBoxTextBox.Text));
                }
                else  // new supplier; add supplier
                {
                    if (string.IsNullOrWhiteSpace(SuppliersAutoSuggestBox.Text) ||
                        string.IsNullOrWhiteSpace(ProductsAutoSuggestBox.Text))
                    {
                        ShowMessageUpdated(false);
                        return;
                    }
                        _service.AddNewProduct(
                        ProductsAutoSuggestBox.Text,
                        SuppliersAutoSuggestBox.Text,
                        null,
                        decimal.Parse(WidrawalTextBox.Text),
                        decimal.Parse(RetailTextBox.Text),
                        int.Parse(PiecesPerBoxTextBox.Text));
                    LoadSuppliers();
                }
                LoadProducts();
               
                ShowMessageUpdated(true);
            }
        }

        private void ClearInputs()
        {
            WidrawalTextBox.Text = "";
            RetailTextBox.Text = "";
            PiecesPerBoxTextBox.Text = "";
            _selectedProduct = null;
        }

        private async void ShowMessageUpdated(bool ok)
        {
            string message = "Changes applied";
            if (!ok) {
                message = "Please input product and supplier";
            }
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Product Update",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
