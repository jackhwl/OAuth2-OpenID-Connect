using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Controllers
{
    public class ApplicationExceptionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
