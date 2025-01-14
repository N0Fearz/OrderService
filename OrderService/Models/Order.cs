using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class Order
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "OrderNumber is verplicht.")]
        [MaxLength(50, ErrorMessage = "OrderNumber mag maximaal 50 karakters lang zijn.")]
        public string OrderNumber { get; set; }
        [Required(ErrorMessage = "Customer is verplicht.")]
        [MaxLength(100, ErrorMessage = "Customer mag maximaal 100 karakters lang zijn.")]
        public string Customer { get; set; }
        public int[]? ArticleIds {get; set;}
    }
}
