using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public class Version : EntityBase
    {
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
