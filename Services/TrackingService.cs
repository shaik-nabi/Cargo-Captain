using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CargoCaptain.Data;
using CargoCaptain.Models;
using CargoCaptain.Interfaces;
using CargoCaptain.ViewModels;
using CargoCaptain.Enums;

namespace CargoCaptain.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly ApplicationDbContext _context;

        public TrackingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrackingDetailsViewModel?> GetTrackingDetailsAsync(string bookingNumber)
        {
            if (string.IsNullOrWhiteSpace(bookingNumber)) return null;

            var cleanNum = bookingNumber.Trim();

            var booking = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                    .ThenInclude(c => c.CargoEvents)
                .Include(sb => sb.CustomsDeclaration)
                .FirstOrDefaultAsync(sb => sb.bookingNumber.ToLower() == cleanNum.ToLower());

            if (booking == null) return null;

            var timeline = new List<TrackingTimelineItem>();

            // 1. Booking Confirmation Milestone
            timeline.Add(new TrackingTimelineItem
            {
                EventName = "Booking Confirmation",
                Timestamp = booking.bookingDate,
                Location = booking.originPort,
                Remarks = "Shipment booking has been registered and confirmed."
            });

            // 2. Container Allocation Milestone
            if (booking.Containers != null && booking.Containers.Any())
            {
                var containerNums = string.Join(", ", booking.Containers.Select(c => c.containerNumber));
                timeline.Add(new TrackingTimelineItem
                {
                    EventName = "Cargo Container Allocation",
                    Timestamp = booking.bookingDate,
                    Location = booking.originPort,
                    Remarks = $"Allocated containers: {containerNums}."
                });
            }

            // 3. Customs Clearance Milestone
            var customs = booking.CustomsDeclaration;
            if (customs != null && customs.clearanceStatus == ClearanceStatus.CLEARED)
            {
                timeline.Add(new TrackingTimelineItem
                {
                    EventName = $"Customs Clearance Approved ({customs.declarationType})",
                    Timestamp = booking.bookingDate,
                    Location = customs.declarationType == DeclarationType.EXPORT ? booking.originPort : booking.destinationPort,
                    Remarks = $"Customs declaration cleared successfully. Commercial cargo value: ${customs.declaredValue:N2}."
                });
            }

            // 4. Container Milestones (CargoEvents)
            var allEvents = new List<CargoEvent>();
            if (booking.Containers != null)
            {
                foreach (var c in booking.Containers)
                {
                    if (c.CargoEvents != null)
                    {
                        allEvents.AddRange(c.CargoEvents);
                    }
                }
            }

            foreach (var ce in allEvents.OrderBy(e => e.eventTimestamp))
            {
                timeline.Add(new TrackingTimelineItem
                {
                    EventName = $"Container Milestone: {FriendlyEventName(ce.eventType)}",
                    Timestamp = ce.eventTimestamp,
                    Location = ce.eventLocation,
                    Remarks = ce.remarks
                });
            }

            // Order chronological
            timeline = timeline.OrderBy(t => t.Timestamp).ToList();

            // Calculate progress and status based on latest cargo event
            int progress = 0;
            string status = "Booking Confirmed";
            DateTime? latestUpdate = timeline.Any() ? timeline.Max(t => t.Timestamp) : (DateTime?)null;

            var latestEvent = allEvents.OrderBy(e => e.eventTimestamp).LastOrDefault();
            
            var decType = customs?.declarationType ?? DeclarationType.EXPORT;

            if (latestEvent != null)
            {
                if (decType == DeclarationType.IMPORT)
                {
                    status = latestEvent.eventType switch
                    {
                        CargoEventType.SAILED => "Sailed / In Voyage Transit",
                        CargoEventType.DISCHARGED => "Discharged / Dest. Port Yard",
                        CargoEventType.GATE_OUT => "Gated Out / Delivered",
                        _ => "In Import Processing"
                    };

                    progress = latestEvent.eventType switch
                    {
                        CargoEventType.SAILED => 33,
                        CargoEventType.DISCHARGED => 66,
                        CargoEventType.GATE_OUT => 100,
                        _ => 0
                    };
                }
                else // EXPORT
                {
                    status = latestEvent.eventType switch
                    {
                        CargoEventType.GATE_IN => "Gated In / Terminal Yard",
                        CargoEventType.LOADED => "Loaded / Stowed on Vessel",
                        CargoEventType.SAILED => "Sailed / In Voyage Transit",
                        _ => "In Export Processing"
                    };

                    progress = latestEvent.eventType switch
                    {
                        CargoEventType.GATE_IN => 33,
                        CargoEventType.LOADED => 66,
                        CargoEventType.SAILED => 100,
                        _ => 0
                    };
                }
            }
            else
            {
                // Fallbacks if no container events are logged yet
                if (customs != null && customs.clearanceStatus == ClearanceStatus.CLEARED)
                {
                    status = "Customs Cleared / Awaiting Cargo Staging";
                    progress = 10;
                }
                else if (booking.Containers != null && booking.Containers.Any())
                {
                    status = "Containers Assigned / Awaiting Clearance";
                    progress = 5;
                }
            }

            return new TrackingDetailsViewModel
            {
                BookingNumber = booking.bookingNumber,
                OriginPort = booking.originPort,
                DestinationPort = booking.destinationPort,
                CargoDescription = booking.cargoDescription ?? "General Merchandise",
                CurrentStatus = status,
                ProgressPercentage = progress,
                LatestUpdate = latestUpdate,
                TimelineItems = timeline
            };
        }

        private string FriendlyEventName(CargoEventType type)
        {
            return type switch
            {
                CargoEventType.GATE_IN => "Gate In",
                CargoEventType.LOADED => "Loaded",
                CargoEventType.SAILED => "Sailed",
                CargoEventType.DISCHARGED => "Discharged",
                CargoEventType.GATE_OUT => "Gate Out",
                _ => type.ToString()
            };
        }
    }
}
