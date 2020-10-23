using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace QueueDataAccess.Models
{
    public class QueueDbContext : DbContext
    {
        public QueueDbContext(DbContextOptions<QueueDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Data Source=.\\SQLEXPRESS;initial catalog=QueueDb2;user id=Jomari;password=admin123");

            //optionsBuilder.UseSqlServer(
            //    "Data Source=192.168.1.132\\SQLEXPRESS,1433;initial catalog=QueueDb2;user id=Jomari;password=admin123");

            //optionsBuilder.UseSqlServer(
            //    "Data Source=192.168.1.100\\SQLEXPRESS,1433;initial catalog=QueueDb2;user id=Jomari;password=admin123");

            //optionsBuilder.UseSqlServer(
            //    "Data Source=192.168.2.132\\SQLEXPRESS,1433;initial catalog=QueueDb2;user id=Jomari;password=admin123");

            //optionsBuilder.UseSqlServer(
            //    "Data Source=.\\SQLEXPRESS;initial catalog=QueueDb2;user id=Jomari;password=admin123");
        }

        public DbSet<Counter> Counters { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CounterType> CounterTypes { get; set; }
        public DbSet<TransPool> TransPools { get; set; }
        public DbSet<TransControl> TransControls { get; set; }
        public DbSet<Device> Devices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Transaction
            modelBuilder.Entity<Transaction>()
                .HasOne(x => x.PrevTrans)
                .WithOne()
                .HasForeignKey<Transaction>(x => x.PrevId)
                .HasConstraintName("FK_Transaction_PrevId");

            modelBuilder.Entity<Transaction>()
                .HasOne(x => x.NextTrans)
                .WithOne()
                .HasForeignKey<Transaction>(x => x.NextId)
                .HasConstraintName("FK_Transaction_NextId");

            modelBuilder.Entity<Transaction>()
                .Property(x => x.RowVersion2)
                .HasConversion<byte[]>(
                    v => BitConverter.GetBytes(v), v => 
                        BitConverter.ToUInt64(v != null && v.Length > 0 ? v : new byte[]
                        {
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                        }))
                .HasColumnType("Timestamp")
                .IsRowVersion();

            // Counter
            modelBuilder.Entity<Counter>()
                .HasOne(x => x.CounterType)
                .WithMany(x => x.Counters)
                .HasForeignKey(x => x.CounterTypeId)
                .HasConstraintName("FK_Counter_CounterTypeId");

            modelBuilder.Entity<Counter>()
                .Property(x => x.RowVersion2)
                .HasConversion<byte[]>(
                    v => BitConverter.GetBytes(v), v =>
                        BitConverter.ToUInt64(v != null && v.Length > 0 ? v : new byte[]
                        {
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                        }))
                .HasColumnType("Timestamp")
                .IsRowVersion();
        }
    }
}
