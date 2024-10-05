namespace Game_API.Dtos
{
    public class GameDetails
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string HeaderImage { get; set; } = string.Empty;
        public int SteamAppID { get; set; }
        public PriceOverview? PriceOverview { get; set; }
        public List<string>? Genres { get; set; }
        public List<string>? Developers { get; set; }
        public List<string>? Publishers { get; set; }
        public string? ReleaseDate { get; set; }
        public bool IsFree { get; set; }
        public string? Platform { get; set; }
        public string? Categories { get; set; }
        public ReviewSummary? ReviewSummary { get; set; }
    }
}
