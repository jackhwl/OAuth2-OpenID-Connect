using ChangeManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.ViewModels
{
    public class ChangeRequestViewModel
    {
        public ChangeRequest ChangeRequest { get; set; }
        public int TaskId { get; set; }
        public DateTime TaskModifiedTime { get; set; }
    }
}
