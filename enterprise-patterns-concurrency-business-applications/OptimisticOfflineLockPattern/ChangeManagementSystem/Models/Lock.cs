using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public class Lock
    {
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public string OwnerId { get; set; }
        public DateTime AcquiredDateTime { get; set; }
    }
}
