﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Service.BonusRewards.Postgres;

#nullable disable

namespace Service.BonusRewards.Postgres.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("bonusrewards")
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Service.BonusRewards.Domain.Models.RewardEntity", b =>
                {
                    b.Property<string>("ClientId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("RewardId")
                        .HasColumnType("text");

                    b.Property<decimal>("AmountAbs")
                        .HasColumnType("numeric");

                    b.Property<decimal>("AmountRel")
                        .HasColumnType("numeric");

                    b.Property<string>("Asset")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("CampaignId")
                        .HasColumnType("text");

                    b.Property<string>("ClientWalletId")
                        .HasColumnType("text");

                    b.Property<string>("FeeShareGroup")
                        .HasColumnType("text");

                    b.Property<decimal>("IndexPrice")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Percentage")
                        .HasColumnType("numeric");

                    b.Property<string>("ReferralClientId")
                        .HasColumnType("text");

                    b.Property<string>("ReferrerClientId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("RewardType")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("ClientId", "RewardId");

                    b.HasIndex("CampaignId");

                    b.HasIndex("ClientId");

                    b.HasIndex("RewardId");

                    b.ToTable("rewards", "bonusrewards");
                });
#pragma warning restore 612, 618
        }
    }
}
