﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrderService.Data;

#nullable disable

namespace OrderService.Migrations
{
    [DbContext(typeof(OrderDbContext))]
    partial class OrderDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("OrderService.Models.Order", b =>
                {
                    b.Property<int>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("OrderId"));

                    b.Property<string>("Customer")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("OrderNumber")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("OrderId");

                    b.ToTable("Orders");

                    b.HasData(
                        new
                        {
                            OrderId = 1,
                            Customer = "customer a",
                            OrderNumber = "132456"
                        },
                        new
                        {
                            OrderId = 2,
                            Customer = "customer b",
                            OrderNumber = "789123"
                        },
                        new
                        {
                            OrderId = 3,
                            Customer = "customer c",
                            OrderNumber = "456789"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
