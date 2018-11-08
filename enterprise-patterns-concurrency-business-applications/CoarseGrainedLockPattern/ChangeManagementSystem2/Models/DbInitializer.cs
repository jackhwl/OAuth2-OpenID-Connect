using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public class DbInitializer
    {
        public static void Initialize(AppDataContext context)
        {
            context.Database.EnsureCreated();

            if (context.ChangeRequests.Any())
            {
                return;   // DB has been seeded
            }

            var cr1 = new ChangeRequest
            {
                Name = "Open FW to External Service",
                Summary = "Open a Firewall",
                TargetDate = DateTime.Parse("2018-01-05"),
                ActualDate = null,
                Owner = "system@changemanagement.com",
                Modified = DateTime.Parse("2017-12-15"),
                ModifiedBy = "admin@changemanagement.com",
                Priority = ChangeRequest.PriorityEnum.medium,
                Status = ChangeRequest.StatusEnum.inProgress,
                Urgency = ChangeRequest.UrgencyEnum.medium,
            };


            var crt1 = new ChangeRequestTask
            {
                ChangeRequest = cr1,
                Name = "Modify Firewall Rules",
                Summary = "Opened port 443 on firewall to 193.169.1.1",
                Status = ChangeRequestTask.StatusEnum.inProgress,
                CompletedDate = null,
                ModifiedBy = "admin@changemanagement.com",
                Modified = DateTime.Parse("2018-01-14"),

            };

            var cr2 = new ChangeRequest{
                Name ="Create New VM for COTS",
                Summary ="Create a VM for ABC Solution",
                TargetDate =DateTime.Parse("2018-01-02"),
                ActualDate =null,
                Owner="admin@changemanagement.com",
                Modified =DateTime.Parse("2017-01-06"),
                ModifiedBy ="system@changemanagement.com",
                Priority =ChangeRequest.PriorityEnum.low,
                Status =ChangeRequest.StatusEnum.inProgress,
                Urgency =ChangeRequest.UrgencyEnum.medium
            };

            var crt2 = new ChangeRequestTask
            {
                ChangeRequest = cr2,
                Name = "Provision new VM",
                Summary = "Windows 2016 VM created",
                Status = ChangeRequestTask.StatusEnum.completed,
                CompletedDate = null,
                ModifiedBy = "admin@changemanagement.com",
                Modified = DateTime.Parse("2018-01-05"),
            };
            var crt3 = new ChangeRequestTask
            {
                ChangeRequest = cr2,
                Name = "Join VM to domain",
                Summary = "VM joined to AD",
                Status = ChangeRequestTask.StatusEnum.inProgress,
                CompletedDate = null,
                ModifiedBy = "admin@changemanagement.com",
                Modified = DateTime.Parse("2018-01-05"),
            };
            var crt4 = new ChangeRequestTask
            {
                ChangeRequest = cr2,
                Name = "Install ABC middleware",
                Summary = "COTS installed as per build book",
                Status = ChangeRequestTask.StatusEnum.inProgress,
                CompletedDate = null,
                ModifiedBy = "admin@changemanagement.com",
                Modified = DateTime.Parse("2018-01-05"),
            };

            context.ChangeRequests.Add(cr1);
            context.ChangeRequests.Add(cr2);
            context.ChangeRequestTasks.Add(crt1);
            context.ChangeRequestTasks.Add(crt2);
            context.ChangeRequestTasks.Add(crt3);
            context.ChangeRequestTasks.Add(crt4);

            context.SaveChanges();
        }
    }
}
