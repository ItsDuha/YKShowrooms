namespace DatabaseModel.Models
{
    /// <summary>
    /// Represents how a visitor heard about the showroom.
    /// Add this as a string column on the Visitor entity (see migration).
    /// </summary>
    public static class MarketingSourceOptions
    {
        public const string Instagram = "Instagram";
        public const string TikTok    = "TikTok";
        public const string Website   = "Website";
        public const string Friend    = "Friend";
        public const string WalkIn    = "Walk-in";
        public const string Other     = "Other";

        public static readonly string[] All =
        [
            Instagram, TikTok, Website, Friend, WalkIn, Other
        ];
    }
}
