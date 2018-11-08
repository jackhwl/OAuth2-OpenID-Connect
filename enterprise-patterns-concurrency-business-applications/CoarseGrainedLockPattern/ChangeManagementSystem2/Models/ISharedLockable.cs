using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    interface ISharedLockable
    {
        int SharedVersionId { get; set; }
        Version SharedVersion { get; set; }
    }
}
