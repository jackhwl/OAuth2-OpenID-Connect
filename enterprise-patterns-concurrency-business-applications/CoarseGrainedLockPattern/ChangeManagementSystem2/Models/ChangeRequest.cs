using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public class ChangeRequest
    {
        public enum UrgencyEnum
        {
            [Display(Name="Low")]low,
            [Display(Name = "Medium")] medium,
            [Display(Name = "High")] high
        }

        public enum PriorityEnum
        {
            [Display(Name = "Low")] low,
            [Display(Name = "Medium")] medium,
            [Display(Name = "High")] high
        }

        public enum StatusEnum
        {
            [Display(Name = "Draft")] draft,
            [Display(Name = "In Progress")] inProgress,
            [Display(Name = "Completed")] completed
        }

        public int ID { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime Modified { get; set; }

        [Display(Name = "Title")]
        [StringLength(100)]
        [Required(ErrorMessage = "Please Enter Title")]
        public string Name { get; set; }

        public string Summary { get; set; }

        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ActualDate { get; set; }

        public UrgencyEnum Urgency { get; set; }

        public PriorityEnum Priority { get; set; }

        public StatusEnum Status { get; set; }

        public List<ChangeRequestTask> ChangeRequestTasks { get; set; }
   
        public string Owner { get; set; }

    }
}
