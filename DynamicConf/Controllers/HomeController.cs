using System.Diagnostics;
using DynamicConf.ConfigurationLibrary;
using DynamicConf.Models;
using Microsoft.AspNetCore.Mvc;

namespace DynamicConf.Controllers
{
    public class HomeController : Controller
    {
       
        private readonly ConfigurationReader _configReader;

        public HomeController(ConfigurationReader confReader)
        {
            _configReader = confReader;
        }

        public IActionResult Index()
        {
            try
            {
                string myValue = _configReader.GetValue<string>("IsBasketEnabled");
                ViewBag.ConfigValue = myValue;
            }
            catch (Exception ex)
            {
                ViewBag.ConfigValue = $"Hata: {ex.Message}";
            }

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
