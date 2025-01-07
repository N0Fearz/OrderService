namespace OrderService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string Customer { get; set; }
        public int[] ArticleIds {get; set;}
    }
}
