using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Juna.Feed.WebApp.Models;
using Juna.Feed.WebApp.Proxy;

namespace Juna.Feed.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly TestServiceProxy testService;

        public HomeController(TestServiceProxy testService)
        {
            this.testService = testService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> AboutAsync()
        {
            ViewData["Message"] = $"Hello {User.Identity.Name}!";
            ViewData["Values"] = await testService.GetValuesAsync();

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Contact";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
