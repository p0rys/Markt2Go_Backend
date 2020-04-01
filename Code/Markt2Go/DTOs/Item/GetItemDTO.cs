namespace Markt2Go.DTOs.Item
{
    public class GetItemDTO
    {
        public long Id { get; set; }
        public long ReservationId { get; set; }
        public string Name { get; set; }
        public string ArticleId { get; set; }
        public float Amount { get; set; }
        public string Unit { get; set; }
    }
}