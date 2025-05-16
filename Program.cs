using JwtAuthDemo.Models;
using JwtAuthDemo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddNpgsql<Appdatacontext>("Host=localhost;Port=5432;Database=Beth;Username=postgres;Password=12345678");

builder.Services.AddScoped<JwtService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourNewSuperSecretKey0123456789!"))
        };
    });

builder.Services.AddAuthorization();

// ✅ Configure session properly
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Default session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Ensure Session is Available
app.UseSession();

// ✅ Ensure this Middleware Runs Before Authentication
app.Use(async (context, next) =>
{
    var sessionTimeout = context.Session.GetString("SessionTimeout");

    if (!string.IsNullOrEmpty(sessionTimeout) && sessionTimeout == "7Days")
    {
        context.Session.SetString("CustomTimeout", "7Days");
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// <-- Changed default route here -->
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
