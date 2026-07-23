using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CargoCaptain.Models;
using CargoCaptain.Interfaces;
using CargoCaptain.ViewModels;
using CargoCaptain.Enums;
using CargoCaptain.Data;

namespace CargoCaptain.Controllers
{
    [Authorize(Roles = "PortOperator")]
    public class PortOperatorController : Controller
    {
        private readonly IPortOperatorService _portService;
        private readonly ApplicationDbContext _context;

        public PortOperatorController(IPortOperatorService portService, ApplicationDbContext context)
        {
            _portService = portService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var containers = (await _portService.GetAllContainersAsync()).ToList();

            // Direct counts from DB queries for exact numbers
            var gatedIn = await _context.CargoEvents.Where(e => e.eventType == CargoEventType.GATE_IN).Select(e => e.containerId).Distinct().CountAsync();
            var loaded = await _context.CargoEvents.Where(e => e.eventType == CargoEventType.LOADED).Select(e => e.containerId).Distinct().CountAsync();
            var sailed = await _context.CargoEvents.Where(e => e.eventType == CargoEventType.SAILED).Select(e => e.containerId).Distinct().CountAsync();
            var discharged = await _context.CargoEvents.Where(e => e.eventType == CargoEventType.DISCHARGED).Select(e => e.containerId).Distinct().CountAsync();
            var gatedOut = await _context.CargoEvents.Where(e => e.eventType == CargoEventType.GATE_OUT).Select(e => e.containerId).Distinct().CountAsync();

            var viewModel = new PortDashboardViewModel
            {
                TotalContainers = containers.Count,
                GatedInCount = gatedIn,
                LoadedCount = loaded,
                SailedCount = sailed,
                DischargedCount = discharged,
                GateOutCount = gatedOut,
                RecentMilestones = (await _portService.GetRecentEventsAsync(5)).ToList(),
                AwaitingActionContainers = (await _portService.GetContainersAwaitingActionAsync()).Take(5).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Containers(string? section = "all")
        {
            ViewBag.ActiveSection = section;

            IEnumerable<Container> containerList = section?.ToLower() switch
            {
                "export" => await _portService.GetContainersBySectionAsync("export"),
                "import" => await _portService.GetContainersBySectionAsync("import"),
                _ => await _portService.GetAllContainersAsync()
            };

            return View(containerList);
        }

        [HttpGet]
        public async Task<IActionResult> RecordEvent(int containerId)
        {
            var container = await _portService.GetContainerByIdAsync(containerId);
            if (container == null)
            {
                return NotFound();
            }

            var viewModel = new RecordMilestoneViewModel
            {
                containerId = container.containerId,
                containerNumber = container.containerNumber,
                EventHistory = container.CargoEvents.OrderBy(e => e.eventTimestamp).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordEvent(RecordMilestoneViewModel model)
        {
            var container = await _portService.GetContainerByIdAsync(model.containerId);
            if (container == null)
            {
                return NotFound();
            }

            model.containerNumber = container.containerNumber;
            model.EventHistory = container.CargoEvents.OrderBy(e => e.eventTimestamp).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Combine split inputs (Date & Time) to form eventTimestamp
            var eventDateTime = model.eventDate.Date + model.eventTime;

            var cargoEvent = new CargoEvent
            {
                containerId = model.containerId,
                eventType = model.eventType,
                eventLocation = model.eventLocation.Trim(),
                eventTimestamp = eventDateTime,
                remarks = model.remarks?.Trim() ?? string.Empty
            };

            var recorder = User.Identity?.Name ?? "Port Operator";

            try
            {
                await _portService.RecordCargoEventAsync(cargoEvent, recorder);
                TempData["SuccessMessage"] = $"Milestone '{model.eventType}' recorded successfully for container '{container.containerNumber}'.";
                return RedirectToAction(nameof(RecordEvent), new { containerId = model.containerId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Milestones(int containerId)
        {
            var container = await _portService.GetContainerByIdAsync(containerId);
            if (container == null)
            {
                return NotFound();
            }

            return View(container);
        }
    }
}
