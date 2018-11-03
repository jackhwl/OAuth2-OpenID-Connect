using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public class ChangeRequestSearchCriteria
    {
        public ChangeRequestSearchCriteria()
        {
            //TargetDateStart = (DateTime?)null;
            //TargetDateEnd = (DateTime?)null;
        }
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime? TargetDateStart { get; set; }
        [DataType(DataType.Date)]
        public DateTime? TargetDateEnd { get; set; }
        public ChangeRequest.StatusEnum Status { get; set; }

    }
}
