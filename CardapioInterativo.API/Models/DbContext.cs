using Microsoft.EntityFrameworkCore;

namespace CardapioInterativo.API.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cardapio> Cardapios { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cidade> Cidades { get; set; }

    public virtual DbSet<ClienteCurtida> ClienteCurtidas { get; set; }

    public virtual DbSet<Perfi> Perfis { get; set; }

    public virtual DbSet<Prato> Pratos { get; set; }

    public virtual DbSet<Restaurante> Restaurantes { get; set; }

    public virtual DbSet<TiposRestaurate> TiposRestaurates { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=10.106.130.93,1433;Database=dbCardapio;User=sa;Password=Senai@123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cardapio>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PratoId).HasColumnName("PratoID");
            entity.Property(e => e.RestauranteId).HasColumnName("RestauranteID");

            entity.HasOne(d => d.Prato).WithMany(p => p.Cardapios)
                .HasForeignKey(d => d.PratoId)
                .HasConstraintName("FK_Cardapios_Pratos");

            entity.HasOne(d => d.Restaurante).WithMany(p => p.Cardapios)
                .HasForeignKey(d => d.RestauranteId)
                .HasConstraintName("FK_Cardapios_Restaurantes");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Cidade>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ClienteCurtida>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdPrato).HasColumnName("idPrato");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ClienteCurtida)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK_ClienteCurtidas_Usuario");

            entity.HasOne(d => d.IdPratoNavigation).WithMany(p => p.ClienteCurtida)
                .HasForeignKey(d => d.IdPrato)
                .HasConstraintName("FK_ClienteCurtidas_Pratos");
        });

        modelBuilder.Entity<Perfi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TiposUsuario");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Prato>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.Descricao).HasColumnType("text");
            entity.Property(e => e.Foto).IsUnicode(false);
            entity.Property(e => e.Ingredientes).HasColumnType("text");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Categoria).WithMany(p => p.Pratos)
                .HasForeignKey(d => d.CategoriaId)
                .HasConstraintName("FK_Pratos_Categorias");
        });

        modelBuilder.Entity<Restaurante>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CidadeId).HasColumnName("CidadeID");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Descricao).HasColumnType("text");
            entity.Property(e => e.DonoId).HasColumnName("DonoID");
            entity.Property(e => e.Endereco)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Foto)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TipoId).HasColumnName("TipoID");

            entity.HasOne(d => d.Cidade).WithMany(p => p.Restaurantes)
                .HasForeignKey(d => d.CidadeId)
                .HasConstraintName("FK_Restaurantes_Cidades");

            entity.HasOne(d => d.Dono).WithMany(p => p.Restaurantes)
                .HasForeignKey(d => d.DonoId)
                .HasConstraintName("FK_Restaurantes_Users");

            entity.HasOne(d => d.Tipo).WithMany(p => p.Restaurantes)
                .HasForeignKey(d => d.TipoId)
                .HasConstraintName("FK_Restaurantes_TiposRestaurate");
        });

        modelBuilder.Entity<TiposRestaurate>(entity =>
        {
            entity.ToTable("TiposRestaurate");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Icone)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Users");

            entity.ToTable("Usuario");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Cpf)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CPF");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PerfilId).HasColumnName("PerfilID");
            entity.Property(e => e.Senha)
                .HasMaxLength(80)
                .IsUnicode(false);

            entity.HasOne(d => d.Perfil).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.PerfilId)
                .HasConstraintName("FK_Users_TiposUsuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
