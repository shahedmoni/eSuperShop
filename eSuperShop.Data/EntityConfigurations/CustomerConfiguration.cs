﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eSuperShop.Data
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasOne(d => d.Registration)
                .WithMany(p => p.Customer)
                .HasForeignKey(d => d.RegistrationId)
                .HasConstraintName("FK_Customer_Registration");

        }
    }
}
