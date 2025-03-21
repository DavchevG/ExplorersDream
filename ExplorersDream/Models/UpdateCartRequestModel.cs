namespace ExplorersDream.Models
{
    public class UpdateCartRequestModel
    {
        public int ProductId { get; set; }
        public string? Size { get; set; }
        public int Quantity { get; set; }
    }
}
