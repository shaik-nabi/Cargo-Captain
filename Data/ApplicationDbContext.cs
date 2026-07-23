using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CargoCaptain.Models;
using CargoCaptain.Enums;

namespace CargoCaptain.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ShipmentBooking> ShipmentBookings { get; set; }
        public DbSet<Container> Containers { get; set; }
        public DbSet<CustomsDeclaration> CustomsDeclarations { get; set; }
        public DbSet<CargoEvent> CargoEvents { get; set; }
        public DbSet<FreightInvoice> FreightInvoices { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configure Precision for Decimal Properties (18,2)
            modelBuilder.Entity<CustomsDeclaration>()
                .Property(cd => cd.declaredValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CustomsDeclaration>()
                .Property(cd => cd.calculatedDuty)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FreightInvoice>()
                .Property(fi => fi.freightCharges)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FreightInvoice>()
                .Property(fi => fi.surchargeAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FreightInvoice>()
                .Property(fi => fi.demurrageAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FreightInvoice>()
                .Property(fi => fi.totalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShipmentBooking>()
                .Property(sb => sb.cargoWeight)
                .HasPrecision(18, 2);

            // 2. Unique Constraints and Indexes
            modelBuilder.Entity<ShipmentBooking>()
                .HasIndex(sb => sb.bookingNumber)
                .IsUnique();

            modelBuilder.Entity<Container>()
                .HasIndex(c => c.containerNumber)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.email)
                .IsUnique();

            modelBuilder.Entity<FreightInvoice>()
                .HasIndex(fi => fi.invoiceNumber)
                .IsUnique();

            // 3. Configure Entity Relationships and Cascade Deletes
            
            // Container -> ShipmentBooking (Many-to-One)
            modelBuilder.Entity<Container>()
                .HasOne(c => c.ShipmentBooking)
                .WithMany(sb => sb.Containers)
                .HasForeignKey(c => c.bookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // CargoEvent -> Container (Many-to-One)
            modelBuilder.Entity<CargoEvent>()
                .HasOne(ce => ce.Container)
                .WithMany(c => c.CargoEvents)
                .HasForeignKey(ce => ce.containerId)
                .OnDelete(DeleteBehavior.Cascade);

            // CustomsDeclaration ↔ ShipmentBooking (True 1-to-1)
            modelBuilder.Entity<CustomsDeclaration>()
                .HasOne(cd => cd.ShipmentBooking)
                .WithOne(sb => sb.CustomsDeclaration)
                .HasForeignKey<CustomsDeclaration>(cd => cd.bookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // FreightInvoice ↔ ShipmentBooking (True 1-to-1)
            modelBuilder.Entity<FreightInvoice>()
                .HasOne(fi => fi.ShipmentBooking)
                .WithOne(sb => sb.FreightInvoice)
                .HasForeignKey<FreightInvoice>(fi => fi.bookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Employee ↔ Login (True 1-to-1)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Login)
                .WithOne(l => l.Employee)
                .HasForeignKey<Employee>(e => e.userId)
                .OnDelete(DeleteBehavior.Cascade);

            // ShipmentBooking -> Login (Many-to-One)
            modelBuilder.Entity<ShipmentBooking>()
                .HasOne(sb => sb.Login)
                .WithMany()
                .HasForeignKey(sb => sb.userId)
                .OnDelete(DeleteBehavior.Restrict);

            // FreightInvoice -> Login (Many-to-One)
            modelBuilder.Entity<FreightInvoice>()
                .HasOne(fi => fi.PaidByUser)
                .WithMany()
                .HasForeignKey(fi => fi.paidByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FreightInvoice>()
                .HasOne(fi => fi.DemurragePaidByUser)
                .WithMany()
                .HasForeignKey(fi => fi.demurragePaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Dynamic Password Hashing for Seed Logins
            var hasher = new PasswordHasher<Login>();

            var adminLogin = new Login 
            { 
                UserId = 1, 
                Role = UserRole.Admin, 
                AssociatedName = "System Admin" 
            };
            adminLogin.Password = hasher.HashPassword(adminLogin, "admin");

            var forwarderLogin = new Login 
            { 
                UserId = 2, 
                Role = UserRole.FreightForwarder, 
                AssociatedName = "Freight Forwarder Client" 
            };
            forwarderLogin.Password = hasher.HashPassword(forwarderLogin, "forwarder");

            var brokerLogin = new Login 
            { 
                UserId = 3, 
                Role = UserRole.CustomsBroker, 
                AssociatedName = "Customs Broker Client" 
            };
            brokerLogin.Password = hasher.HashPassword(brokerLogin, "broker");

            var operatorLogin = new Login 
            { 
                UserId = 4, 
                Role = UserRole.PortOperator, 
                AssociatedName = "Port Operator Client" 
            };
            operatorLogin.Password = hasher.HashPassword(operatorLogin, "operator");

            // Seed Logins
            modelBuilder.Entity<Login>().HasData(adminLogin, forwarderLogin, brokerLogin, operatorLogin);

            // Seed Employees linked to respective Logins
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    employeeId = 1,
                    firstName = "System",
                    lastName = "Admin",
                    email = "admin@cargocaptain.com",
                    phoneNumber = "+15550100",
                    userId = 1
                },
                new Employee
                {
                    employeeId = 2,
                    firstName = "Freight",
                    lastName = "Forwarder",
                    email = "forwarder@cargocaptain.com",
                    phoneNumber = "+15550101",
                    userId = 2
                },
                new Employee
                {
                    employeeId = 3,
                    firstName = "Customs",
                    lastName = "Broker",
                    email = "broker@cargocaptain.com",
                    phoneNumber = "+15550102",
                    userId = 3
                },
                new Employee
                {
                    employeeId = 4,
                    firstName = "Port",
                    lastName = "Operator",
                    email = "operator@cargocaptain.com",
                    phoneNumber = "+15550103",
                    userId = 4
                }
            );
        }
    }
}
