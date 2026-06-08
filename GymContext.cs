using GymFitnessSystem.Models;
using GymFitnessSystem.Models.Comanda_Programare;
using GymFitnessSystem.Models.Produs_Serviciu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymFitnessSystem.Data
{
    public class GymContext:DbContext
    {
        // Tabelele de bază (Polimorfice)
        public DbSet<OfertaBase> Oferte { get; set; }
        public DbSet<ActivitateBase> Activitati { get; set; }
        public DbSet<SetariSistem> SetariSistem { get; set; }

        // Tabelele specifice ( ajută la interogări rapide)
        public DbSet<Produs> Produse { get; set; }
        public DbSet<Serviciu> Servicii { get; set; }
        public DbSet<Comanda> Comenzi { get; set; }
        public DbSet<Programare> Programari { get; set; }

        // Tabelele simple
        public DbSet<Utilizator> Utilizatori { get; set; }
        public DbSet<Plata> Plati { get; set; }
        public DbSet<Notificare> Notificari { get; set; }
        public DbSet<Raport> Rapoarte { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=GymFitnessDB;Trusted_Connection=True;")
                              .UseLazyLoadingProxies(); // Pentru a permite încărcarea leneșă a datelor legate (ex: Comenzi din Utilizator)
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // FORȚĂM STRATEGIA TPT (Table Per Type)
            // Fără  EF Core nu va crea tabele separate pentru Produse/Servicii
            modelBuilder.Entity<OfertaBase>().ToTable("Oferte");
            modelBuilder.Entity<Produs>().ToTable("Produse");
            modelBuilder.Entity<Serviciu>().ToTable("Servicii");

            modelBuilder.Entity<ActivitateBase>().ToTable("Activitati");
            modelBuilder.Entity<Comanda>().ToTable("Comenzi");
            modelBuilder.Entity<Programare>().ToTable("Programari");

            // Alte configurări
            modelBuilder.Entity<Utilizator>().HasIndex(u => u.Username).IsUnique();
                
            
            modelBuilder.Entity<ActivitateBase>().ToTable("Activitati");
            modelBuilder.Entity<Comanda>().ToTable("Comenzi");
            modelBuilder.Entity<Programare>().ToTable("Programari");

            // CONFIGURARE RELAȚIE 1:1 (Cheie partajată)
            modelBuilder.Entity<Comanda>()
                .HasOne(c => c.Activitate)
                .WithOne() // Dacă nu ai o proprietate de navigare "Comanda" în ActivitateBase
                .HasForeignKey<Comanda>(c => c.Id);

            modelBuilder.Entity<Programare>()
                .HasOne(p => p.Activitate)
                .WithOne()
                .HasForeignKey<Programare>(p => p.Id);

        }
    }
}
