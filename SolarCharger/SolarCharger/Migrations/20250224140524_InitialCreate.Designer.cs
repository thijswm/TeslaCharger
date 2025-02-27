﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SolarCharger.EF;

#nullable disable

namespace SolarCharger.Migrations
{
    [DbContext(typeof(ChargeContext))]
    [Migration("20250224140524_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.13");

            modelBuilder.Entity("SolarCharger.EF.ChargeCurrentChange", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ChargeSessionId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Current")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ChargeSessionId");

                    b.ToTable("ChargeCurrentChanges");
                });

            modelBuilder.Entity("SolarCharger.EF.ChargeSession", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int?>("BatteryLevelEnded")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BatteryLevelStarted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("End")
                        .HasColumnType("TEXT");

                    b.Property<double>("EnergyAdded")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Start")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ChargeSessions");
                });

            modelBuilder.Entity("SolarCharger.EF.Settings", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EnoughSolarWatt")
                        .HasColumnType("INTEGER");

                    b.Property<string>("HomeWizardAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("MaximumAmp")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MinimumAmp")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("MinimumChargeDuration")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("MinimumCurrentDuration")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("MinimumInitialChargeDuration")
                        .HasColumnType("TEXT");

                    b.Property<int>("NumberOfPhases")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("PollTime")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("SolarMovingAverage")
                        .HasColumnType("TEXT");

                    b.Property<string>("TeslaClientId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TeslaCommandsAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TeslaFleetAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TeslaRefreshToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Vin")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("SolarCharger.EF.ChargeCurrentChange", b =>
                {
                    b.HasOne("SolarCharger.EF.ChargeSession", "ChargeSession")
                        .WithMany()
                        .HasForeignKey("ChargeSessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ChargeSession");
                });
#pragma warning restore 612, 618
        }
    }
}
