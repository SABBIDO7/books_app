using books_app.api.Application.Dtos;
using books_app.api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace books_app.api.Application.Validators.BookValidators;

public interface IBookValidator
{
    Task<ValidationResultDto> ValidateAsync(CreateBookDto request);
}

public class BookValidator : IBookValidator
{
    private readonly BookDbContext _context;

    public BookValidator(BookDbContext context)
    {
        _context = context;
    }

    public async Task<ValidationResultDto> ValidateAsync(CreateBookDto request)
    {
        var result = new ValidationResultDto{IsValid = true };
        var errors = new List<string>();

        // Title is mandatory
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add("Title is mandatory.");
        }

        // Illustrator is mandatory
        if (request.IllustratorId <= 0)
        {
            errors.Add("Illustrator is mandatory.");
        }
        else
        {
            // Check if illustrator exists
            var illustratorExists = await _context.Illustrators
                .AnyAsync(i => i.Id == request.IllustratorId);
            
            if (!illustratorExists)
            {
                errors.Add($"Illustrator with ID {request.IllustratorId} does not exist.");
            }
        }

        // A book cannot have a publication year in the future
        var currentYear = DateTime.Now.Year;
        if (request.PublicationYear > currentYear)
        {
            errors.Add($"Publication year cannot be in the future (current year: {currentYear}).");
        }

        // A book cannot have been written before 1450
        if (request.PublicationYear < 1450)
        {
            errors.Add("Books cannot have been written before 1450.");
        }

        // ISBN validation
        var isbnValidation = ValidateISBN(request.ISBN, request.PublicationYear);
        if (!isbnValidation.isValid)
        {
            errors.Add(isbnValidation.error);
        }

        // Check if ISBN already exists (if provided)
        if (!string.IsNullOrWhiteSpace(request.ISBN))
        {
            var isbnExists = await _context.Books
                .AnyAsync(b => b.ISBN == request.ISBN);
            
            if (isbnExists)
            {
                errors.Add($"A book with ISBN {request.ISBN} already exists.");
            }
        }

        // Check for duplicate book (same title, year, and author)
        if (request.AuthorIds != null && request.AuthorIds.Any())
        {
            foreach (var authorId in request.AuthorIds)
            {
                var duplicateExists = await _context.Books
                    .Where(b => b.Title == request.Title && b.PublicationYear == request.PublicationYear)
                    .AnyAsync(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId));
                
                if (duplicateExists)
                {
                    var author = await _context.Authors.FindAsync(authorId);
                    errors.Add($"A book with the same title, publication year, and author ({author?.FullName}) already exists.");
                    break;
                }
            }
        }

        // Validate that all authors exist
        if (request.AuthorIds != null && request.AuthorIds.Any())
        {
            var existingAuthorIds = await _context.Authors
                .Where(a => request.AuthorIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();
            
            var missingAuthors = request.AuthorIds.Except(existingAuthorIds).ToList();
            if (missingAuthors.Any())
            {
                errors.Add($"The following author IDs do not exist: {string.Join(", ", missingAuthors)}");
            }
        }

        if (errors.Any())
        {
            result.IsValid = false;
            result.Errors = errors;
        }

        return result;
    }

    private (bool isValid, string error) ValidateISBN(string? isbn, int publicationYear)
    {
        // Books before 1970 don't need ISBN
        if (publicationYear < 1970)
        {
            // ISBN is optional for books before 1970
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return (true, string.Empty);
            }
        }
        else
        {
            // Books from 1970 onwards must have ISBN
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return (false, "ISBN is required for books published in 1970 or later.");
            }
        }

        // If ISBN is provided, it must be exactly 13 digits
        if (!string.IsNullOrWhiteSpace(isbn))
        {
            if (isbn.Length != 13)
            {
                return (false, "ISBN must be exactly 13 digits.");
            }

            if (!isbn.All(char.IsDigit))
            {
                return (false, "ISBN must contain only digits.");
            }
        }

        return (true, string.Empty);
    }
}
