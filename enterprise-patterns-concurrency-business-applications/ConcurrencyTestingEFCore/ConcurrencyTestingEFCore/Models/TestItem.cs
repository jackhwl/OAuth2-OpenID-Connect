using System;

namespace ConcurrencyTestingEFCore.Models
{
    public class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }

    }
}
