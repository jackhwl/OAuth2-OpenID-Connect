using ChangeManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.ViewModels
{
    public class ChangeRequestSearchViewModel
    {
        public ChangeRequestSearchCriteria Criteria { get; set; }
        public IEnumerable<ChangeRequest> ChangeRequests { get; set; }
    }
}
