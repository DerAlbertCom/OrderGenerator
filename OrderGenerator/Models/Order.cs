using System;
using System.Diagnostics;

namespace OrderGenerator.Models
{
    public class Order
    {
        public Order()
        {
            Address=new Address();
            Person = new Person();
        }
        public string OrderId { get; set; }
        public DateTime Date { get; set; }
        public int Merchant { get; set; }
        public Address Address { get; set; }
        public Person Person { get; set; }
        public string Phone { get; set; }
        public string EMail { get; set; }

        public decimal Amount { get; set; }
    }
}