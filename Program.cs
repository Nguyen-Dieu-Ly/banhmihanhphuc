using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;
using banhmihanhphuc.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var staffUser = await db.Users
        .FirstOrDefaultAsync(u => u.Username == "staff");

    if (staffUser == null)
    {
        staffUser = new User
        {
            Username = "staff",
            FullName = "Nhân viên bán hàng",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        db.Users.Add(staffUser);
        await db.SaveChangesAsync();
    }

    var staffRole = await db.Roles
        .FirstOrDefaultAsync(r => r.Name == "Staff");

    if (staffRole != null)
    {
        bool alreadyAssigned = await db.UserRoles
            .AnyAsync(ur =>
                ur.UserId == staffUser.Id &&
                ur.RoleId == staffRole.Id
            );

        if (!alreadyAssigned)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = staffUser.Id,
                RoleId = staffRole.Id
            });

            await db.SaveChangesAsync();
        }
    }
}
app.Run();
