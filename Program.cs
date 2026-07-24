using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using CargoCaptain.Data;
using CargoCaptain.Interfaces;
using CargoCaptain.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DB Context Configuration (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Add Cookie-based Authentication Services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// 3. Add Authorization Services
builder.Services.AddAuthorization();

// 4. Register Services in DI container
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IShipmentBookingService, ShipmentBookingService>();
builder.Services.AddScoped<IContainerService, ContainerService>();
builder.Services.AddScoped<ICustomsService, CustomsService>();
builder.Services.AddScoped<IPortOperatorService, PortOperatorService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();
builder.Services.AddScoped<IFreightInvoiceService, FreightInvoiceService>();

// 5. Register MVC Services (Controllers with Views)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 6. Configure the HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization Middlewares
app.UseAuthentication();
app.UseAuthorization();

// 7. Route Configuration
app.MapControllerRoute(
    name: "adminDashboard",
    pattern: "Admin/Dashboard",
    defaults: new { controller = "Admin", action = "Index" });

app.MapControllerRoute(
    name: "forwarderDashboard",
    pattern: "FreightForwarder/Dashboard",
    defaults: new { controller = "Container", action = "Index" });

app.MapControllerRoute(
    name: "customsDashboard",
    pattern: "CustomsBroker/Dashboard",
    defaults: new { controller = "Customs", action = "Index" });

app.MapControllerRoute(
    name: "portOperatorDashboard",
    pattern: "PortOperator/Dashboard",
    defaults: new { controller = "PortOperator", action = "Index" });

app.MapControllerRoute(
    name: "trackingSearch",
    pattern: "Tracking/Search",
    defaults: new { controller = "Tracking", action = "Search" });

app.MapControllerRoute(
    name: "invoiceDashboard",
    pattern: "FreightInvoice/Dashboard",
    defaults: new { controller = "FreightInvoice", action = "Index" });

app.MapControllerRoute(
    name: "shipperDashboard",
    pattern: "Shipper/Dashboard",
    defaults: new { controller = "ShipmentBooking", action = "Index" });

app.MapControllerRoute(
    name: "consigneeDashboard",
    pattern: "Consignee/Dashboard",
    defaults: new { controller = "Consignee", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
