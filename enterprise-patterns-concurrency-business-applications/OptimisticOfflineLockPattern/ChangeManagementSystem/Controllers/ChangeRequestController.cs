using System;
using ChangeManagementSystem.Models;
using ChangeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChangeManagementSystem.Controllers
{
    [Authorize]
    public class ChangeRequestController : Controller
    {
        private readonly IChangeRepository _changeRequestRepository;
        private readonly UserManager<IdentityUser> _userManager;
        #region ctor

        // ASP.NET dependency injection will use implementation specified in Startup.cs
        public ChangeRequestController(IChangeRepository changeRequestRepository, UserManager<IdentityUser> userManager)
        {
            _changeRequestRepository = changeRequestRepository;
            _userManager = userManager;
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
            var cr = _changeRequestRepository.GetChangeRequestbyId(id).Result;
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
                _changeRequestRepository.CreateChangeRequest(cr, _userManager.GetUserName(this.User));
                return RedirectToAction("Index", "Home");
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
            var cr = _changeRequestRepository.GetChangeRequestbyId(id).Result;
            return View(cr);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ChangeRequest model)
        {
            try
            {
                _changeRequestRepository.UpdateChangeRequest(model, _userManager.GetUserName(this.User));
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var exceptionEntry = ex.Entries.Single();
                if (exceptionEntry.Entity.GetType() == typeof(ChangeRequest))
                {
                    var clientValues = (ChangeRequest) exceptionEntry.Entity;
                    var dbEntry = exceptionEntry.GetDatabaseValues();
                    if (dbEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. Change Request was deleted by another user.");
                    }
                    else
                    {
                        var dbValues = (ChangeRequest) dbEntry.ToObject();
                        if (dbValues.Name != clientValues.Name)
                        {
                            ModelState.AddModelError("Name", $"Current Value: {dbValues.Name}");
                        }

                        if (dbValues.Summary != clientValues.Summary)
                        {
                            ModelState.AddModelError("Summary", $"Current Value: {dbValues.Summary}");
                        }

                        if (dbValues.Status != clientValues.Status)
                        {
                            ModelState.AddModelError("Status", $"Current Value: {dbValues.Status}");
                        }

                        if (dbValues.Priority != clientValues.Priority)
                        {
                            ModelState.AddModelError("Priority", $"Current Value: {dbValues.Priority}");
                        }

                        if (dbValues.Urgency != clientValues.Urgency)
                        {
                            ModelState.AddModelError("Urgency", $"Current Value: {dbValues.Urgency}");
                        }

                        if (dbValues.TargetDate != clientValues.TargetDate)
                        {
                            ModelState.AddModelError("TargetDate", $"Current Value: {dbValues.TargetDate}");
                        }

                        if (dbValues.ActualDate != clientValues.ActualDate)
                        {
                            ModelState.AddModelError("ActualDate", $"Current Value: {dbValues.ActualDate}");
                        }

                        if (dbValues.Owner != clientValues.Owner)
                        {
                            ModelState.AddModelError("Owner", $"Current Value: {dbValues.Owner}");
                        }

                        if (dbValues.Modified != clientValues.Modified)
                        {
                            ModelState.AddModelError("Modified", $"Current Value: {dbValues.Modified}");
                        }

                        if (dbValues.ModifiedBy != clientValues.ModifiedBy)
                        {
                            ModelState.AddModelError("ModifiedBy", $"Current Value: {dbValues.ModifiedBy}");
                        }

                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                               + "was modified by another user after you got the original value. The "
                                                               + "edit operation was cancelled and the current values in the database "
                                                               + "have been displayed.");

                        model.RowVersion = (byte[]) dbValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred saving the record.");
            }

            return View(model);
        }

        #endregion

        #region Delete Change Request

        public async Task<IActionResult> Delete(int id, string rowversion)
        {
            var rv = System.Text.Encoding.Unicode.GetBytes(rowversion);
            bool result = _changeRequestRepository.DeleteChangeRequest(id, rv);
            if (result == false)
            {
                TempData["ConcurrencyErrorMessage"] = "Record " + id.ToString("D5") + " was modified "
                                                      + "by another user after you got the original value. Please click View to see "
                                                      + "the latest values, or click the Delete button again to continue deleting "
                                                      + "the record";
            }
            return RedirectToAction("List");
        }

        #endregion 

    }
}
