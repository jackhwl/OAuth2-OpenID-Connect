﻿using ChangeManagementSystem.Models;
using ChangeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChangeManagementSystem.Controllers
{
    [Authorize]
    public class ChangeRequestController : Controller
    {
        private readonly IChangeRepository _changeRequestRepository;
        
        #region ctor

        // ASP.NET dependency injection will use implementation specified in Startup.cs
        public ChangeRequestController(IChangeRepository changeRequestRepository)
        {
            _changeRequestRepository = changeRequestRepository;
        }

        #endregion

        #region List and Search

        public ViewResult List()
        {
            ChangeRequestListViewModel crViewModel = new ChangeRequestListViewModel();
            crViewModel.ChangeRequests = _changeRequestRepository.GetOpenChangeRequests();

            return View(crViewModel);
        }

        public ViewResult Search()
        {
            ChangeRequestSearchViewModel vm = new ChangeRequestSearchViewModel();
            ChangeRequestSearchCriteria criteria = new ChangeRequestSearchCriteria();
            vm.Criteria = criteria;
            vm.ChangeRequests = new List<ChangeRequest>();
            return View(vm);
        }

        public IActionResult SearchSubmit(ChangeRequestSearchViewModel vm)
        {
            vm.ChangeRequests = _changeRequestRepository.SearchChangeRequests(vm.Criteria);
            return View("Search", vm);
        }

        #endregion

        #region View Change Request

        public IActionResult View(int id)
        {
            var cr = _changeRequestRepository.GetChangeRequestbyId(id);
            return View(cr);
        }

        #endregion

        #region New Change Request

        // GET: /<controller>/
        public IActionResult New()
        {
            var cr = _changeRequestRepository.CreateNewChangeRequest();
            return View(cr);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ChangeRequest cr)
        {
            if (ModelState.IsValid)
            {
                _changeRequestRepository.CreateChangeRequest(cr);
                return RedirectToAction("Edit", new { id = cr.ID });
            }
            else
            {
                ModelState.AddModelError("", "Values are not valid");
                return View("New", cr);
            }
        }

        #endregion

        #region Edit Change Request



        // GET: /<controller>/
        public IActionResult Edit(int id)
        {
            var changeRequest = _changeRequestRepository.ContinueBusinessTransaction(id);
            if (changeRequest == null)
            {
                changeRequest = _changeRequestRepository.GetChangeRequestbyId(id);
                _changeRequestRepository.StartBusinessTransaction(changeRequest);
            }

            ChangeRequestViewModel vm = new ChangeRequestViewModel
            {
                ChangeRequest = changeRequest
            };

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ChangeRequestViewModel model)
        {
            try
            {
                _changeRequestRepository.UpdateChangeRequest(model.ChangeRequest);
                _changeRequestRepository.EndBusinessTransaction();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred saving the record.");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult NewChangeRequestTask(ChangeRequestViewModel model)
        {
            _changeRequestRepository.ContinueBusinessTransaction(model.ChangeRequest);
            return RedirectToAction("New", "ChangeRequestTask", new { changeRequestId = model.ChangeRequest.ID });
        }

        [HttpPost]
        public IActionResult ViewChangeRequestTask(ChangeRequestViewModel model)
        {
            _changeRequestRepository.ContinueBusinessTransaction(model.ChangeRequest);
            return RedirectToAction("View", "ChangeRequestTask", new { id = model.TaskId, changeRequestId = model.ChangeRequest.ID, modified = model.TaskModifiedTime, returnView = "Edit" });
        }


        [HttpPost]
        public IActionResult EditChangeRequestTask(ChangeRequestViewModel model)
        {
            _changeRequestRepository.ContinueBusinessTransaction(model.ChangeRequest);
            return RedirectToAction("Edit", "ChangeRequestTask", new { id = model.TaskId, changeRequestId = model.ChangeRequest.ID, modified = model.TaskModifiedTime });
        }

        [HttpPost]
        public IActionResult DeleteChangeRequestTask(ChangeRequestViewModel model)
        {
            _changeRequestRepository.ContinueBusinessTransaction(model.ChangeRequest);
            return RedirectToAction("Delete", "ChangeRequestTask", new { id = model.TaskId, changeRequestId = model.ChangeRequest.ID, modified = model.TaskModifiedTime });
        }

        [HttpPost]
        public IActionResult NavigateToEditSummary(ChangeRequestViewModel model)
        {
            _changeRequestRepository.ContinueBusinessTransaction(model.ChangeRequest);
            return RedirectToAction("EditSummary", "ChangeRequest", new { id = model.ChangeRequest.ID });
        }

        [HttpPost]
        public IActionResult Cancel()
        {
            _changeRequestRepository.EndBusinessTransaction();
            return RedirectToAction("Index", "Home");
        }

        // GET: /<controller>/
        public IActionResult EditSummary(int id)
        {
            var changeRequest = _changeRequestRepository.ContinueBusinessTransaction(id);

            if (changeRequest == null)
            {
                return RedirectToAction("index", "Home");
            }

            return View(changeRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSummary(ChangeRequest model, string Save, string Cancel)
        {
            if(Save != null)
            {
                var changeRequest = _changeRequestRepository.ContinueBusinessTransaction(model.ID);
                changeRequest.Summary = model.Summary;
                _changeRequestRepository.ContinueBusinessTransaction(changeRequest);
            }
            return RedirectToAction("Edit", new { id = model.ID });
        }

        #endregion

        #region Delete Change Request

        public IActionResult Delete(int id)
        {
            bool result = _changeRequestRepository.DeleteChangeRequest(id);
            return RedirectToAction("Index", "Home");
        }
        #endregion 

    }
}
