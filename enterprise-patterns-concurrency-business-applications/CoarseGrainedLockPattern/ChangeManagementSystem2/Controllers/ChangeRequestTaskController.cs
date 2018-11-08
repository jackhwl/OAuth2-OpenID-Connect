using ChangeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ChangeManagementSystem.ViewModels;

namespace ChangeManagementSystem.Controllers
{
    [Authorize]
    public class ChangeRequestTaskController : Controller
    {
        private readonly IChangeRepository _changeRequestRepository;
        private readonly UserManager<IdentityUser> _userManager;
        const string SessionKeyName = "_ChangeRequest"; 


        #region ctor

        // ASP.NET dependency injection will use implementation specified in Startup.cs
        public ChangeRequestTaskController(IChangeRepository changeRequestRepository, UserManager<IdentityUser> userManager)
        {
            _changeRequestRepository = changeRequestRepository;
            _userManager = userManager;
        }

        #endregion

        #region Get ChangeRequestTask

        public IActionResult View(int id, int changeRequestId, string returnView)
        {
            var task = _changeRequestRepository.GetChangeRequestTaskbyId(id);

            var vm = new ChangeRequestTaskViewModel();
            vm.Task = task;
            vm.ReturnView = returnView;

            return View("View", vm);
        }

        [HttpPost]
        public IActionResult View(ChangeRequestTaskViewModel vm)
        {
            return RedirectToAction(vm.ReturnView, "ChangeRequest", new { id = vm.Task.ChangeRequestID });
        }

        #endregion

        #region Create ChangeRequestTask

        public IActionResult New(int changeRequestId)
        {
            var task = _changeRequestRepository.CreateNewChangeRequestTask(changeRequestId);
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ChangeRequestTask task)
        {
            if (ModelState.IsValid)
            {
                _changeRequestRepository.CreateChangeRequestTask(task);
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
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ChangeRequestTask task)
        {
            if (ModelState.IsValid)
            {
                _changeRequestRepository.UpdateChangeRequestTask(task);
                return RedirectToAction("Edit", "ChangeRequest", new { id = task.ChangeRequestID });
            }
            else
            {
                ModelState.AddModelError("", "Values are not valid");
                return View("Edit", task);
            }
        }

        #endregion

        public IActionResult Delete(int id, int changeRequestId)
        {
            var task = _changeRequestRepository.GetChangeRequestTaskbyId(id);
            return View(task);   
        }

        [HttpPost]
        public IActionResult Delete(ChangeRequestTask task)
        {
            bool result = _changeRequestRepository.DeleteChangeRequestTask(task);
            return RedirectToAction("Edit", "ChangeRequest", new { id = task.ChangeRequestID });
        }
    }
}
