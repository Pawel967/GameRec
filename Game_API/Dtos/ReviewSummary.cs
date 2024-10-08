﻿namespace Game_API.Dtos
{
    public class ReviewSummary
    {
        public int NumReviews { get; set; }
        public int ReviewScore { get; set; }
        public string ReviewScoreDesc { get; set; } = string.Empty;
        public int TotalPositive { get; set; }
        public int TotalNegative { get; set; }
        public int TotalReviews { get; set; }
    }
}
