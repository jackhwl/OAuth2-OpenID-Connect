using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public interface IChangeRepository
    {
        IEnumerable<ChangeRequest> GetOpenChangeRequests();
        //IEnumerable<ChangeRequest> GetOpenChangeRequestsForUser(System.Security.Claims.ClaimsPrincipal user);

        IEnumerable<ChangeRequest> SearchChangeRequests(ChangeRequestSearchCriteria criteria);

        IEnumerable<ChangeRequest>  ChangeRequests { get; }

        Task<ChangeRequest> GetChangeRequestbyId(int CRId);
        Task<bool> DeleteChangeRequest(int Id);

        ChangeRequest CreateNewChangeRequest();

        void CreateChangeRequest(ChangeRequest cr, string currentUser);

        void UpdateChangeRequest(ChangeRequest cr, string currentUser);

        Task<ChangeRequestTask> GetChangeRequestTaskbyId(int taskId);

        int CreateChangeRequestTask(ChangeRequestTask task, string currentUser);

        void UpdateChangeRequestTask(ChangeRequestTask task, string currentUser);
        Task<bool> DeleteChangeRequestTask(int Id);
    }
}
