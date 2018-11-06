using ChangeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ChangeManagementSystem.Concurrency
{
    public class LockManager
    {
        private AppDataContext _appDataContext;

        public LockManager(AppDataContext appDataContext)
        {
            _appDataContext = appDataContext;
        }

        public void AcquireLock(int id, string entityName, string owner)
        {
            if (!HasLock(id, entityName, owner))
            {
                try
                {
                    Lock _lock = new Lock
                    {
                        EntityId = id,
                        EntityName = entityName,
                        OwnerId = owner,
                        AcquiredDateTime = DateTime.Now
                    };

                    _appDataContext.Locks.Add(_lock);
                    _appDataContext.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    throw new ConcurrencyException(
                        "Entity is locked by another user and annot be edited at this time.");
                }
            }
        }
        public void ReleaseLock(int id, string entityName, string owner)
        {
            if (HasLock(id, entityName, owner))
            {
                Lock _lock = _appDataContext.Locks.FirstOrDefault(c =>
                    c.EntityId == id && c.EntityName == entityName && c.OwnerId == owner);
                if (_lock != null)
                {
                    try
                    {
                        _appDataContext.Locks.Remove(_lock);
                        _appDataContext.SaveChanges();
                    }
                    catch (Exception)
                    {
                        throw new ConcurrencyException("Unexpected error releasing lock on Entity.");
                    }
                }
            }

        }
        public bool HasLock(int id, string entityName, string owner)
        {
            Lock _lock = _appDataContext.Locks.AsNoTracking().FirstOrDefault(c =>
                c.EntityId == id && c.EntityName == entityName && c.OwnerId == owner);
            return _lock != null;
        }
        public void ReleaseAllLocks(string owner)
        {
            Lock[] _locks = _appDataContext.Locks.Where(c => c.OwnerId == owner).ToArray();
            if (_locks != null)
            {
                foreach (var _lock in _locks)
                {
                    _appDataContext.Locks.Remove(_lock);
                }

                _appDataContext.SaveChanges();
            }
        }
    }
}
