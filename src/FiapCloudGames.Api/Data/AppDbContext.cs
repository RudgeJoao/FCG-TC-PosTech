using FiapCloudGames.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Jogo> Jogos => Set<Jogo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(160).IsRequired();
            entity.Property(x => x.SenhaHash).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();

            entity.HasMany(x => x.Biblioteca)
                .WithMany()
                .UsingEntity("UsuarioJogos");
        });

        modelBuilder.Entity<Jogo>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Descricao).HasMaxLength(500);
            entity.Property(x => x.Preco).HasColumnType("decimal(10,2)");
        });
    }
}
