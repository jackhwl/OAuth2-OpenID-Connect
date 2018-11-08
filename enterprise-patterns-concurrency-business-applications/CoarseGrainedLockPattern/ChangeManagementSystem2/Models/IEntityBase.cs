using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public enum TrackedEntityState
    {
        Unchanged,
        Added,
        Modified,
        Deleted
    }

    public interface IEntityBase
    {
        int ID { get; set; }
        DateTime Modified { get; set; }
        string ModifiedBy { get; set; }
        TrackedEntityState State { get; set; }
    }
}
