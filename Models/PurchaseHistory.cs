using System;

namespace MyShop.Models
{
    public class PurchaseHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public string CategoryKey { get; set; } = null!;

        public DateTime PurchasedAt { get; set; } = DateTime.Now;
    }
}
