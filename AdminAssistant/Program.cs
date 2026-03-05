using AdminAssistant.Core.Interfaces;
using AdminAssistant.Data;
using AdminAssistant.Data.Context;
using AdminAssistant.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Blazor & Razor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Windows Authentication
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("KeyUserOnly", policy =>
//        policy.RequireAssertion(ctx =>
//            ctx.User.IsInRole(@"ASD\GG_ADMIN_ASD") ||
builder.Services.AddScoped<IDatabaseMaintenanceService, DatabaseMaintenanceService>();
//            ctx.User.IsInRole(@"ASD\GG_ADMIN_SP"))); // Admins dürfen alles

//    options.AddPolicy("AdminOnly", policy =>
//        policy.RequireAssertion(ctx =>
//            ctx.User.IsInRole(@"ASD\GG_ADMIN_SP")));
//});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("KeyUserOnly", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(@"SP\GRP_KeyUser_AdminAssistant") ||
            ctx.User.IsInRole(@"SP\GRP_Admin_AdminAssistant"))); // Admins dürfen alles

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(@"SP\GRP_Admin_AdminAssistant")));
});

// EF Core SQLite
builder.Services.AddDbContext<AdminAssistantDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Eigene Services
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IVpnInventoryService, VpnInventoryService>();
builder.Services.AddScoped<IAdService, AdService>();
builder.Services.AddScoped<IOuAccessService, OuAccessService>();
builder.Services.AddScoped<IDhcpService, DhcpService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    //var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    //var cs = config.GetConnectionString("DefaultConnection") ?? "<null>";
    //System.IO.File.AppendAllText("cs-log.txt", $"{DateTime.UtcNow:u} CS: {cs}{Environment.NewLine}");

    var db = scope.ServiceProvider.GetRequiredService<AdminAssistantDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<AdminAssistant.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
