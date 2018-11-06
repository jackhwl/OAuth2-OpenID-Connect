using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChangeManagementSystem.Concurrency;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ChangeManagementSystem.Models
{
    public class ChangeRepository : IChangeRepository
    {
        private readonly AppDataContext _appDataContext;

        #region ctor

        /// <summary>
        /// Constructor for ChangeRepository
        /// </summary>
        /// <param name="AppDataContext">Configured through ASP.NET Core Dependency Injection (defined in Startup.ConfigureServices)</param>
        public ChangeRepository(AppDataContext AppDataContext)
        {
            _appDataContext = AppDataContext;

        }

        #endregion

        #region ChangeRequests

        #region Get ChangeRequests

        public IEnumerable<ChangeRequest> GetOpenChangeRequests()
        {
            return _appDataContext.ChangeRequests
                .Where(c => c.Status == ChangeRequest.StatusEnum.draft || c.Status == ChangeRequest.StatusEnum.inProgress)
                .Include("ChangeRequestTasks")
                .OrderByDescending(c => c.Modified);
        }

        public IEnumerable<ChangeRequest> SearchChangeRequests(ChangeRequestSearchCriteria criteria)
        {
            var cr = from s in _appDataContext.ChangeRequests
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

        public async Task<ChangeRequest> GetChangeRequestbyId(int CRId)
        {
            return await _appDataContext.ChangeRequests.Where(cr => cr.ID == CRId)
                .Include(cr => cr.ChangeRequestTasks)
                .SingleOrDefaultAsync();
        }

        public IEnumerable<ChangeRequest> ChangeRequests => _appDataContext.ChangeRequests.Include("ChangeRequestTasks");

        #endregion

        #region Create ChangeRequests

        public ChangeRequest CreateNewChangeRequest()
        {
            ChangeRequest cr = new ChangeRequest();
            cr.TargetDate = DateTime.Now.AddDays(7);
            cr.ActualDate = (DateTime?)null;
            cr.Status = ChangeRequest.StatusEnum.draft;
            return cr;
        }

        public void CreateChangeRequest(ChangeRequest cr, string currentUser)
        {
            cr.ModifiedBy = currentUser;
            cr.Modified = DateTime.Now;
            cr.Owner = currentUser;

            _appDataContext.ChangeRequests.Add(cr);
            _appDataContext.SaveChanges();

        }

        #endregion

        #region Update ChangeRequests

        public void UpdateChangeRequest(ChangeRequest cr, string currentUser)
        {
            cr.ModifiedBy = currentUser;
            cr.Modified = DateTime.Now;

            //_appDataContext.Update(cr);
            _appDataContext.ChangeTracker.TrackGraph(cr, e=> UpdateStateOfItems(e));
            
            _appDataContext.SaveChanges();
        }

        private void UpdateStateOfItems(EntityEntryGraphNode node)
        {
            node.Entry.State = EntityState.Modified;
            if (node.Entry.Entity.GetType() == typeof(ChangeRequestTask))
            {
                node.Entry.State = EntityState.Unchanged;
            }
        }

        #endregion

        #region Delete ChangeRequest

        public bool DeleteChangeRequest(int Id, byte[] rv)
        {
            using (var transaction = _appDataContext.Database.BeginTransaction())
            {
                var cr = _appDataContext.ChangeRequests
                    .FromSql("SELECT * FROM ChangeRequests WITH (UPDLOCK) WHERE Id= " + Id)
                    .FirstOrDefault();
                if (cr == null)
                {
                    // record has been deleted by another user
                    return true;
                }
                else if (!cr.RowVersion.SequenceEqual<byte>(rv))
                {
                    // record has been modified by another user
                    return false;
                }

                _appDataContext.ChangeRequests.Remove(cr);
                _appDataContext.SaveChanges();
                transaction.Commit();
                return true;
            }
        }

        #endregion

        #endregion

        #region ChangeRequestTask

        #region Create ChangeRequestTask

        public int CreateChangeRequestTask(ChangeRequestTask task, string currentUser)
        {
            task.ModifiedBy = currentUser;
            task.Modified = DateTime.Now;

            _appDataContext.ChangeRequestTasks.Add(task);
            _appDataContext.SaveChanges();

            return task.ID;
        }

        #endregion

        #region Get ChangeRequestTask

        public async Task<ChangeRequestTask> GetChangeRequestTaskbyId(int taskId)
        {
            return await _appDataContext.ChangeRequestTasks.Where(t => t.ID == taskId)
                .Include(t => t.ChangeRequest)
                .SingleOrDefaultAsync();
        }

        public ChangeRequestTask GetChangeRequestTaskByIdForEdit(int id, string currentUser)
        {
            using (var transaction = _appDataContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    LockManager lockManager = new LockManager(_appDataContext);
                    lockManager.AcquireLock(id, "ChangeRequestTask", currentUser);

                    ChangeRequestTask crt = _appDataContext.ChangeRequestTasks.Where(c => c.ID == id).SingleOrDefault();
                    if (crt != null)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        throw new ConcurrencyException(
                            "Entity not found. Entity man have been deleted by another user.");
                    }

                    return crt;
                }
                catch (ConcurrencyException ex)
                {
                    transaction.Rollback();
                    string newMessage = ex.Message.Replace("Entity", "Change Request Task " + id.ToString("D5"));
                    throw new ConcurrencyException(newMessage);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Update ChangeRequestTask


        public void UpdateChangeRequestTask(ChangeRequestTask task, string currentUser)
        {
            task.ModifiedBy = currentUser;
            task.Modified = DateTime.Now;
            using (var transaction = _appDataContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var lockManager = new LockManager(_appDataContext);
                    if (lockManager.HasLock(task.ID, "ChangeRequestTask", currentUser))
                    {
                        _appDataContext.ChangeRequestTasks.Update(task);
                        _appDataContext.SaveChanges();

                        lockManager.ReleaseLock(task.ID, "ChangeRequestTask", currentUser);
                        transaction.Commit();
                    }
                    else
                    {
                        throw new ConcurrencyException("User does not have a lock on Entity.  "
                            + "This may be due to a timeout. "
                            +"Please reload record and restart editing to prevent overwriting another user's changes.");
                    }
                }
                catch (ConcurrencyException ex)
                {
                    transaction.Rollback();
                    string newMessage = ex.Message.Replace("Entity", "Change Request Task " + task.ID.ToString("D5"));
                    throw new ConcurrencyException(newMessage);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region Delete ChangeRequestTask

        public async Task<bool> DeleteChangeRequestTask(int Id)
        {
            var task = await _appDataContext.ChangeRequestTasks
                .SingleOrDefaultAsync(m => m.ID == Id);
            if (task == null)
            {
                return false;
            }
            try
            {
                _appDataContext.ChangeRequestTasks.Remove(task);
                await _appDataContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // log error
                return false;
            }
        }

        #endregion

        #endregion

    }
}
