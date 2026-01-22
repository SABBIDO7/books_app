
using books_app.api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace books_app.api.Infrastructure.Data;

public class BookDbContext(DbContextOptions<BookDbContext> options): DbContext(options)
{

    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Illustrator> Illustrators { get; set; }
    public DbSet<BookAuthor> BookAuthors { get; set; }
    public DbSet<BookGenre> BookGenres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Book entity
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(500);
            entity.Property(b => b.ISBN).IsRequired(false).HasMaxLength(13);
            
            entity.HasIndex(b => b.ISBN).IsUnique().HasFilter("[ISBN] IS NOT NULL");;
            
            entity.HasOne(b => b.Illustrator)
                  .WithMany(i => i.Books)
                  .HasForeignKey(b => b.IllustratorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Author entity
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.LastName).IsRequired().HasMaxLength(100);
        });

        // Configure Illustrator entity
        modelBuilder.Entity<Illustrator>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(i => i.LastName).IsRequired().HasMaxLength(100);
        });

        // Configure BookAuthor (many-to-many)
        modelBuilder.Entity<BookAuthor>(entity =>
        {
            entity.HasKey(ba => new { ba.BookId, ba.AuthorId });
            
            entity.HasOne(ba => ba.Book)
                  .WithMany(b => b.BookAuthors)
                  .HasForeignKey(ba => ba.BookId);
            
            entity.HasOne(ba => ba.Author)
                  .WithMany(a => a.BookAuthors)
                  .HasForeignKey(ba => ba.AuthorId);
        });

        // Configure BookGenre
        modelBuilder.Entity<BookGenre>(entity =>
        {
            entity.HasKey(bg => new { bg.BookId, bg.Genre });
            
            entity.HasOne(bg => bg.Book)
                  .WithMany(b => b.BookGenres)
                  .HasForeignKey(bg => bg.BookId);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Authors
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FirstName = "Stephen", LastName = "King" },
            new Author { Id = 2, FirstName = "Marguerite", LastName = "Duras" },
            new Author { Id = 3, FirstName = "Isaac", LastName = "Asimov" }
        );

        // Seed Illustrators
        modelBuilder.Entity<Illustrator>().HasData(
            new Illustrator { Id = 1, FirstName = "Norman", LastName = "Rockwell" },
            new Illustrator { Id = 2, FirstName = "Gustave", LastName = "Doré" },
            new Illustrator { Id = 3, FirstName = "Beya", LastName = "Rebaï" }
        );
    }
}