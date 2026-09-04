using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;


var builder = WebApplication.CreateBuilder(args);


// =====================================================
// KẾT NỐI DATABASE
// =====================================================

// Local:
// lấy từ appsettings.Development.json
//
// Render:
// lấy từ biến môi trường
// ConnectionStrings__DefaultConnection

var connectionString =
    builder.Configuration
        .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Không tìm thấy ConnectionStrings:DefaultConnection."
    );
}


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);


// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();


// =====================================================
// ĐĂNG NHẬP COOKIE
// =====================================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        options.LoginPath =
            "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";
    });


builder.Services.AddAuthorization();


// =====================================================
// FORWARDED HEADERS
// Dùng khi chạy sau proxy của Render
// =====================================================

builder.Services.Configure<ForwardedHeadersOptions>(
    options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto;

        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    }
);


var app = builder.Build();


// =====================================================
// HTTP PIPELINE
// =====================================================

app.UseForwardedHeaders();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error"
    );

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();


app.UseAuthentication();

app.UseAuthorization();


// =====================================================
// FILE TĨNH
// =====================================================

app.MapStaticAssets();


// =====================================================
// ROUTE
// =====================================================

app.MapControllerRoute(
        name: "default",
        pattern:
            "{controller=Home}/{action=Index}/{id?}"
    )
    .WithStaticAssets();


app.Run();