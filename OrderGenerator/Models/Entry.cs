using System;

namespace OrderGenerator.Models
{
    public class Entry
    {
        public Entry(EntryType type, Order order)
        {
            Merchant = order.Merchant;
            Type = type;
            if (type == EntryType.Invoice)
            {
                Id = order.OrderId;
            }
            else
            {
                Id = Guid.NewGuid().ToString();
                ReferenceId = order.OrderId;
            }
            Amount = order.Amount;

            if (type == EntryType.Refund || type == EntryType.Cashing)
            {
                Amount = Amount * -1.0m;
            }
            // hack
            Date = Program.GetDate();
        }
        public int Merchant { get; set; }
        public EntryType Type { get; set; }
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public decimal Amount { get; set; }

        public DateTime Date { get; set; }
    }
}