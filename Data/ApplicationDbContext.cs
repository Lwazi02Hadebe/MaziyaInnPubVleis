using Microsoft.EntityFrameworkCore;
using MaziyaInnPubVleis.Models;

namespace MaziyaInnPubVleis.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventBooking> EventBookings { get; set; }
        public DbSet<EventStock> EventStocks { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<GoodsReceivedVoucher> GoodsReceivedVouchers { get; set; }
        public DbSet<GRVItem> GRVItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<EmployeeSchedule> EmployeeSchedules { get; set; }
        public DbSet<EmployeeAttendance> EmployeeAttendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(15);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.RegistrationDate).HasDefaultValueSql("datetime('now')");
            });

            // Configure CustomerAccount entity - FIXED
            modelBuilder.Entity<CustomerAccount>(entity =>
            {
                entity.HasKey(e => e.CustomerAccountId);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(15);
                entity.Property(e => e.RegistrationDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.LastLogin).HasDefaultValueSql("datetime('now')");

                // Make CustomerId optional to avoid circular dependency issues
                entity.Property(e => e.CustomerId).IsRequired(false);
            });

            // Configure Inventory entity - FIXED
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StockLevel).HasDefaultValue(0);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IsSixPack).HasDefaultValue(false);
                entity.Property(e => e.SixPackQuantity).HasDefaultValue(6);
                entity.Property(e => e.MinimumStockLevel).HasDefaultValue(10);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Make SupplierId truly optional - FIXED
                entity.Property(e => e.SupplierId).IsRequired(false);
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VATAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.GrossProfit).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasDefaultValue(OrderStatus.Pending);
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.OrderItemId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            });

            // Configure Event entity
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TicketPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasDefaultValue(EventStatus.Scheduled);
                entity.Property(e => e.CurrentAttendees).HasDefaultValue(0);
            });

            // Configure EventBooking entity
            modelBuilder.Entity<EventBooking>(entity =>
            {
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.BookingDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasDefaultValue(BookingStatus.Confirmed);
            });

            // Configure Cart entity
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.CartId);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("datetime('now')");
            });

            // Configure CartItem entity
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.CartItemId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Quantity).HasDefaultValue(1);
                entity.Property(e => e.AddedDate).HasDefaultValueSql("datetime('now')");
            });

            // Configure EmployeeSchedule entity
            modelBuilder.Entity<EmployeeSchedule>(entity =>
            {
                entity.HasKey(e => e.ScheduleId);
                entity.Property(e => e.ShiftType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(200);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
            });

            // Configure EmployeeAttendance entity
            modelBuilder.Entity<EmployeeAttendance>(entity =>
            {
                entity.HasKey(e => e.AttendanceId);
                entity.Property(e => e.Status).HasMaxLength(50);
            });

            // Configure relationships and constraints - FIXED FOREIGN KEY CONSTRAINTS
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.ProcessedBy)
                .WithMany()
                .HasForeignKey(o => o.ProcessedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Inventory)
                .WithMany(i => i.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventBooking>()
                .HasOne(eb => eb.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(eb => eb.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventBooking>()
                .HasOne(eb => eb.Customer)
                .WithMany(c => c.EventBookings)
                .HasForeignKey(eb => eb.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.CreatedBy)
                .WithMany()
                .HasForeignKey(po => po.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FIXED: CustomerAccount-Customer relationship
            modelBuilder.Entity<CustomerAccount>()
                .HasOne(ca => ca.Customer)
                .WithMany()
                .HasForeignKey(ca => ca.CustomerId)
                .OnDelete(DeleteBehavior.SetNull); // Changed from Restrict to SetNull

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.CustomerAccount)
                .WithMany(ca => ca.Carts)
                .HasForeignKey(c => c.CustomerAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Inventory)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FIXED: Inventory-Supplier relationship
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Supplier)
                .WithMany(s => s.InventoryItems)
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.SetNull); // Changed from Restrict to SetNull

            // Employee Schedule relationships
            modelBuilder.Entity<EmployeeSchedule>()
                .HasOne(es => es.Employee)
                .WithMany()
                .HasForeignKey(es => es.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeSchedule>()
                .HasOne(es => es.CreatedBy)
                .WithMany()
                .HasForeignKey(es => es.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeAttendance>()
                .HasOne(ea => ea.Employee)
                .WithMany()
                .HasForeignKey(ea => ea.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    Email = "admin@maziyainn.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = UserRole.Admin,
                    CreatedDate = seedDate,
                    IsActive = true,
                    LastLogin = seedDate
                },
                new User
                {
                    UserId = 2,
                    Username = "manager",
                    Email = "manager@maziyainn.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    Role = UserRole.Manager,
                    CreatedDate = seedDate,
                    IsActive = true,
                    LastLogin = seedDate
                },
                new User
                {
                    UserId = 3,
                    Username = "cashier1",
                    Email = "cashier1@maziyainn.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("cashier123"),
                    Role = UserRole.Cashier,
                    CreatedDate = seedDate,
                    IsActive = true,
                    LastLogin = seedDate
                },
                new User
                {
                    UserId = 4,
                    Username = "eventcoordinator",
                    Email = "events@maziyainn.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("events123"),
                    Role = UserRole.EventCoordinator,
                    CreatedDate = seedDate,
                    IsActive = true,
                    LastLogin = seedDate
                }
            );

            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    CustomerId = 1,
                    Name = "John Doe",
                    Email = "customer@example.com",
                    Phone = "0115551234",
                    Address = "123 Main Street",
                    RegistrationDate = seedDate
                }
            );

            modelBuilder.Entity<CustomerAccount>().HasData(
                new CustomerAccount
                {
                    CustomerAccountId = 1,
                    Email = "customer@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("customer123"),
                    FirstName = "John",
                    LastName = "Doe",
                    Phone = "0115551234",
                    CustomerId = 1,
                    RegistrationDate = seedDate,
                    IsActive = true,
                    LastLogin = seedDate
                }
            );

            modelBuilder.Entity<Supplier>().HasData(
                new Supplier
                {
                    SupplierId = 1,
                    Name = "Beverage Distributors Ltd",
                    ContactPerson = "John Smith",
                    Phone = "0115551234",
                    Email = "john@beveragedist.co.za"
                },
                new Supplier
                {
                    SupplierId = 2,
                    Name = "Fresh Meat Suppliers",
                    ContactPerson = "Sarah Johnson",
                    Phone = "0115555678",
                    Email = "sarah@freshmeat.co.za"
                }
            );

            modelBuilder.Entity<Inventory>().HasData(
                new Inventory
                {
                    ProductId = 1,
                    ProductName = "Castle Lager Beer",
                    Description = "330ml Bottle",
                    StockLevel = 100,
                    UnitPrice = 25.00m,
                    CostPrice = 15.00m,
                    IsSixPack = true,
                    SixPackQuantity = 6,
                    MinimumStockLevel = 10,
                    SupplierId = 1,
                    IsActive = true
                },
                new Inventory
                {
                    ProductId = 2,
                    ProductName = "T-Bone Steak",
                    Description = "500g Premium Cut",
                    StockLevel = 50,
                    UnitPrice = 120.00m,
                    CostPrice = 80.00m,
                    IsSixPack = false,
                    SixPackQuantity = 6,
                    MinimumStockLevel = 5,
                    SupplierId = 2,
                    IsActive = true
                },
                new Inventory
                {
                    ProductId = 3,
                    ProductName = "Chicken Wings",
                    Description = "1kg Spicy Wings",
                    StockLevel = 30,
                    UnitPrice = 85.00m,
                    CostPrice = 55.00m,
                    IsSixPack = false,
                    SixPackQuantity = 6,
                    MinimumStockLevel = 8,
                    SupplierId = 2,
                    IsActive = true
                }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    EventId = 1,
                    EventName = "Live Music Friday",
                    Description = "Enjoy live music performances every Friday night",
                    EventDate = DateTime.Today.AddDays(7).AddHours(19),
                    EndDate = DateTime.Today.AddDays(7).AddHours(23),
                    Status = EventStatus.Scheduled,
                    MaxAttendees = 100,
                    CurrentAttendees = 0,
                    TicketPrice = 50.00m,
                    CreatedByUserId = 2
                }
            );

            // Seed employee schedules
            modelBuilder.Entity<EmployeeSchedule>().HasData(
                new EmployeeSchedule
                {
                    ScheduleId = 1,
                    EmployeeId = 3,
                    ShiftDate = DateTime.Today,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(16, 0, 0),
                    ShiftType = "Morning",
                    Notes = "Regular shift",
                    CreatedByUserId = 2,
                    CreatedDate = seedDate
                },
                new EmployeeSchedule
                {
                    ScheduleId = 2,
                    EmployeeId = 4,
                    ShiftDate = DateTime.Today,
                    StartTime = new TimeSpan(12, 0, 0),
                    EndTime = new TimeSpan(20, 0, 0),
                    ShiftType = "Evening",
                    Notes = "Event coordination",
                    CreatedByUserId = 2,
                    CreatedDate = seedDate
                }
            );
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto-set timestamps for entities
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IHasTimestamps &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    ((IHasTimestamps)entityEntry.Entity).CreatedDate = DateTime.Now;
                }
                ((IHasTimestamps)entityEntry.Entity).LastUpdated = DateTime.Now;
            }

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                // Log the inner exception for debugging
                var innerException = ex.InnerException?.Message ?? ex.Message;
                throw new DbUpdateException($"Error saving changes: {innerException}", ex);
            }
        }
    }

    // Interface for entities that have timestamp fields
    public interface IHasTimestamps
    {
        DateTime CreatedDate { get; set; }
        DateTime LastUpdated { get; set; }
    }
}