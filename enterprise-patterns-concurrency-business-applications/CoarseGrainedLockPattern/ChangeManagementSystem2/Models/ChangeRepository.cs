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
                .Include(cr => cr.SharedVersion)
                .Include(cr => cr.ChangeRequestTasks)
                .ThenInclude(crt => crt.SharedVersion)
                .SingleOrDefault();
        }

        public ChangeRequest GetChangeRequestByVersionId(int id)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDataContext>();
            optionsBuilder.UseSqlServer(Startup.ConnectionString);
            using (var newContext = new AppDataContext(optionsBuilder.Options))
            {
                var changeRequest = newContext.ChangeRequests
                    .AsNoTracking()
                    .Include("SharedVersion")
                    .Include("ChangeRequestTasks")
                    .Where(c => c.SharedVersionId == id).FirstOrDefault();

                return changeRequest;
            }
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

            var version = new Version();
            cr.SharedVersion = version;
            cr.SharedVersionId = version.ID;

            cr.State = TrackedEntityState.Unchanged;

            return cr;
        }

        /// <summary>
        /// Create ChangeRequest and graph records in database
        /// </summary>
        /// <param name="cr"></param>
        public void CreateChangeRequest(ChangeRequest cr)
        {
            cr.State = TrackedEntityState.Added;

            cr.Owner = CurrentUser;
            cr.ModifiedBy = CurrentUser;
            cr.Modified = DateTime.Now;
            cr.SharedVersion.State = TrackedEntityState.Added;

            //DbContext.ChangeRequests.Add(cr);
            DbContext.ChangeTracker.TrackGraph(cr, e => UpdateStateOfItems(e));
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

            oldCr.SharedVersion.RowVersion = newCr.SharedVersion.RowVersion;
            oldCr.State = TrackedEntityState.Modified;

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

            //DbContext.ChangeRequests.Update(changeRequestToSave);
            DbContext.ChangeTracker.TrackGraph(changeRequestToSave, e => UpdateStateOfItems(e));
            DbContext.SaveChanges();
        }

        private void UpdateStateOfItems(EntityEntryGraphNode node)
        {
            if (node.Entry.Entity.GetType().BaseType == typeof(EntityBase))
            {
                if (((EntityBase) node.Entry.Entity).State == TrackedEntityState.Added)
                {
                    node.Entry.State = EntityState.Added;
                } else if (((EntityBase) node.Entry.Entity).State == TrackedEntityState.Modified)
                {
                    node.Entry.State = EntityState.Modified;
                } else if (((EntityBase) node.Entry.Entity).State == TrackedEntityState.Deleted)
                {
                    if (((EntityBase) node.Entry.Entity).ID == 0)
                    {
                        node.Entry.State = EntityState.Unchanged;
                    }
                    else
                    {
                        node.Entry.State = EntityState.Deleted;
                    }
                }
            }
            else
            {
                if (node.Entry.IsKeySet)
                {
                    node.Entry.State = EntityState.Modified;
                } else if (node.Entry.State != EntityState.Deleted)
                {
                    node.Entry.State = EntityState.Added;
                    ((EntityBase) node.Entry.Entity).ID = 0;
                }
            }

            if (node.Entry.Entity.GetType() == typeof(Version))
            {
                if (node.Entry.State != EntityState.Added)
                {
                    node.Entry.State = EntityState.Modified;
                }

                node.Entry.CurrentValues["Modified"] = DateTime.Now;
                node.Entry.CurrentValues["ModifiedBy"] = CurrentUser;
            }
        }

        #endregion

        #region Delete ChangeRequest

        public bool DeleteChangeRequest(int id)
        {
            ChangeRequest changeRequestToDelete = GetChangeRequestbyId(id);
            changeRequestToDelete.State = TrackedEntityState.Deleted;
            //DbContext.Remove(changeRequestToDelete);
            DbContext.ChangeTracker.TrackGraph(changeRequestToDelete, e => UpdateStateOfItems(e));
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
            task.ModifiedBy = CurrentUser;
            task.Modified = DateTime.Now;

            task.State = TrackedEntityState.Added;
            var changeRequest = ContinueBusinessTransaction(task.ChangeRequestID);
            if (changeRequest.ChangeRequestTasks == null)
            {
                changeRequest.ChangeRequestTasks = new List<ChangeRequestTask>();
            }

            task.SharedVersionId = changeRequest.SharedVersion.ID;
            task.SharedVersion = changeRequest.SharedVersion;

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

            retrievedTask.State = TrackedEntityState.Modified;

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

            retrievedTask.State = TrackedEntityState.Deleted;

            retrievedTask.Modified = DateTime.Now;
            retrievedTask.ModifiedBy = CurrentUser;

            UpdateBusinessEntity(changeRequest);

            return true;
        }

        #endregion

        #endregion

    }
}







