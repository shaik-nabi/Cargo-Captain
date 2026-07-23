using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using CargoCaptain.Interfaces;
using CargoCaptain.ViewModels;
using CargoCaptain.Enums;
using CargoCaptain.Models;

namespace CargoCaptain.Controllers
{
    [Authorize(Roles = "None")]
    public class ReportsController : Controller
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> ViewReport(string type, DateTime? start, DateTime? end, InvoiceStatus? status)
        {
            if (string.IsNullOrWhiteSpace(type)) return RedirectToAction(nameof(Index));

            var reportType = type.ToLower();

            // Strict role restrictions check
            if (User.IsInRole("FreightForwarder") && (reportType == "revenue" || reportType == "analytics"))
            {
                return Forbid();
            }

            ViewBag.ReportType = type;
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            ViewBag.Status = status;

            switch (reportType)
            {
                case "shipment":
                    var shipments = await _reportsService.GetShipmentReportAsync(start, end);
                    return View("ViewReport", shipments);
                case "invoice":
                    var invoices = await _reportsService.GetInvoiceReportAsync(status);
                    return View("ViewReport", invoices);
                case "revenue":
                    var rev = await _reportsService.GetRevenueReportAsync(start, end);
                    return View("ViewReport", rev);
                case "demurrage":
                    var dem = await _reportsService.GetDemurrageReportAsync();
                    return View("ViewReport", dem);
                case "booking":
                    var bkg = await _reportsService.GetBookingReportAsync();
                    return View("ViewReport", bkg);
                case "container":
                    var cnt = await _reportsService.GetContainerReportAsync();
                    return View("ViewReport", cnt);
                case "analytics":
                    // Gather some quick stats
                    var totalBkg = (await _reportsService.GetBookingReportAsync()).Sum(x => x.Count);
                    var totalCnt = (await _reportsService.GetContainerReportAsync()).Count();
                    var revSummary = await _reportsService.GetRevenueReportAsync(null, null);

                    ViewBag.TotalBookings = totalBkg;
                    ViewBag.TotalContainers = totalCnt;
                    ViewBag.TotalRevenue = revSummary.TotalRevenue;
                    ViewBag.PaidRevenue = revSummary.PaidRevenue;

                    return View("ViewReport");
                default:
                    return RedirectToAction(nameof(Index));
            }
        }

