using ForwardingLetterPortal.Interface.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.DataAccess
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<ForwardingLetterRequest> ForwardingLetterRequests => Set<ForwardingLetterRequest>();
        public DbSet<GeneratedLetter> GeneratedLetters => Set<GeneratedLetter>();
        public DbSet<DownloadLog> DownloadLogs => Set<DownloadLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Primary keys
            modelBuilder.Entity<BankAccount>()
                .HasKey(b => b.BankAccountId);

            modelBuilder.Entity<Customer>()
                .HasKey(c => c.CustomerId);

            modelBuilder.Entity<ForwardingLetterRequest>()
                .HasKey(r => r.RequestId);

            modelBuilder.Entity<GeneratedLetter>()
                .HasKey(g => g.LetterId);

            modelBuilder.Entity<DownloadLog>()
                .HasKey(d => d.LogId);

            // Relationships
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.BankAccount)
                .WithOne(b => b.Customer)
                .HasForeignKey<Customer>(c => c.BankAccountId);

            modelBuilder.Entity<ForwardingLetterRequest>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.Requests)
                .HasForeignKey(r => r.CustomerId);

            modelBuilder.Entity<GeneratedLetter>()
                .HasOne(g => g.Request)
                .WithOne(r => r.GeneratedLetter)
                .HasForeignKey<GeneratedLetter>(g => g.RequestId);
        }
    }
}
