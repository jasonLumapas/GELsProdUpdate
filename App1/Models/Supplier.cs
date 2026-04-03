using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.Models
{
    public class Supplier
    {
        public string SupplierId { get; set; }              // reader[0]
        public string Name { get; set; }             // reader[1]
        public override string ToString() => Name;
    }
}
