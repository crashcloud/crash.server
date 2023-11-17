﻿// <auto-generated />
using System;
using Crash.Server.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Crash.Server.Migrations
{
    [DbContext(typeof(CrashContext))]
    partial class CrashContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.9");

            modelBuilder.Entity("Crash.Server.Model.ImmutableChange", b =>
                {
                    b.Property<Guid>("UniqueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .IsRequired(true);

                    b.Property<int>("Action")
                        .HasColumnType("INTEGER")
                        .IsRequired(true);

                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT")
                        .IsRequired(true);

                    b.Property<string>("Owner")
                        .HasColumnType("TEXT")
                        .IsUnicode();

                    b.Property<string>("Payload")
                        .HasColumnType("TEXT")
                        .IsUnicode();

                    b.Property<DateTime>("Stamp")
                        .HasColumnType("TEXT")
                        .IsRequired(true);

                    b.Property<string>("Type")
                        .HasColumnType("TEXT")
                        .IsRequired(true);

                    b.HasKey("UniqueId");
                    
                    b.ToTable("Changes");
                });

            modelBuilder.Entity("Crash.Server.Model.MutableChange", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .IsRequired(true);

                    b.Property<int>("Action")
                        .HasColumnType("INTEGER")
                        .IsRequired(true);

                    b.Property<string>("Owner")
                        .HasColumnType("TEXT")
                        .IsRequired(true)
                        .IsUnicode();

                    b.Property<string>("Payload")
                        .HasColumnType("TEXT")
                        .IsRequired(true)
                        .IsUnicode();

                    b.Property<DateTime>("Stamp")
                        .HasColumnType("TEXT")
                        .IsRequired(true);

                    b.Property<string>("Type")
                        .HasColumnType("TEXT")
                        .IsRequired(true);

                    b.HasKey("Id");

                    b.ToTable("LatestChanges");
                });

            modelBuilder.Entity("Crash.Server.Model.User", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .IsRequired(true)
                        .IsUnicode();

                    b.Property<string>("Follows")
                        .HasColumnType("TEXT")
                        .IsUnicode();

                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.HasKey("Name");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
