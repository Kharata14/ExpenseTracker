using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Infrastructure.Database.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount)
      .HasColumnType("decimal(18, 2)")
      .IsRequired();

        builder.Property(e => e.Description)
      .HasMaxLength(500);

        builder.Property(e => e.ExpenseDate)
      .IsRequired();

        builder.HasOne(e => e.User)
      .WithMany(u => u.Expenses)
      .HasForeignKey(e => e.UserId)
      .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Category)
      .WithMany(c => c.Expenses)
      .HasForeignKey(e => e.CategoryId)
      .OnDelete(DeleteBehavior.Restrict);
    }
}