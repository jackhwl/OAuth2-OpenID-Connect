using ChangeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ChangeManagementSystem.Concurrency
{
    public class LockManager
    {
        private readonly int _lockExpirySeconds = 15;
        private AppDataContext _appDataContext;

        public LockManager(AppDataContext appDataContext)
        {
            _appDataContext = appDataContext;
        }

        public void AcquireLock(int id, string entityName, string owner)
        {
            RemoveExpiredLock(id, entityName);
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

        // ======= Create this function in DB =========
        //
        // CREATE FUNCTION GetLockWithUPDLock(@entityId int, @entityName nvarchar(450))
        // RETURNS TABLE
        // AS
        // RETURN 
        //      SELECT EntityId, EntityName, AcquiredDateTime, OwnerId
        //      FROM dbo.Locks WITH (UPDLOCK)
        //      WHERE EntityId = @entityId AND EntityName = @entityName

        private void RemoveExpiredLock(int id, string entityName)
        {
            Lock _lock = _appDataContext.Locks
                .FromSql("SELECT * FROM dbo.GetLockWithUPDLock({0},{1})", id, entityName)
                .AsNoTracking()
                .FirstOrDefault();

            if (_lock != null && _lock.AcquiredDateTime <= DateTime.Now.AddSeconds(-_lockExpirySeconds))
            {
                _appDataContext.Locks.Remove(_lock);
                _appDataContext.SaveChanges();
            }
        }

        public void RenewLock(int id, string entityName, string currentUser)
        {
            Lock _lock = _appDataContext.Locks
                .FromSql("SELECT * FROM dbo.GetLockWithUPDLock({0},{1})", id, entityName)
                .AsNoTracking()
                .FirstOrDefault();
            if (_lock != null && _lock.OwnerId == currentUser)
            {
                if (_lock.AcquiredDateTime <= DateTime.Now.AddSeconds(-_lockExpirySeconds))
                {
                    throw new ConcurrencyException("Lock on Entity has expired. Please restart Business Transaction.");
                }
                _lock.AcquiredDateTime = DateTime.Now;
                _appDataContext.SaveChanges();
            }
            else
            {
                throw new ConcurrencyException("Lock not found for user and Entity");
            }
        }

    }
}
