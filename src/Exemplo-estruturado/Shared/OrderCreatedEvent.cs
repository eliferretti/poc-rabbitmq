namespace Shared
{
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}