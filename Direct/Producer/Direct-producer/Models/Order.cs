using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct_producer.Models
{
    public class Order
    {
        public long Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public long Amount { get; set; }

        public Order(long id, long amount)
        {
           Id = id;
            Amount = amount;
            CreateDate = LastUpdated = DateTime.UtcNow;
        }

        public void UpdateOrder(long amount)
        {
            Amount = amount;
            LastUpdated = DateTime.UtcNow;
        }
    }
}
