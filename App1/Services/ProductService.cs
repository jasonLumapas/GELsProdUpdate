using App1.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.Services
{
    public class ProductService
    {
        private readonly string _connectionString;

        public ProductService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Product> GetProducts()
        {
            var products = new List<Product>();

            using var cn = new MySqlConnection(_connectionString);
            cn.Open();

            using var cmd = new MySqlCommand("GetActiveProductPrices", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                products.Add(new Product
                {
                    Sku = reader[0].ToString(),
                    Name = reader[1].ToString(),
                    Pieces = Convert.ToInt32(reader[2]),
                    WithdrawalPrice = Convert.ToDecimal(reader[3]),
                    RetailPrice = Convert.ToDecimal(reader[4]),
                    SupplierId = reader[5].ToString(),
                    SupplierName = reader[6].ToString()
                });
            }

            return products;
        }

        public List<Supplier> GetSuppliers()
        {
            var suppliers = new List<Supplier>();

            using var cn = new MySqlConnection(_connectionString);
            cn.Open();

            using var cmd = new MySqlCommand("GetSuppliers", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                suppliers.Add(new Supplier
                {
                    SupplierId = reader[0].ToString(),
                    Name = reader[1].ToString()
                                    });
            }

            return suppliers;
        }

        public int UpdateProduct(Product product, decimal withdrawal, decimal retail)
        {
            using var cn = new MySqlConnection(_connectionString);
            cn.Open();

            using var cmd = new MySqlCommand("update_product_price", cn)
            {
                CommandType = CommandType.StoredProcedure
            };
            string todayString = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.Parameters.AddWithValue("@p_sku_id", product.Sku);
            cmd.Parameters.AddWithValue("@p_start_date", todayString);
            cmd.Parameters.AddWithValue("@p_widrawal_price", withdrawal);
            cmd.Parameters.AddWithValue("@p_retail_price", retail);

            return cmd.ExecuteNonQuery();
        }

        public int AddNewProduct(
            string productName, 
            string supplierName, 
            string? supplierId, 
            decimal withdrawal, 
            decimal retail,
            int pieces)
        {
            string todayString = DateTime.Today.ToString("yyyy-MM-dd");

            using var cn = new MySqlConnection(_connectionString);
            cn.Open();

            using var cmd = new MySqlCommand("AddProductAndPriceAutoSKU", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@p_name", productName);
            cmd.Parameters.AddWithValue("@p_pieces", pieces);
            cmd.Parameters.AddWithValue("@p_supplier_id", supplierId);
            cmd.Parameters.AddWithValue("@p_is_active", 1);
            cmd.Parameters.AddWithValue("@pp_start_date", todayString);
            cmd.Parameters.AddWithValue("@pp_is_active", 1);
            cmd.Parameters.AddWithValue("@pp_widrawal_price", withdrawal);
            cmd.Parameters.AddWithValue("@pp_retail_price", retail);
            cmd.Parameters.AddWithValue("@new_supplier_name", supplierName);

            return cmd.ExecuteNonQuery();
        }
    }
}
