using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public class ChangeRequestTask
    {
        public enum StatusEnum
        {
            [Display(Name = "Draft")] draft,
            [Display(Name = "In Progress")] inProgress,
            [Display(Name = "Completed")] completed
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CompletedDate { get; set; }

        public StatusEnum Status { get; set; }
   
        public int ChangeRequestID { get; set; }
        public ChangeRequest ChangeRequest { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime Modified { get; set; }

    }
}
