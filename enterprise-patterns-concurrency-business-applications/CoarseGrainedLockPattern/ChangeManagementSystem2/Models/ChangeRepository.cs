using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Models
{
    public class ChangeRepository : IChangeRepository
    {
        private readonly AppDataContext _dbContext;
        private readonly IHttpContextAccessor _context;
        const string _sessionKeyName = nameof(ChangeRepository);

        private string currentUser;

        private AppDataContext DbContext => _dbContext;
        private IHttpContextAccessor Context => _context;
        private static string SessionKeyName => _sessionKeyName;

        private string CurrentUser { get => currentUser; set => currentUser = value; }

        #region ctor

        /// <summary>
        /// Constructor for ChangeRepository
        /// </summary>
        /// <param name="AppDataContext">Configured through ASP.NET Core Dependency Injection (defined in Startup.ConfigureServices)</param>
        public ChangeRepository(AppDataContext dbContext, 
            IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _context = contextAccessor;
            currentUser = _context.HttpContext.User.Identity.Name;
        }

        #endregion

        #region Session

        public void StartBusinessTransaction(ChangeRequest entity)
        {
            _context.HttpContext.Session.Set<ChangeRequest>(SessionKeyName, entity);
        }

        public void EndBusinessTransaction()
        {
            _context.HttpContext.Session.Set<ChangeRequest>(SessionKeyName, null);
        }

        private void UpdateBusinessEntity(ChangeRequest entity)
        {
            _context.HttpContext.Session.Set<ChangeRequest>(SessionKeyName, entity);
        }

        protected ChangeRequest GetCurrentEntityFromSession()
        {
            return _context.HttpContext.Session.Get<ChangeRequest>(SessionKeyName);
        }

        public void ContinueBusinessTransaction(ChangeRequest entity)
        {
            var changeRequest = ContinueBusinessTransaction(entity.ID);
            UpdateChangeRequestProperties(entity, changeRequest);
            UpdateBusinessEntity(changeRequest);
        }

        public ChangeRequest ContinueBusinessTransaction(int id)
        {
            // get ChangeRequest in session
            ChangeRequest cr = GetCurrentEntityFromSession();

            if (cr != null)
            {
                // verify the id passed from the controller
                if (cr.ID == id)
                {
                    return cr;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region ChangeRequests

        #region Get ChangeRequests

        public IEnumerable<ChangeRequest> GetOpenChangeRequests()
        {
            return DbContext.ChangeRequests
                .Where(c => c.Status == ChangeRequest.StatusEnum.draft || c.Status == ChangeRequest.StatusEnum.inProgress)
                .Include("ChangeRequestTasks")
                .OrderByDescending(c => c.Modified);
        }

        public IEnumerable<ChangeRequest> SearchChangeRequests(ChangeRequestSearchCriteria criteria)
        {
            var cr = from s in DbContext.ChangeRequests
                     select s;
            if (!String.IsNullOrEmpty(criteria.Name))
            {
                cr = cr.Where(s => s.Name.Contains(criteria.Name));
            }

            cr = cr.Where(s => s.Status == criteria.Status);

            if (criteria.TargetDateStart != null && criteria.TargetDateEnd != null)
            {
                cr = cr.Where(s => s.ActualDate <= criteria.TargetDateStart && s.ActualDate <= criteria.TargetDateEnd);
            }

            return cr.ToList();

        }


        public ChangeRequest GetChangeRequestbyId(int CRId)
        {
            return DbContext.ChangeRequests.Where(cr => cr.ID == CRId)
                .AsNoTracking()
                .Include(cr => cr.ChangeRequestTasks)
                .SingleOrDefault();
        }

        #endregion

        #region Create ChangeRequests

        /// <summary>
        /// create ChangeRequest and set defaults
        /// </summary>
        /// <returns></returns>
        public ChangeRequest CreateNewChangeRequest()
        {

            ChangeRequest cr = new ChangeRequest();
            cr.TargetDate = DateTime.Now.AddDays(7);
            cr.ActualDate = (DateTime?)null;
            cr.Status = ChangeRequest.StatusEnum.draft;

            return cr;
        }

        /// <summary>
        /// Create ChangeRequest and graph records in database
        /// </summary>
        /// <param name="cr"></param>
        public void CreateChangeRequest(ChangeRequest cr)
        {
            cr.Owner = CurrentUser;
            cr.ModifiedBy = CurrentUser;
            cr.Modified = DateTime.Now;

            DbContext.ChangeRequests.Add(cr);
            DbContext.SaveChanges();

        }

        #endregion

        #region Update ChangeRequests

        private void UpdateChangeRequestProperties(ChangeRequest newCr, ChangeRequest oldCr)
        {
            oldCr.Name = newCr.Name;
            oldCr.Owner = newCr.Owner;
            oldCr.Summary = newCr.Summary;
            oldCr.TargetDate = newCr.TargetDate;
            oldCr.Priority = newCr.Priority;
            oldCr.Urgency = newCr.Urgency;
            oldCr.Status = newCr.Status;
            oldCr.ActualDate = newCr.ActualDate;

            oldCr.ModifiedBy = CurrentUser;
            oldCr.Modified = DateTime.Now;
        }

        /// <summary>
        /// Update existing ChangeRequest and graph records in database
        /// </summary>
        /// <param name="cr"></param>
        public void UpdateChangeRequest(ChangeRequest changeRequest)
        {
            var changeRequestToSave = ContinueBusinessTransaction(changeRequest.ID);
            UpdateChangeRequestProperties(changeRequest, changeRequestToSave);

            DbContext.ChangeRequests.Update(changeRequestToSave);
            DbContext.SaveChanges();
        }

        #endregion

        #region Delete ChangeRequest

        public bool DeleteChangeRequest(int id)
        {
            ChangeRequest changeRequestToDelete = GetChangeRequestbyId(id);
            DbContext.Remove(changeRequestToDelete);
            DbContext.SaveChanges();

            return true;
        }

        #endregion

        #endregion

        #region ChangeRequestTask

        #region Create ChangeRequestTask

        public ChangeRequestTask CreateNewChangeRequestTask(int changeRequestId)
        {
            var changeRequest = ContinueBusinessTransaction(changeRequestId);
            if (changeRequest != null)
            {
                // Create new task and associate to ChangeRequest
                var task = new ChangeRequestTask();
                task.ChangeRequestID = changeRequest.ID;
                task.ChangeRequest = changeRequest;

                // used later to help find within collection prior to primary key being generated
                task.Modified = DateTime.Now;

                return task;
            }
            else
            {
                throw new Exception("Error creating task.");
            }

        }

        public int CreateChangeRequestTask(ChangeRequestTask task)
        {
            var changeRequest = ContinueBusinessTransaction(task.ChangeRequestID);
            if (changeRequest.ChangeRequestTasks == null)
            {
                changeRequest.ChangeRequestTasks = new List<ChangeRequestTask>();
            }

            changeRequest.ChangeRequestTasks.Add(task);

            UpdateBusinessEntity(changeRequest);

            return task.ID;
        }

        #endregion

        #region Get ChangeRequestTask

        public ChangeRequestTask GetChangeRequestTaskbyId(int taskId, string modified)
        {
            var changeRequest = this.GetCurrentEntityFromSession();
            ChangeRequestTask task;
            if (taskId == 0)
            {
                string dt = Convert.ToDateTime(modified).ToLongDateString();
                task = changeRequest.ChangeRequestTasks.Find(t =>
                    t.ID == taskId && t.Modified.ToLongDateString() == dt);
            }
            else
            {
                task = changeRequest.ChangeRequestTasks.Find(t => t.ID == taskId);
            }

            return task;
        }

        #endregion

        #region Update ChangeRequestTask

        public void UpdateChangeRequestTask(ChangeRequestTask task)
        {
            var changeRequest = ContinueBusinessTransaction(task.ChangeRequestID);
            ChangeRequestTask retrievedTask;

            if (task.ID == 0)
            {
                retrievedTask = changeRequest.ChangeRequestTasks.Find(t=>t.ID == task.ID && t.Modified.ToLongDateString()==task.Modified.ToLongDateString());
            }
            else 
            {
                retrievedTask = changeRequest.ChangeRequestTasks.Find(t=>t.ID == task.ID);
            }

            retrievedTask.CompletedDate = task.CompletedDate;
            retrievedTask.Name = task.Name;
            retrievedTask.Status = task.Status;
            retrievedTask.Summary = task.Summary;
            retrievedTask.Modified = DateTime.Now;
            retrievedTask.ModifiedBy = currentUser;

            this.UpdateBusinessEntity(changeRequest);
        }

        #endregion

        #region Delete ChangeRequestTask

        public bool DeleteChangeRequestTask(ChangeRequestTask task)
        {
            var changeRequest = ContinueBusinessTransaction(task.ChangeRequestID);
            ChangeRequestTask retrievedTask;

            if (task.ID == 0)
            {
                retrievedTask = changeRequest.ChangeRequestTasks.Find(t=>t.ID == task.ID && t.Modified.ToLongDateString()==task.Modified.ToLongDateString());
            }
            else 
            {
                retrievedTask = changeRequest.ChangeRequestTasks.Find(t=>t.ID == task.ID);
            }

            retrievedTask.Modified = DateTime.Now;
            retrievedTask.ModifiedBy = CurrentUser;

            UpdateBusinessEntity(changeRequest);

            return true;
        }

        #endregion

        #endregion

    }
}
