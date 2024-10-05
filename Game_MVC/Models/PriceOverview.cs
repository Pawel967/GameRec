namespace Game_MVC.Models
{
    public class PriceOverview
    {
        public string Currency { get; set; }
        public int Initial { get; set; }
        public int Final { get; set; }
        public int DiscountPercent { get; set; }
    }
}
