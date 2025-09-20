using ExpenseTracker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ExpenseTracker.Models;
using ExpenseTracker;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddAntiforgery(o => o.HeaderName = "X-CSRF-TOKEN");

builder.Services.AddAuthorization(Options =>
{
    //This enables the use of [Authorize(Policy = "<role>")] anywhere.
    Options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin")); 
    Options.AddPolicy("RequireStaff", p => p.RequireRole("Staff"));
    Options.AddPolicy("CanViewPeople", p => p.RequireAuthenticatedUser());
});

//Add EF Core DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Add Identity services
//Change RequireConfirmedAccount to true once ready to deply; this would require implementing IEmailSendor
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() //Add this line to enable role management; has to be added before the next line
.AddEntityFrameworkStores<ApplicationDbContext>(); //This line is needed to link Identity to the ApplicationDbContext


builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

//Register EventServices
builder.Services.AddScoped<ExpenseTracker.Services.EventService>();




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

app.UseHttpsRedirection();
app.UseStaticFiles();




app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();


// Expose a CSRF token endpoint for the SPA (must be authenticated)
app.MapGet("/api/antiforgery/token", (Microsoft.AspNetCore.Antiforgery.IAntiforgery af, HttpContext ctx) =>
{
    var tokens = af.GetAndStoreTokens(ctx);
    return Results.Ok(new { token = tokens.RequestToken });
}).RequireAuthorization();

// Map API controllers
app.MapControllers();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//.WithStaticAssets();



app.MapRazorPages();



// Seed Identity data: roles + first admin user
//Make sure to create an AdminSeed section in appsettings.json with Email and Password values
await IdentitySeed.EnsureSeededAsync(app.Services, app.Configuration);

app.Run();
