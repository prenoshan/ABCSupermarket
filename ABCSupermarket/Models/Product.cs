using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ABCSupermarket.Models
{
    public class Product
    {

        //properties to store info about a product

        public int ID { get; set; }
        
        //data annotations to make the fields required
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        //data annotation to set the data type for the price
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Required]
        public decimal Price { get; set; }

        public string ImageURL { get; set; }

    }
}
