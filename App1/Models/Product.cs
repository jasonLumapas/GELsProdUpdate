using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.Models
{
    public class Product
    {
        public string Sku { get; set; }              // reader[0]
        public string Name { get; set; }             // reader[1]
        public int Pieces { get; set; }              // reader[2]
        public decimal WithdrawalPrice { get; set; } // reader[3]
        public decimal RetailPrice { get; set; }     // reader[4]
        public string SupplierId { get; set; }              // reader[5]
        public string SupplierName { get; set; }              // reader[6]

        // This makes the AutoSuggestBox display the Name
        public override string ToString() => Name;
    }
}
