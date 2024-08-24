using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.DAL.Models;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5091, listenOptions =>
    {
        listenOptions.UseHttps(); // Use default development certificate
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 0; // Allow for the shortest possible password.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 0;
})
    .AddEntityFrameworkStores<AppDbContext>();


//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.Cookie.Name = ".AspNetCore.Identity.Application";
//    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
//    options.SlidingExpiration = true;
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensures cookies are sent over HTTPS
//    options.Cookie.SameSite = SameSiteMode.None; // Allows cross-site requests
//    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
//});

//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.Cookie.Name = ".AspNetCore.Identity.Application";
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
//        options.SlidingExpiration = true;
//        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensures cookies are sent over HTTPS
//        options.Cookie.SameSite = SameSiteMode.None; // Allows cross-site requests
//        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
//    });

// Add Razor Pages services
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();



//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie();

//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.Cookie.Name = ".AspNetCore.Identity.Application";
//    options.Cookie.Path = "/"; // Ensure this is set correctly
//    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Set cookie expiration
//    options.SlidingExpiration = true;
//    options.Cookie.HttpOnly = true;
//    options.Cookie.SameSite = SameSiteMode.None;
//    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
//});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    //options.LoginPath = "/account/login"; // Path to your login endpoint
    //options.AccessDeniedPath = "/account/access-denied"; // Path to your access denied endpoint
    options.Cookie.SameSite = SameSiteMode.None; // Needed for cross-site requests from Angular
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use Secure cookies in production

});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
     policyBuilder =>
     {
         policyBuilder.WithOrigins("https://127.0.0.1:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
     });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();

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

app.UseCors("AllowAngularApp");
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
