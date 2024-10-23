﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;
using WebAPI.Data;

#nullable disable

namespace WebAPI.Migrations
{
    [DbContext(typeof(WebApiContext))]
    partial class WebApiContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebAPI.Data.Blog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<NpgsqlTsVector>("FullTextSearchVector")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("tsvector")
                        .HasAnnotation("Npgsql:TsVectorConfig", "english")
                        .HasAnnotation("Npgsql:TsVectorProperties", new[] { "Title", "Content" });

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<int>("OwnerId")
                        .HasColumnType("integer");

                    b.Property<List<string>>("Tags")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("FullTextSearchVector");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("FullTextSearchVector"), "GIN");

                    b.HasIndex("IsDeleted")
                        .HasFilter("\"IsDeleted\" = false");

                    b.HasIndex("OwnerId");

                    b.HasIndex("Title", "Content")
                        .HasAnnotation("Npgsql:TsVectorConfig", "english");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Title", "Content"), "GIN");

                    b.ToTable("Blogs");
                });

            modelBuilder.Entity("WebAPI.Data.Member", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateOnly>("DateOfBirth")
                        .HasColumnType("date");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<int>("Membership")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("PaymentTypes")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("IsDeleted")
                        .HasFilter("\"IsDeleted\" = false");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("WebAPI.Data.Blog", b =>
                {
                    b.HasOne("WebAPI.Data.Member", "Owner")
                        .WithMany("Blogs")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("WebAPI.Data.Member", b =>
                {
                    b.OwnsOne("WebAPI.Data.ContactDetails", "ContactDetails", b1 =>
                        {
                            b1.Property<int>("MemberId")
                                .HasColumnType("integer");

                            b1.Property<string>("Phone")
                                .HasColumnType("text");

                            b1.HasKey("MemberId");

                            b1.ToTable("Members");

                            b1.ToJson("ContactDetails");

                            b1.WithOwner()
                                .HasForeignKey("MemberId");

                            b1.OwnsOne("WebAPI.Data.Address", "Address", b2 =>
                                {
                                    b2.Property<int>("ContactDetailsMemberId")
                                        .HasColumnType("integer");

                                    b2.Property<string>("City")
                                        .HasColumnType("text");

                                    b2.Property<string>("Country")
                                        .HasColumnType("text");

                                    b2.Property<string>("Street")
                                        .HasColumnType("text");

                                    b2.HasKey("ContactDetailsMemberId");

                                    b2.ToTable("Members");

                                    b2.WithOwner()
                                        .HasForeignKey("ContactDetailsMemberId");
                                });

                            b1.Navigation("Address");
                        });

                    b.Navigation("ContactDetails");
                });

            modelBuilder.Entity("WebAPI.Data.Member", b =>
                {
                    b.Navigation("Blogs");
                });
#pragma warning restore 612, 618
        }
    }
}
