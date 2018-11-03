using ChangeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ChangeManagementSystem.Controllers
{
    [Authorize]
    public class ChangeRequestTaskController : Controller
    {
        private readonly IChangeRepository _changeRequestRepository;
        private readonly UserManager<IdentityUser> _userManager;
        #region ctor

        // ASP.NET dependency injection will use implementation specified in Startup.cs
        public ChangeRequestTaskController(IChangeRepository changeRequestRepository, UserManager<IdentityUser> userManager)
        {
            _changeRequestRepository = changeRequestRepository;
            _userManager = userManager;
        }

        #endregion

        #region Get ChangeRequestTask

        public IActionResult View(int id)
        {
            var task = _changeRequestRepository.GetChangeRequestTaskbyId(id);
            return View(task.Result);
        }

        #endregion

        #region Create ChangeRequestTask

        public IActionResult New(int changeRequestId)
        {
            var changeRequest = _changeRequestRepository.GetChangeRequestbyId(changeRequestId).Result;
            var task = new ChangeRequestTask();
            task.ChangeRequestID = changeRequest.ID;

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ChangeRequestTask task)
        {
            if (ModelState.IsValid)
            {
                _changeRequestRepository.CreateChangeRequestTask(task, _userManager.GetUserName(this.User));
                return RedirectToAction("Edit", "ChangeRequest", new { id = task.ChangeRequestID });
            }
            else
            {
                ModelState.AddModelError("", "Values are not valid");
                return View("New", task);
            }
        }

        #endregion

        #region Edit ChangeRequestTask

        // GET: /<controller>/
        public IActionResult Edit(int id, int changeRequestId)
        {
            var task = _changeRequestRepository.GetChangeRequestTaskbyId(id);
            return View(task.Result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ChangeRequestTask task)
        {
            if (ModelState.IsValid)
            {
                _changeRequestRepository.UpdateChangeRequestTask(task, _userManager.GetUserName(this.User));
                
                return RedirectToAction("Edit", "ChangeRequest", new { id = task.ChangeRequestID });
            }
            else
            {
                ModelState.AddModelError("", "Values are not valid");
                return View("Edit", task);
            }
        }

        #endregion

        public async Task<IActionResult> Delete(int id, int changeRequestId)
        {
            bool result = await _changeRequestRepository.DeleteChangeRequestTask(id);
            return RedirectToAction("Edit", "ChangeRequest", new { id = changeRequestId });
        }

    }
}
