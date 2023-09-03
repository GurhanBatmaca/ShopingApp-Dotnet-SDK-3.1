using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using shopapp.entity;

namespace shopapp.webui.Models
{
    public class ProductModel
    {
        public ProductModel()
        {
            this.SelectedCategories = new List<Category>(); 
        }
        public int ProductId { get; set; }
        public string Name { get; set; }

        [Required(ErrorMessage ="Url zorunlu bir alan.")]
        public string Url { get; set; }

        public double? Price { get; set; }

        [Required(ErrorMessage ="Description zorunlu bir alan.")]
        [StringLength(100,MinimumLength=5,ErrorMessage="Description 5-100 karater aralığında olmadır.")]
        public string Description { get; set; }

        [Required(ErrorMessage ="ImageUrl zorunlu bir alan.")]
        public string ImageUrl { get; set; }
        public bool IsApporoved { get; set; }
        public bool IsHome { get; set; }
        public List<Category> SelectedCategories { get; set; }
    }
}