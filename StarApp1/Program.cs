
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Design;
using StarApp1.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<StarUserDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MYConnector")));

builder.Services.ConfigureApplicationCookie(config => config.LoginPath = "/SignIn") ;

//builder.Services.AddMvc();
builder.Services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();




builder.Services.AddDistributedMemoryCache();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "ip_c_sess";
    //options.Cookie.Expiration = TimeSpan.FromMinutes(15);
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    //options.Cookie.Expiration = TimeSpan.FromMinutes(15);
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    
    options.LoginPath = "/User/SignIn";
    //options.LogoutPath = "/auth/logout";
    //options.AccessDeniedPath = "/auth/login";
    //options.EventsType = typeof(SessionCookieAuthenticationEvents);
});
//builder.Services.Configure<IdentityOptions>(options =>
//{
//    // Default Lockout settings.
//    options.Password.RequireDigit = true;
//    options.Password.RequireLowercase = true;
//    options.Password.RequireNonAlphanumeric = true;
//    options.Password.RequireUppercase = true;
//    options.Password.RequiredLength = 8;
//    options.Password.RequiredUniqueChars = 1;
//});

//builder.Services
//    .AddAuthentication(options =>
//    {
//options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
//})
//    .AddCookie();
  ////  .AddGoogle(options =>
///    {
//        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//    });
    
builder.Services.AddRazorPages();





// Add services to the container.
//builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseSession();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
