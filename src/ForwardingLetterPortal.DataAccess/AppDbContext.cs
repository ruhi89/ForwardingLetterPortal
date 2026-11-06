using ForwardingLetterPortal.Interface.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForwardingLetterPortal.DataAccess
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Officer> Officers => Set<Officer>();
        public DbSet<OfficerLoginLog> OfficerLoginLogs => Set<OfficerLoginLog>();
        public DbSet<RetailService> RetailServices => Set<RetailService>();
        public DbSet<ServicePurpose> ServicePurposes => Set<ServicePurpose>();
        public DbSet<LetterTemplate> LetterTemplates => Set<LetterTemplate>();
        public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
        public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
        public DbSet<ForwardingLetter> ForwardingLetters => Set<ForwardingLetter>();
        public DbSet<LetterPrintLog> LetterPrintLogs => Set<LetterPrintLog>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // Primary keys
            b.Entity<Officer>().HasKey(x => x.OfficerId);
            b.Entity<OfficerLoginLog>().HasKey(x => x.LoginLogId);
            b.Entity<RetailService>().HasKey(x => x.ServiceId);
            b.Entity<ServicePurpose>().HasKey(x => x.PurposeId);
            b.Entity<LetterTemplate>().HasKey(x => x.TemplateId);
            b.Entity<BankAccount>().HasKey(x => x.BankAccountId);
            b.Entity<OtpVerification>().HasKey(x => x.VerificationId);
            b.Entity<ForwardingLetter>().HasKey(x => x.LetterId);
            b.Entity<LetterPrintLog>().HasKey(x => x.PrintLogId);

            // Relationships
            b.Entity<ServicePurpose>()
                .HasOne(p => p.Service).WithMany(s => s.Purposes).HasForeignKey(p => p.ServiceId);

            b.Entity<LetterTemplate>().HasOne(t => t.Service).WithMany().HasForeignKey(t => t.ServiceId);
            b.Entity<LetterTemplate>().HasOne(t => t.Purpose).WithMany().HasForeignKey(t => t.PurposeId);

            b.Entity<BankAccount>().HasOne(a => a.Service).WithMany().HasForeignKey(a => a.ServiceId);

            b.Entity<OtpVerification>().HasOne(v => v.BankAccount).WithMany().HasForeignKey(v => v.BankAccountId);

            b.Entity<ForwardingLetter>().HasOne(l => l.Officer).WithMany().HasForeignKey(l => l.OfficerId);
        }
    }
}
