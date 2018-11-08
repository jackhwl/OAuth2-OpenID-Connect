using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public interface IChangeRepository
    {
        void StartBusinessTransaction(ChangeRequest entity);
        void ContinueBusinessTransaction(ChangeRequest entity);
        ChangeRequest ContinueBusinessTransaction(int id);
        void EndBusinessTransaction();

        IEnumerable<ChangeRequest> GetOpenChangeRequests();

        IEnumerable<ChangeRequest> SearchChangeRequests(ChangeRequestSearchCriteria criteria);

        ChangeRequest GetChangeRequestbyId(int crId);
        ChangeRequest GetChangeRequestByVersionId(int id);

        bool DeleteChangeRequest(int id);

        ChangeRequest CreateNewChangeRequest();

        void CreateChangeRequest(ChangeRequest cr);

        void UpdateChangeRequest(ChangeRequest cr);

        ChangeRequestTask GetChangeRequestTaskbyId(int taskId, string modified);

        ChangeRequestTask CreateNewChangeRequestTask(int changeRequestId);

        int CreateChangeRequestTask(ChangeRequestTask task);

        void UpdateChangeRequestTask(ChangeRequestTask task);

        bool DeleteChangeRequestTask(ChangeRequestTask task);
    }
}
