using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public abstract class EntityBase : IEntityBase
    {
        public int ID { get; set; }
        public DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }

        [NotMapped]
        public TrackedEntityState State { get; set; }
    }
}
