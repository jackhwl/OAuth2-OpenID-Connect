using ChangeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Components
{
    public class ChangeRequestMenu : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var menuItems = new MenuLink[]
            {
                new MenuLink{ ControllerName="ChangeRequest", ControllerAction ="List", DisplayText="Open Change Requests"},
                new MenuLink{ ControllerName="ChangeRequest", ControllerAction ="Search", DisplayText="Search Change Requests"},
                new MenuLink{ ControllerName="ChangeRequest", ControllerAction ="New", DisplayText="New Change Request"}
            };

            return View(menuItems);
        }
    }
}
