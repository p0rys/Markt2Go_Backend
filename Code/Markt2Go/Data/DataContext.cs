using System;
using Markt2Go.Model;
using Microsoft.EntityFrameworkCore;

namespace Markt2Go.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Market> Markets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<MarketSeller> MarketSellers { get; set; }
        public DbSet<Item> Items { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // utc conversations
            modelBuilder.Entity<Reservation>()
                .Property(x => x.Pickup)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Entity<Reservation>()
                .Property(x => x.CreatedAt)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<User>()
                .Property(x => x.CreatedAt)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            // default values
            modelBuilder.Entity<User>()
                .Property(x => x.IsValidated)
                .HasDefaultValue(false);

            modelBuilder.Entity<Reservation>()
                .Property(x => x.Packed)
                .HasDefaultValue(false);

            // SELLER to MARKET
            modelBuilder.Entity<MarketSeller>()
                 .HasIndex(x => new { x.SellerId, x.MarketId })
                 .IsUnique();

            modelBuilder.Entity<MarketSeller>()
                .HasOne(x => x.Seller)
                .WithMany(x => x.MarketSellers)
                .HasForeignKey(x => x.SellerId);
            modelBuilder.Entity<MarketSeller>()
                .HasOne(x => x.Market)
                .WithMany(x => x.MarketSellers)
                .HasForeignKey(x => x.MarketId);

            /*
            // SELLER
            modelBuilder.Entity<SELLER>()
                .HasMany(s => s.USERS)
                .WithOne()
                .HasForeignKey(u => u.SELLER_ID);
            
            // RESERVATION
            modelBuilder.Entity<RESERVATION>()
                .HasOne(r => r.SELLER)
                .WithMany(s => s.RESERVATIONS)
                .HasForeignKey(r => r.SELLER_ID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RESERVATION>()
                .HasOne(r => r.USER)
                .WithMany(u => u.RESERVATIONS)
                .HasForeignKey(r => r.USER_ID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RESERVATION>()
                .HasOne(r => r.MARKET)
                .WithMany()
                .HasForeignKey(u => u.SELLER_ID)
                .OnDelete(DeleteBehavior.Restrict);
            
            // ITEMS
            modelBuilder.Entity<ITEM>()
                .HasOne(i => i.RESERVATION)
                .WithMany(r => r.ITEMS)
                .HasForeignKey(i => i.RESERVATION_ID);

            */
        }

    }
}