        // --- ClosedXML True Excel Export ---
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string type, DateTime? start, DateTime? end, InvoiceStatus? status)
        {
            if (string.IsNullOrWhiteSpace(type)) return BadRequest();

            var reportType = type.ToLower();
            if (User.IsInRole("FreightForwarder") && (reportType == "revenue" || reportType == "analytics"))
            {
                return Forbid();
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report Output");

                // Style headers
                var headerRow = 1;
                worksheet.Row(1).Style.Font.Bold = true;

                switch (reportType)
                {
                    case "shipment":
                        var shipments = await _reportsService.GetShipmentReportAsync(start, end);
                        worksheet.Cell(headerRow, 1).Value = "Booking Number";
                        worksheet.Cell(headerRow, 2).Value = "Shipper";
                        worksheet.Cell(headerRow, 3).Value = "Origin";
                        worksheet.Cell(headerRow, 4).Value = "Destination";
                        worksheet.Cell(headerRow, 5).Value = "Current Status";
                        worksheet.Cell(headerRow, 6).Value = "Latest Milestone";
                        worksheet.Cell(headerRow, 7).Value = "Booking Date";

                        var sIndex = 2;
                        foreach (var row in shipments)
                        {
                            worksheet.Cell(sIndex, 1).Value = row.BookingNumber;
                            worksheet.Cell(sIndex, 2).Value = row.Shipper;
                            worksheet.Cell(sIndex, 3).Value = row.Origin;
                            worksheet.Cell(sIndex, 4).Value = row.Destination;
                            worksheet.Cell(sIndex, 5).Value = row.CurrentStatus;
                            worksheet.Cell(sIndex, 6).Value = row.LatestMilestone;
                            worksheet.Cell(sIndex, 7).Value = row.BookingDate.ToString("yyyy-MM-dd");
                            sIndex++;
                        }
                        break;

                    case "invoice":
                        var invoices = await _reportsService.GetInvoiceReportAsync(status);
                        worksheet.Cell(headerRow, 1).Value = "Invoice Number";
                        worksheet.Cell(headerRow, 2).Value = "Booking Reference";
                        worksheet.Cell(headerRow, 3).Value = "Freight Charges ($)";
                        worksheet.Cell(headerRow, 4).Value = "Surcharge ($)";
                        worksheet.Cell(headerRow, 5).Value = "Demurrage ($)";
                        worksheet.Cell(headerRow, 6).Value = "Total Amount ($)";
                        worksheet.Cell(headerRow, 7).Value = "Status";
                        worksheet.Cell(headerRow, 8).Value = "Payment Date";

                        var iIndex = 2;
                        foreach (var row in invoices)
                        {
                            worksheet.Cell(iIndex, 1).Value = row.invoiceNumber;
                            worksheet.Cell(iIndex, 2).Value = row.ShipmentBooking?.bookingNumber;
                            worksheet.Cell(iIndex, 3).Value = row.freightCharges;
                            worksheet.Cell(iIndex, 4).Value = row.surchargeAmount;
                            worksheet.Cell(iIndex, 5).Value = row.demurrageAmount;
                            worksheet.Cell(iIndex, 6).Value = row.totalAmount;
                            worksheet.Cell(iIndex, 7).Value = row.invoiceStatus.ToString();
                            worksheet.Cell(iIndex, 8).Value = row.paymentDate?.ToString("yyyy-MM-dd") ?? "Unpaid";
                            iIndex++;
                        }
                        break;

                    case "revenue":
                        var rev = await _reportsService.GetRevenueReportAsync(start, end);
                        worksheet.Cell(headerRow, 1).Value = "Financial Metric Name";
                        worksheet.Cell(headerRow, 2).Value = "Value / Metric Output";

                        worksheet.Cell(2, 1).Value = "Total Revenue";
                        worksheet.Cell(2, 2).Value = rev.TotalRevenue;
                        worksheet.Cell(3, 1).Value = "Paid Revenue";
                        worksheet.Cell(3, 2).Value = rev.PaidRevenue;
                        worksheet.Cell(4, 1).Value = "Outstanding Revenue";
                        worksheet.Cell(4, 2).Value = rev.OutstandingRevenue;
                        worksheet.Cell(5, 1).Value = "Ocean Freight Total";
                        worksheet.Cell(5, 2).Value = rev.OceanFreightTotal;
                        worksheet.Cell(6, 1).Value = "Surcharge Total";
                        worksheet.Cell(6, 2).Value = rev.SurchargeTotal;
                        worksheet.Cell(7, 1).Value = "Demurrage Total";
                        worksheet.Cell(7, 2).Value = rev.DemurrageTotal;
                        worksheet.Cell(8, 1).Value = "Draft Count";
                        worksheet.Cell(8, 2).Value = rev.DraftCount;
                        worksheet.Cell(9, 1).Value = "Issued Count";
                        worksheet.Cell(9, 2).Value = rev.IssuedCount;
                        worksheet.Cell(10, 1).Value = "Paid Count";
                        worksheet.Cell(10, 2).Value = rev.PaidCount;
                        break;

                    case "demurrage":
                        var dem = await _reportsService.GetDemurrageReportAsync();
                        worksheet.Cell(headerRow, 1).Value = "Container Number";
                        worksheet.Cell(headerRow, 2).Value = "Booking Number";
                        worksheet.Cell(headerRow, 3).Value = "Arrival Date";
                        worksheet.Cell(headerRow, 4).Value = "Departure Date";
                        worksheet.Cell(headerRow, 5).Value = "Staging Days";
                        worksheet.Cell(headerRow, 6).Value = "Billable Days";
                        worksheet.Cell(headerRow, 7).Value = "Fee Amount ($)";

                        var dIndex = 2;
                        foreach (var row in dem)
                        {
                            worksheet.Cell(dIndex, 1).Value = row.ContainerNumber;
                            worksheet.Cell(dIndex, 2).Value = row.BookingNumber;
                            worksheet.Cell(dIndex, 3).Value = row.ArrivalDate?.ToString("yyyy-MM-dd") ?? "N/A";
                            worksheet.Cell(dIndex, 4).Value = row.DepartureDate?.ToString("yyyy-MM-dd") ?? "N/A";
                            worksheet.Cell(dIndex, 5).Value = row.TotalDays;
                            worksheet.Cell(dIndex, 6).Value = row.ChargeableDays;
                            worksheet.Cell(dIndex, 7).Value = row.DemurrageAmount;
                            dIndex++;
                        }
                        break;

                    case "booking":
                        var bookings = await _reportsService.GetBookingReportAsync();
                        worksheet.Cell(headerRow, 1).Value = "Booking Status Type";
                        worksheet.Cell(headerRow, 2).Value = "Bookings Tally Count";
                        worksheet.Cell(headerRow, 3).Value = "Total Cargo Weight (Tons)";

                        var bIndex = 2;
                        foreach (var row in bookings)
                        {
                            worksheet.Cell(bIndex, 1).Value = row.Status;
                            worksheet.Cell(bIndex, 2).Value = row.Count;
                            worksheet.Cell(bIndex, 3).Value = row.TotalWeight;
                            bIndex++;
                        }
                        break;

                    case "container":
                        var containers = await _reportsService.GetContainerReportAsync();
                        worksheet.Cell(headerRow, 1).Value = "Container Number";
                        worksheet.Cell(headerRow, 2).Value = "Container Type";
                        worksheet.Cell(headerRow, 3).Value = "Physical Status";
                        worksheet.Cell(headerRow, 4).Value = "Booking Reference";
                        worksheet.Cell(headerRow, 5).Value = "Current Location";
                        worksheet.Cell(headerRow, 6).Value = "Latest Milestone";

                        var cIndex = 2;
                        foreach (var row in containers)
                        {
                            worksheet.Cell(cIndex, 1).Value = row.ContainerNumber;
                            worksheet.Cell(cIndex, 2).Value = row.ContainerType;
                            worksheet.Cell(cIndex, 3).Value = row.CurrentStatus;
                            worksheet.Cell(cIndex, 4).Value = row.BookingNumber;
                            worksheet.Cell(cIndex, 5).Value = row.CurrentLocation;
                            worksheet.Cell(cIndex, 6).Value = row.LatestMilestone;
                            cIndex++;
                        }
                        break;

                    default:
                        return BadRequest();
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{type}_report.xlsx");
                }
            }
        }

        // --- PdfSharp True PDF Export ---
        [HttpGet]
        public async Task<IActionResult> ExportPDF(string type, DateTime? start, DateTime? end, InvoiceStatus? status)
        {
            if (string.IsNullOrWhiteSpace(type)) return BadRequest();

            var reportType = type.ToLower();
            if (User.IsInRole("FreightForwarder") && (reportType == "revenue" || reportType == "analytics"))
            {
                return Forbid();
            }

            var document = new PdfDocument();
            document.Info.Title = $"{type.ToUpper()} Report Summary";

            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // Configure Fonts
            var fontTitle = new XFont("Arial", 18, XFontStyleEx.Bold);
            var fontHeader = new XFont("Arial", 11, XFontStyleEx.Bold);
            var fontBody = new XFont("Arial", 9);

            // Draw Header Title
            gfx.DrawString($"CargoCaptain Report: {type.ToUpper()} Summary", fontTitle, XBrushes.Navy, new XPoint(40, 50));
            gfx.DrawString($"Report generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm UTC}", fontBody, XBrushes.DarkGray, new XPoint(40, 75));

            var y = 110.0;
            var marginX = 40;

            switch (reportType)
            {
                case "shipment":
                    var shipments = await _reportsService.GetShipmentReportAsync(start, end);
                    gfx.DrawString("Booking No", fontHeader, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString("Shipper", fontHeader, XBrushes.Black, new XPoint(140, y));
                    gfx.DrawString("Origin", fontHeader, XBrushes.Black, new XPoint(250, y));
                    gfx.DrawString("Destination", fontHeader, XBrushes.Black, new XPoint(340, y));
                    gfx.DrawString("Booking Date", fontHeader, XBrushes.Black, new XPoint(450, y));
                    y += 20;

                    foreach (var s in shipments.Take(25)) // Max list limits
                    {
                        gfx.DrawString(s.BookingNumber, fontBody, XBrushes.Black, new XPoint(marginX, y));
                        gfx.DrawString(s.Shipper.Length > 18 ? s.Shipper.Substring(0, 16) + ".." : s.Shipper, fontBody, XBrushes.Black, new XPoint(140, y));
                        gfx.DrawString(s.Origin, fontBody, XBrushes.Black, new XPoint(250, y));
                        gfx.DrawString(s.Destination, fontBody, XBrushes.Black, new XPoint(340, y));
                        gfx.DrawString(s.BookingDate.ToString("yyyy-MM-dd"), fontBody, XBrushes.Black, new XPoint(450, y));
                        y += 18;
                    }
                    break;

                case "invoice":
                    var invoices = await _reportsService.GetInvoiceReportAsync(status);
                    gfx.DrawString("Invoice No", fontHeader, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString("Booking No", fontHeader, XBrushes.Black, new XPoint(160, y));
                    gfx.DrawString("Total ($)", fontHeader, XBrushes.Black, new XPoint(280, y));
                    gfx.DrawString("Status", fontHeader, XBrushes.Black, new XPoint(380, y));
                    gfx.DrawString("Paid Date", fontHeader, XBrushes.Black, new XPoint(460, y));
                    y += 20;

                    foreach (var inv in invoices.Take(25))
                    {
                        gfx.DrawString(inv.invoiceNumber, fontBody, XBrushes.Black, new XPoint(marginX, y));
                        gfx.DrawString(inv.ShipmentBooking?.bookingNumber ?? "N/A", fontBody, XBrushes.Black, new XPoint(160, y));
                        gfx.DrawString($"${inv.totalAmount:N2}", fontBody, XBrushes.Black, new XPoint(280, y));
                        gfx.DrawString(inv.invoiceStatus.ToString(), fontBody, XBrushes.Black, new XPoint(380, y));
                        gfx.DrawString(inv.paymentDate?.ToString("yyyy-MM-dd") ?? "Unpaid", fontBody, XBrushes.Black, new XPoint(460, y));
                        y += 18;
                    }
                    break;

                case "revenue":
                    var rev = await _reportsService.GetRevenueReportAsync(start, end);
                    gfx.DrawString("Metric Type Name", fontHeader, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString("Financial Valuation (USD)", fontHeader, XBrushes.Black, new XPoint(280, y));
                    y += 25;

                    gfx.DrawString("Total Revenue", fontBody, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString($"${rev.TotalRevenue:N2}", fontBody, XBrushes.Black, new XPoint(280, y));
                    y += 18;
                    gfx.DrawString("Paid Revenue", fontBody, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString($"${rev.PaidRevenue:N2}", fontBody, XBrushes.Black, new XPoint(280, y));
                    y += 18;
                    gfx.DrawString("Outstanding Bills Total", fontBody, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString($"${rev.OutstandingRevenue:N2}", fontBody, XBrushes.Black, new XPoint(280, y));
                    y += 18;
                    gfx.DrawString("Ocean Freight Total Charges", fontBody, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString($"${rev.OceanFreightTotal:N2}", fontBody, XBrushes.Black, new XPoint(280, y));
                    y += 18;
                    gfx.DrawString("Staging Surcharges Total", fontBody, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString($"${rev.SurchargeTotal:N2}", fontBody, XBrushes.Black, new XPoint(280, y));
                    y += 18;
                    gfx.DrawString("Demurrage Delays Total", fontBody, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString($"${rev.DemurrageTotal:N2}", fontBody, XBrushes.Black, new XPoint(280, y));
                    break;

                case "demurrage":
                    var dem = await _reportsService.GetDemurrageReportAsync();
                    gfx.DrawString("Container No", fontHeader, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString("Booking No", fontHeader, XBrushes.Black, new XPoint(140, y));
                    gfx.DrawString("Arrived", fontHeader, XBrushes.Black, new XPoint(230, y));
                    gfx.DrawString("Days", fontHeader, XBrushes.Black, new XPoint(320, y));
                    gfx.DrawString("Billable", fontHeader, XBrushes.Black, new XPoint(380, y));
                    gfx.DrawString("Total Fee ($)", fontHeader, XBrushes.Black, new XPoint(450, y));
                    y += 20;

                    foreach (var row in dem.Take(25))
                    {
                        gfx.DrawString(row.ContainerNumber, fontBody, XBrushes.Black, new XPoint(marginX, y));
                        gfx.DrawString(row.BookingNumber, fontBody, XBrushes.Black, new XPoint(140, y));
                        gfx.DrawString(row.ArrivalDate?.ToString("yyyy-MM-dd") ?? "N/A", fontBody, XBrushes.Black, new XPoint(230, y));
                        gfx.DrawString(row.TotalDays.ToString(), fontBody, XBrushes.Black, new XPoint(320, y));
                        gfx.DrawString(row.ChargeableDays.ToString(), fontBody, XBrushes.Black, new XPoint(380, y));
                        gfx.DrawString($"${row.DemurrageAmount:N2}", fontBody, XBrushes.Black, new XPoint(450, y));
                        y += 18;
                    }
                    break;

                case "booking":
                    var bkgs = await _reportsService.GetBookingReportAsync();
                    gfx.DrawString("Booking Status Type", fontHeader, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString("Shipments Count", fontHeader, XBrushes.Black, new XPoint(220, y));
                    gfx.DrawString("Total Weight (Tons)", fontHeader, XBrushes.Black, new XPoint(340, y));
                    y += 20;

                    foreach (var row in bkgs)
                    {
                        gfx.DrawString(row.Status, fontBody, XBrushes.Black, new XPoint(marginX, y));
                        gfx.DrawString(row.Count.ToString(), fontBody, XBrushes.Black, new XPoint(220, y));
                        gfx.DrawString($"{row.TotalWeight:N2} Tons", fontBody, XBrushes.Black, new XPoint(340, y));
                        y += 18;
                    }
                    break;

                case "container":
                    var conts = await _reportsService.GetContainerReportAsync();
                    gfx.DrawString("Container No", fontHeader, XBrushes.Black, new XPoint(marginX, y));
                    gfx.DrawString("Type", fontHeader, XBrushes.Black, new XPoint(140, y));
                    gfx.DrawString("Booking No", fontHeader, XBrushes.Black, new XPoint(220, y));
                    gfx.DrawString("Physical Status", fontHeader, XBrushes.Black, new XPoint(330, y));
                    gfx.DrawString("Latest Milestone", fontHeader, XBrushes.Black, new XPoint(440, y));
                    y += 20;

                    foreach (var row in conts.Take(25))
                    {
                        gfx.DrawString(row.ContainerNumber, fontBody, XBrushes.Black, new XPoint(marginX, y));
                        gfx.DrawString(row.ContainerType, fontBody, XBrushes.Black, new XPoint(140, y));
                        gfx.DrawString(row.BookingNumber, fontBody, XBrushes.Black, new XPoint(220, y));
                        gfx.DrawString(row.CurrentStatus, fontBody, XBrushes.Black, new XPoint(330, y));
                        gfx.DrawString(row.LatestMilestone, fontBody, XBrushes.Black, new XPoint(440, y));
                        y += 18;
                    }
                    break;

                default:
                    return BadRequest();
            }

            using (var stream = new MemoryStream())
            {
                document.Save(stream);
                return File(stream.ToArray(), "application/pdf", $"{type}_report.pdf");
            }
        }
    }
}
