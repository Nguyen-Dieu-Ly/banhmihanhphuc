using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Models;

namespace banhmihanhphuc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // =========================================
        // KHAI BÁO 11 BẢNG TRONG POSTGRESQL
        // =========================================

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<RestaurantTable> RestaurantTables { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =========================================
            // TỰ ĐỘNG ĐỔI TÊN CỘT C# SANG SNAKE_CASE
            // Ví dụ:
            // CategoryId  -> category_id
            // CreatedAt   -> created_at
            // TotalAmount -> total_amount
            // =========================================

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    var name = property.Name;

                    var snakeCase = string.Concat(
                        name.Select((x, i) =>
                            i > 0 && char.IsUpper(x)
                                ? "_" + x
                                : x.ToString()
                        )
                    ).ToLower();

                    property.SetColumnName(snakeCase);
                }
            }


            // =========================================
            // ĐỒNG BỘ KIỂU NGÀY GIỜ VỚI POSTGRESQL
            // Database đang dùng TIMESTAMP không có múi giờ
            // nên ép DateTime về timestamp without time zone
            // =========================================

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) ||
                        property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType(
                            "timestamp without time zone"
                        );
                    }
                }
            }


            // =========================================
            // TÊN BẢNG
            // =========================================

            modelBuilder.Entity<User>()
                .ToTable("users");

            modelBuilder.Entity<Role>()
                .ToTable("roles");

            modelBuilder.Entity<Permission>()
                .ToTable("permissions");

            modelBuilder.Entity<UserRole>()
                .ToTable("user_roles");

            modelBuilder.Entity<RolePermission>()
                .ToTable("role_permissions");

            modelBuilder.Entity<Category>()
                .ToTable("categories");

            modelBuilder.Entity<Food>()
                .ToTable("foods");

            modelBuilder.Entity<RestaurantTable>()
                .ToTable("restaurant_tables");

            modelBuilder.Entity<Order>()
                .ToTable("orders");

            modelBuilder.Entity<OrderDetail>()
                .ToTable("order_details");

            modelBuilder.Entity<Payment>()
                .ToTable("payments");


            // =========================================
            // USER - ROLE
            // Một tài khoản có thể có nhiều vai trò
            // =========================================

            modelBuilder.Entity<UserRole>()
                .HasKey(x => new
                {
                    x.UserId,
                    x.RoleId
                });

            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId);


            // =========================================
            // ROLE - PERMISSION
            // Một vai trò có thể có nhiều quyền
            // =========================================

            modelBuilder.Entity<RolePermission>()
                .HasKey(x => new
                {
                    x.RoleId,
                    x.PermissionId
                });

            modelBuilder.Entity<RolePermission>()
                .HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId);


            // =========================================
            // CATEGORY - FOOD
            // Một danh mục có nhiều món ăn
            // =========================================

            modelBuilder.Entity<Food>()
                .HasOne(x => x.Category)
                .WithMany(x => x.Foods)
                .HasForeignKey(x => x.CategoryId);


            // =========================================
            // TABLE - ORDER
            // Một bàn có thể có nhiều hóa đơn theo thời gian
            // =========================================

            // Một bàn có thể có nhiều hóa đơn theo thời gian
// Khi xóa bàn, hóa đơn cũ vẫn được giữ lại
// và table_id của hóa đơn sẽ chuyển thành NULL
modelBuilder.Entity<Order>()
    .HasOne(x => x.Table)
    .WithMany(x => x.Orders)
    .HasForeignKey(x => x.TableId)
    .OnDelete(DeleteBehavior.SetNull);


            // =========================================
            // USER - ORDER
            // Một tài khoản có thể tạo nhiều hóa đơn
            // =========================================

            modelBuilder.Entity<Order>()
                .HasOne(x => x.User)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId);


            // =========================================
            // ORDER - ORDER DETAIL
            // Một hóa đơn có nhiều món
            // =========================================

            modelBuilder.Entity<OrderDetail>()
                .HasOne(x => x.Order)
                .WithMany(x => x.OrderDetails)
                .HasForeignKey(x => x.OrderId);


            // =========================================
            // FOOD - ORDER DETAIL
            // Một món có thể xuất hiện trong nhiều hóa đơn
            // =========================================

            modelBuilder.Entity<OrderDetail>()
                .HasOne(x => x.Food)
                .WithMany(x => x.OrderDetails)
                .HasForeignKey(x => x.FoodId);


            // =========================================
            // ORDER - PAYMENT
            // Một hóa đơn chỉ có một thông tin thanh toán
            // =========================================

            modelBuilder.Entity<Payment>()
                .HasOne(x => x.Order)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.OrderId);
        }
    }
}