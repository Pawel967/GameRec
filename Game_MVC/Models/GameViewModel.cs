namespace Game_MVC.Models
{
    public class GameViewModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ShortDescription { get; set; }
        public string HeaderImage { get; set; }
        public int SteamAppID { get; set; }
        public PriceOverview? PriceOverview { get; set; }
        public List<string>? Genres { get; set; }
        public List<string>? Developers { get; set; }
        public List<string>? Publishers { get; set; }
        public string? ReleaseDate { get; set; }
        public bool IsFree { get; set; }
        public double? UserRating { get; set; }
        public string? Platform { get; set; }
        public string? Categories { get; set; }
    }
}
