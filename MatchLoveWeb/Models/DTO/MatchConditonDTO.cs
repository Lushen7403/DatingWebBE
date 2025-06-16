namespace MatchLoveWeb.Models.DTO
{
    public class MatchConditionDTO
    {
        public int? AccountId { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public double? MaxDistanceKm { get; set; }
        public int? GenderId { get; set; }
    }
}
