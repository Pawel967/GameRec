namespace Game_API.Dtos
{
    public class PriceOverview
    {
        public string Currency { get; set; } = string.Empty;
        public int Initial { get; set; }
        public int Final { get; set; }
        public int DiscountPercent { get; set; }
    }
}
