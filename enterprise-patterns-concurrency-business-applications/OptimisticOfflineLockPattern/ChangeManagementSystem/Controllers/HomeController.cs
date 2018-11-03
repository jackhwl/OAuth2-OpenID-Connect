using ChangeManagementSystem.Models;
using ChangeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChangeManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IChangeRepository _changeRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(IChangeRepository changeRepository, UserManager<IdentityUser> _userManager)
        {
            _changeRepository = changeRepository;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            var homeViewModel = new HomeViewModel
            {
                ChangeRequests = _changeRepository.GetOpenChangeRequests()
            };

            return View(homeViewModel);
        }
    }
}
