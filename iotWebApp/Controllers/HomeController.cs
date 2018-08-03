using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using iotWebApp.Models;
using Microsoft.Extensions.Configuration;

namespace iotWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration Config;

        public HomeController(IConfiguration config)
        {
            Config = config;
        }

        public IActionResult Index()
        {
            ViewBag.Sample =Config["Sample"];
            ViewBag.Storage = Config["ConnectionStrings:Storage"];
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
