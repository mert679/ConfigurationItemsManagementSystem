using DynamicConf.Interface;
using Microsoft.AspNetCore.Mvc;
using DynamicConf.Models;
using Microsoft.EntityFrameworkCore;
using DynamicConf.ConfigurationLibrary;
namespace DynamicConf.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly IConfigurationRepository _configurationRepository;

        public ConfigurationController(IConfigurationRepository configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }
        public async Task<IActionResult> Index(string? search)
        {
            return View(await _configurationRepository.GetAllConfigurations(search));
        }

        // GETView: Configuration/Create
        public IActionResult Create(){return View();}
        [HttpPost]
        public async Task<IActionResult> Create(Models.ConfigurationItems items) {
            if (ModelState.IsValid)
            {
                await _configurationRepository.AddConfiguration(items);
                return RedirectToAction("Index");
            }
            return View(items);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _configurationRepository.GetConfigurationById(id);
            return View(item);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Models.ConfigurationItems item)
        {
            if (ModelState.IsValid)
            {
                await _configurationRepository.UpdateConfiguration(item);
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

    }
}
