using books_app.api.Application.Dtos;
using books_app.api.Application.Validators.BookValidators;
using books_app.api.Domain.Entities;
using books_app.api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace books_app.api.Application.Services;

public interface IBookService
{
    Task<(bool success, BookResponseDto? book, List<string> errors)> CreateBookAsync(CreateBookDto request);
    Task<List<BookResponseDto>> GetBooksAsync(int? authorId = null, string? sortBy = null);
}

public class BookService : IBookService
{
    private readonly BookDbContext _context;
    private readonly IBookValidator _validator;

    public BookService(BookDbContext context, IBookValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<(bool success, BookResponseDto? book, List<string> errors)> CreateBookAsync(CreateBookDto request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try 
        {
            // Validate the book
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return (false, null, validationResult.Errors!);
            }

            // Create the book entity
            var book = new Book
            {
                Title = request.Title,
                PublicationYear = request.PublicationYear,
                IllustratorId = request.IllustratorId,
                ISBN = string.IsNullOrWhiteSpace(request.ISBN) ? null : request.ISBN
            };

            // Add book to context
            _context.Books.Add(book);
            await _context.SaveChangesAsync(); // Save to get the book ID

            // Add authors
            if (request.AuthorIds != null && request.AuthorIds.Any())
            {
                foreach (var authorId in request.AuthorIds)
                {
                    var bookAuthor = new BookAuthor
                    {
                        BookId = book.Id,
                        AuthorId = authorId
                    };
                    _context.BookAuthors.Add(bookAuthor);
                }
            }

            // Add Book's genres
            if (request.Genres != null && request.Genres.Any())
            {
                foreach (var genre in request.Genres)
                {
                    var bookGenre = new BookGenre
                    {
                        BookId = book.Id,
                        Genre = genre
                    };
                    _context.BookGenres.Add(bookGenre);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Reload the book with all related data
            var createdBook = await _context.Books
                .Include(b => b.Illustrator)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                .FirstOrDefaultAsync(b => b.Id == book.Id);

            var response = MapToResponse(createdBook!);
            return (true, response, new List<string>());
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, null, ["One or more Insertion failed!"]);
        }
    }

    public async Task<List<BookResponseDto>> GetBooksAsync(int? authorId = null, string? sortBy = null)
    {
        var query = _context.Books
            .Include(b => b.Illustrator)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Include(b => b.BookGenres)
            .AsQueryable();

        // Filter by author if provided
        if (authorId.HasValue)
        {
            query = query.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId.Value));
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "title" => query.OrderBy(b => b.Title),
            "year" or "publicationyear" => query.OrderBy(b => b.PublicationYear),
            "genre" => query.OrderBy(b => b.BookGenres.Min(bg => bg.Genre.ToString())),
            _ => query.OrderBy(b => b.Id)
        };

        var books = await query.ToListAsync();
        return books.Select(MapToResponse).ToList();
    }

    private BookResponseDto MapToResponse(Book book)
    {
        return new BookResponseDto
        (
            Id : book.Id,
            Title : book.Title,
            PublicationYear : book.PublicationYear,
            Authors : book.BookAuthors
                .Select(ba => ba.Author.FullName)
                .OrderBy(name => name)
                .ToList(),
            Illustrator : book.Illustrator.FullName,
            Genres : book.BookGenres
                .Select(bg => bg.Genre.ToString())
                .OrderBy(g => g)
                .ToList(),
            ISBN : book.ISBN
        );
    }
}
