using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CargoCaptain.Data;
using CargoCaptain.Models;
using CargoCaptain.Interfaces;
using CargoCaptain.Enums;

namespace CargoCaptain.Services
{
    public class PortOperatorService : IPortOperatorService
    {
        private readonly ApplicationDbContext _context;

        public PortOperatorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Container>> GetAllContainersAsync()
        {
            return await _context.Containers
                .Include(c => c.ShipmentBooking)
                    .ThenInclude(sb => sb!.CustomsDeclaration)
                .Include(c => c.CargoEvents)
                .OrderByDescending(c => c.containerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Container>> GetContainersBySectionAsync(string section)
        {
            var query = _context.Containers
                .Include(c => c.ShipmentBooking)
                    .ThenInclude(sb => sb!.CustomsDeclaration)
                .Include(c => c.CargoEvents)
                .AsQueryable();

            if (section?.ToLower() == "export")
            {
                query = query.Where(c => c.ShipmentBooking != null 
                    && c.ShipmentBooking.CustomsDeclaration != null 
                    && c.ShipmentBooking.CustomsDeclaration.declarationType == DeclarationType.EXPORT);
            }
            else if (section?.ToLower() == "import")
            {
                query = query.Where(c => c.ShipmentBooking != null 
                    && c.ShipmentBooking.CustomsDeclaration != null 
                    && c.ShipmentBooking.CustomsDeclaration.declarationType == DeclarationType.IMPORT);
            }

            return await query.OrderByDescending(c => c.containerId).ToListAsync();
        }

        public async Task<Container?> GetContainerByIdAsync(int containerId)
        {
            return await _context.Containers
                .Include(c => c.ShipmentBooking)
                    .ThenInclude(sb => sb!.CustomsDeclaration)
                .Include(c => c.CargoEvents)
                .FirstOrDefaultAsync(c => c.containerId == containerId);
        }

        public async Task<IEnumerable<CargoEvent>> GetRecentEventsAsync(int count)
        {
            return await _context.CargoEvents
                .Include(ce => ce.Container)
                    .ThenInclude(c => c!.ShipmentBooking)
                .OrderByDescending(ce => ce.eventId)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<CargoEvent>> GetEventsByContainerIdAsync(int containerId)
        {
            return await _context.CargoEvents
                .Where(ce => ce.containerId == containerId)
                .OrderBy(ce => ce.eventTimestamp)
                .ToListAsync();
        }

        public async Task RecordCargoEventAsync(CargoEvent cargoEvent, string recordedBy)
        {
            var container = await _context.Containers
                .Include(c => c.ShipmentBooking)
                    .ThenInclude(sb => sb!.CustomsDeclaration)
                .Include(c => c.CargoEvents)
                .FirstOrDefaultAsync(c => c.containerId == cargoEvent.containerId);

            if (container == null)
            {
                throw new KeyNotFoundException("Container record not found.");
            }

            var booking = container.ShipmentBooking;
            if (booking == null)
            {
                throw new InvalidOperationException("Container is not associated with any active booking.");
            }

            // 1. Customs Clearance Assertion Check
            var customs = booking.CustomsDeclaration;
            if (customs == null || customs.clearanceStatus != ClearanceStatus.CLEARED)
            {
                throw new InvalidOperationException("Clearance block: Customs Clearance must be Approved (Cleared) before port operators can log milestones.");
            }

            // 2. Prevent duplicate milestone events
            var existingEvents = container.CargoEvents.OrderBy(e => e.eventTimestamp).ToList();
            if (existingEvents.Any(e => e.eventType == cargoEvent.eventType))
            {
                throw new InvalidOperationException($"Milestone event '{cargoEvent.eventType}' has already been registered for this container.");
            }

            // 3. Milestone Sequence State Machine Check
            if (!existingEvents.Any())
            {
                // First milestone must be GATE_IN (or SAILED if starting directly as import)
                if (cargoEvent.eventType != CargoEventType.GATE_IN && cargoEvent.eventType != CargoEventType.SAILED)
                {
                    throw new InvalidOperationException("First recorded container milestone must be Gate In (or Sailed for direct imports).");
                }

                if (cargoEvent.eventType == CargoEventType.GATE_IN)
                {
                    if (customs.declarationType != DeclarationType.EXPORT || customs.clearanceStatus != ClearanceStatus.CLEARED)
                    {
                        throw new InvalidOperationException("Gate In is blocked: Export Customs Clearance must be Approved first.");
                    }
                }
            }
            else
            {
                var lastEvent = existingEvents.Last();

                if (lastEvent.eventType == CargoEventType.GATE_IN)
                {
                    if (cargoEvent.eventType != CargoEventType.LOADED)
                    {
                        throw new InvalidOperationException("Invalid transition: Gated In container must be Loaded next.");
                    }
                }
                else if (lastEvent.eventType == CargoEventType.LOADED)
                {
                    if (cargoEvent.eventType != CargoEventType.SAILED)
                    {
                        throw new InvalidOperationException("Invalid transition: Loaded container must be Sailed next.");
                    }
                }
                else if (lastEvent.eventType == CargoEventType.SAILED)
                {
                    if (cargoEvent.eventType != CargoEventType.DISCHARGED)
                    {
                        throw new InvalidOperationException("Invalid transition: Sailed container must be Discharged next.");
                    }
                }
                else if (lastEvent.eventType == CargoEventType.DISCHARGED)
                {
                    if (cargoEvent.eventType != CargoEventType.GATE_OUT)
                    {
                        throw new InvalidOperationException("Invalid transition: Discharged container must be Gated Out next.");
                    }

                    // Strict Import Customs Clearance Check before GATE_OUT
                    if (customs.declarationType != DeclarationType.IMPORT || customs.clearanceStatus != ClearanceStatus.CLEARED)
                    {
                        throw new InvalidOperationException("Gate Out is blocked: Import Customs Clearance must be filed and Approved first.");
                    }
                }
                else if (lastEvent.eventType == CargoEventType.GATE_OUT)
                {
                    throw new InvalidOperationException("Gate Out is complete. No further container milestones can be recorded.");
                }
            }

            // 4. Timestamp Validation Checks
            if (cargoEvent.eventTimestamp > DateTime.UtcNow)
            {
                throw new InvalidOperationException("Event timestamp cannot be set in the future.");
            }

            if (existingEvents.Any())
            {
                var lastEvent = existingEvents.Last();
                if (cargoEvent.eventTimestamp < lastEvent.eventTimestamp)
                {
                    throw new InvalidOperationException("Event timestamp cannot be earlier than the previous recorded milestone's timestamp.");
                }
            }

            // 5. Update Container Status in database
            container.containerStatus = cargoEvent.eventType switch
            {
                CargoEventType.GATE_IN => ContainerStatus.EMPTY,
                CargoEventType.LOADED => ContainerStatus.LOADED,
                CargoEventType.SAILED => ContainerStatus.IN_TRANSIT,
                CargoEventType.DISCHARGED => ContainerStatus.DISCHARGED,
                CargoEventType.GATE_OUT => ContainerStatus.DISCHARGED,
                _ => container.containerStatus
            };

            // Setup Audit logging parameters
            cargoEvent.recordedBy = recordedBy;
            cargoEvent.createdDate = DateTime.UtcNow;

            _context.CargoEvents.Add(cargoEvent);
            _context.Entry(container).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Container>> GetContainersAwaitingActionAsync()
        {
            var all = await _context.Containers
                .Include(c => c.ShipmentBooking)
                    .ThenInclude(sb => sb!.CustomsDeclaration)
                .Include(c => c.CargoEvents)
                .ToListAsync();

            var awaiting = new List<Container>();

            foreach (var c in all)
            {
                if (c.ShipmentBooking?.CustomsDeclaration == null) continue;

                var type = c.ShipmentBooking.CustomsDeclaration.declarationType;
                var lastEvent = c.CargoEvents.OrderBy(e => e.eventTimestamp).LastOrDefault();

                if (type == DeclarationType.EXPORT)
                {
                    // Awaiting action if not sailed yet
                    if (lastEvent == null || lastEvent.eventType != CargoEventType.SAILED)
                    {
                        awaiting.Add(c);
                    }
                }
                else // IMPORT
                {
                    // Awaiting action if not gated out yet
                    if (lastEvent == null || lastEvent.eventType != CargoEventType.GATE_OUT)
                    {
                        awaiting.Add(c);
                    }
                }
            }

            return awaiting.OrderBy(c => c.containerId);
        }
    }
}
