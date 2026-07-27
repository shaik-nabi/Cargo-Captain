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
    [Authorize(Roles = "FreightForwarder")]
    public class ContainerController : Controller
    {
        private readonly IContainerService _containerService;
        private readonly ApplicationDbContext _context;

        public ContainerController(IContainerService containerService, ApplicationDbContext context)
        {
            _containerService = containerService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var bookings = (await _containerService.GetAllBookingsAsync()).ToList();
            var pending = (await _containerService.GetPendingAllocationsAsync()).ToList();
            var completed = (await _containerService.GetCompletedAllocationsAsync()).ToList();
            var totalContainers = await _context.Containers.CountAsync();

            var viewModel = new ForwarderDashboardViewModel
            {
                TotalBookings = bookings.Count,
                PendingAllocationsCount = pending.Count,
                CompletedAllocationsCount = completed.Count,
                TotalContainers = totalContainers,
                RecentBookings = bookings.Take(5).ToList(),
                RecentAllocations = (await _containerService.GetRecentAllocationsAsync(5)).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> MasterBookings(string? filter = "all")
        {
            ViewBag.ActiveFilter = filter;

            IEnumerable<ShipmentBooking> bookings = filter?.ToLower() switch
            {
                "pending" => await _containerService.GetPendingAllocationsAsync(),
                "completed" => await _containerService.GetCompletedAllocationsAsync(),
                _ => await _containerService.GetAllBookingsAsync()
            };

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> Allocate(int bookingId)
        {
            var booking = await _containerService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            // Allocation Eligibility Assertions
            if (booking.bookingStatus == BookingStatus.CANCELLED)
            {
                TempData["ErrorMessage"] = "Cannot allocate containers to a cancelled booking.";
                return RedirectToAction(nameof(MasterBookings));
            }
            if (booking.bookingStatus == BookingStatus.COMPLETED)
            {
                TempData["ErrorMessage"] = "Cannot allocate containers to a completed booking.";
                return RedirectToAction(nameof(MasterBookings));
            }

            var viewModel = new ContainerAllocationViewModel
            {
                bookingId = booking.bookingId,
                bookingNumber = booking.bookingNumber,
                cargoDescription = booking.cargoDescription,
                cargoWeight = booking.cargoWeight,
                AllocatedContainers = booking.Containers.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Allocate(ContainerAllocationViewModel model)
        {
            var booking = await _containerService.GetBookingByIdAsync(model.bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            // Populate existing containers list to render in view even on errors
            model.bookingNumber = booking.bookingNumber;
            model.cargoDescription = booking.cargoDescription;
            model.cargoWeight = booking.cargoWeight;
            model.AllocatedContainers = booking.Containers.ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Double check booking state constraints
            if (booking.bookingStatus == BookingStatus.CANCELLED || booking.bookingStatus == BookingStatus.COMPLETED)
            {
                ModelState.AddModelError(string.Empty, "Booking is not eligible for allocation due to its status.");
                return View(model);
            }

            var container = new Container
            {
                containerNumber = model.containerNumber.Trim().ToUpper(),
                sealNumber = model.sealNumber.Trim(),
                containerType = model.containerType,
                containerStatus = model.containerStatus,
                bookingId = model.bookingId
            };

            try
            {
                await _containerService.AllocateContainerAsync(container);
                TempData["SuccessMessage"] = $"Container '{container.containerNumber}' allocated successfully.";
                return RedirectToAction(nameof(Allocate), new { bookingId = model.bookingId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("containerNumber", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id, int bookingId)
        {
            try
            {
                await _containerService.RemoveContainerAsync(id);
                TempData["SuccessMessage"] = "Container allocation removed successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Allocate), new { bookingId });
        }

        [HttpGet]
        public async Task<IActionResult> VesselManifest(int bookingId)
        {
            var booking = await _containerService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
    }
}
