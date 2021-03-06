// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Service.BonusRewards.Postgres;

#nullable disable

namespace Service.BonusRewards.Postgres.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211118115344_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("bonusrewards")
                .HasAnnotation("ProductVersion", "6.0.0")
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

                    b.Property<string>("FeeShareGroup")
                        .HasColumnType("text");

                    b.Property<decimal>("Percentage")
                        .HasColumnType("numeric");

                    b.Property<string>("ReferrerClientId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<int>("RewardType")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

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
