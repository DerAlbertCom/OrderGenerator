using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OrderGenerator.Models;
using CsvHelper;
using GenFu;
using Generate = GenFu.GenFu;

namespace OrderGenerator
{
    class Program
    {
        private static int _zipCodeCount = 10;
        private static int _mailCount = 10;
        private static int _addressCount = 10;
        private static int _personCount = 10;
        private static int _phoneCount= 10;
        private static int _orderCount = 10000;
        private const double DatasetsPerHour = 750;
        private static readonly Random Rnd = new Random();

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                if (!int.TryParse(args[0], out _orderCount))
                {
                    Console.Error.Write("Muss ein Int sein");
                    return;
                }
            }
            SetCounts();
            ConfigureGenerator();
            using (var orderWriter = CreateCsvWriter<Order>("Orders.csv"))
            {
                WriteOrders(NewOrders(0.7), orderWriter, "Entries1.csv");
                WriteOrders(NewOrders(0.1), orderWriter, "Entries2.csv");
                WriteOrders(NewOrders(0.1), orderWriter, "Entries3.csv");
                WriteOrders(NewOrders(0.1), orderWriter, "Entries4.csv");
            }
        }

        private static void SetCounts()
        {
            _personCount = Math.Min((int)(_orderCount * 0.015), 50000);
            _addressCount = Math.Min((int)(_orderCount * 0.015), 50000);
            _mailCount = Math.Min((int)(_orderCount * 0.02), 100000);
            _phoneCount = Math.Min((int)(_orderCount * 0.02), 100000);
            _zipCodeCount = Math.Min((int)(_orderCount * 0.01), 3000);
            Console.WriteLine($"Orders: {_orderCount}, People: {_personCount}, Address: {_addressCount}, Mails: {_mailCount}, ZipCode: {_zipCodeCount}, Phone: {_phoneCount}");
        }


        private static void WriteOrders(IEnumerable<Order> orders, ICsvWriter orderWriter, string fileName)
        {
            using (var entryWriter = CreateCsvWriter<Entry>(fileName))
            {
                foreach (var order in orders)
                {
                    WriteCounter();
                    orderWriter.WriteRecord(order);
                    WriteEntries(order, entryWriter);
                }
            }
        }

        private static double _counter;
        private static int _oldPercent = -1;
        private static void WriteCounter()
        {
            _counter++;
            var percent = (int)(100 * (_counter / _orderCount));
            if (percent == _oldPercent) return;
            _oldPercent = percent;
            Console.WriteLine($"{percent}% Anzahl {_counter}");
        }

        private static IEnumerable<Order> NewOrders(double percent)
        {
            for (int i = 0; i < (int)(_orderCount * percent); i++)
            {
                yield return Generate.New<Order>();
            }
        }
        private static ICsvWriter CreateCsvWriter<T>(string filename)
        {
            Console.WriteLine($"Creating {filename}");
            var entryFile = File.CreateText(filename);
            var entryWriter = new CsvWriter(entryFile);
            entryWriter.Configuration.QuoteAllFields = true;
            entryWriter.Configuration.CultureInfo = CultureInfo.InvariantCulture;
            entryWriter.WriteHeader<T>();
            return entryWriter;
        }

        private static void WriteEntries(Order order, ICsvWriter entryWriter)
        {
            entryWriter.WriteRecord(new Entry(EntryType.Invoice, order));
            if (Should(25))
            {
                entryWriter.WriteRecord(new Entry(EntryType.Refund, order));
            }
            else if (Should(25))
            {
                entryWriter.WriteRecord(new Entry(EntryType.Reminder, order));
                if (Should(25))
                {
                    entryWriter.WriteRecord(new Entry(EntryType.Cashing, order));
                }
            }
        }

        private static void ConfigureGenerator()
        {
            Generate.Configure<Address>()
                .Fill(a => a.ZipCode).WithRandom(GenerateZipCodes())
                .Fill(a => a.Street).AsAddress();
            Generate.Configure<Order>()
                .Fill(a => a.OrderId, () => Guid.NewGuid().ToString())
                .Fill(a => a.Merchant).WithinRange(300, 310)
                .Fill(a => a.Amount).WithinRange(50, 9999)
                .Fill(a => a.EMail).WithRandom(GetMailAddresses())
                .Fill(a => a.Phone).WithRandom(GetPhoneNumbers())
                .Fill(a => a.Address).WithRandom(Generate.ListOf<Address>(_addressCount))
                .Fill(a => a.Person).WithRandom(Generate.ListOf<Person>(_personCount))
                .Fill(a => a.Date, GetDate);
        }

        private static double _offsetSeconds;

        internal static DateTime GetDate()
        {
            _offsetSeconds += (60.0 * 60.0) / DatasetsPerHour;
            var dtDateTime = new DateTime(2013, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddSeconds(_offsetSeconds).ToLocalTime();
        }

        private static string[] GenerateZipCodes()
        {
            var zipCodes = new string[_zipCodeCount];
            for (var i = 0; i < zipCodes.Length; i++)
            {
                zipCodes[i] = $"{Rnd.Next(0, 99999):00000}";
            }
            return zipCodes;
        }

        private static bool Should(int threshold)
        {
            return Rnd.Next(0, 100) <= threshold;
        }

        private class PrecreateClass
        {
            public string EMail { get; set; }
            public string Phone { get; set; }
        }

        private static string[] GetMailAddresses()
        {
            var data = new string[_mailCount];
            for (var i = 0; i < data.Length; i++)
            {
                var mail = Generate.New<PrecreateClass>();
                data[i] = mail.EMail;
            }
            return data;
        }
        private static string[] GetPhoneNumbers()
        {
            var data = new string[_phoneCount];
            for (var i = 0; i < data.Length; i++)
            {
                var mail = Generate.New<PrecreateClass>();
                data[i] = mail.Phone;
            }
            return data;
        }
    }
}