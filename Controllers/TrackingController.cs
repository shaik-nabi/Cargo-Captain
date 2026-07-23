using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CargoCaptain.Interfaces;
using CargoCaptain.ViewModels;

namespace CargoCaptain.Controllers
{
    public class TrackingController : Controller
    {
        private readonly ITrackingService _trackingService;

        public TrackingController(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? bookingNumber)
        {
            ViewBag.SearchQuery = bookingNumber;

            if (string.IsNullOrWhiteSpace(bookingNumber))
            {
                return View();
            }

            var cleanNum = bookingNumber.Trim();

            var trackingDetails = await _trackingService.GetTrackingDetailsAsync(cleanNum);

            if (trackingDetails == null)
            {
                ViewBag.ErrorMessage = "Tracking number not found. Please verify the code and try again.";
                return View();
            }

            return View(trackingDetails);
        }
    }
}
