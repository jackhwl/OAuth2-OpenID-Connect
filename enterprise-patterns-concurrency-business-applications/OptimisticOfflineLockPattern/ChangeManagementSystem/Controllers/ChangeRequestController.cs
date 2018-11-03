using ChangeManagementSystem.Models;
using ChangeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            _changeRequestRepository.UpdateChangeRequest(model, _userManager.GetUserName(this.User));
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Delete Change Request

        public async Task<IActionResult> Delete(int id)
        {
            bool result = await _changeRequestRepository.DeleteChangeRequest(id);
            return RedirectToAction("List");
        }

        #endregion 

    }
}
