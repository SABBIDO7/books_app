using books_app.api.Application.Dtos;
using books_app.api.Application.enums;
using books_app.api.Application.Validators.BookValidators;
using books_app.api.Domain.Entities;
using books_app.api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace books_app.api.Tests;

public class BookValidatorTests
{
private BookDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new BookDbContext(options);
        
        // Seed test data
        context.Authors.AddRange(
            new Author { Id = 1, FirstName = "Stephen", LastName = "King" },
            new Author { Id = 2, FirstName = "Marguerite", LastName = "Duras" }
        );
        
        context.Illustrators.AddRange(
            new Illustrator { Id = 1, FirstName = "Norman", LastName = "Rockwell" },
            new Illustrator { Id = 2, FirstName = "Gustave", LastName = "Doré" }
        );
        
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task ValidateAsync_TitleIsMissing_ReturnsError()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "",
            PublicationYear : 2020,
            IllustratorId : 1,
            ISBN : "9781234567890",
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Action }
        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Title is mandatory"));
    }

    [Fact]
    public async Task ValidateAsync_IllustratorIsMissing_ReturnsError()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Test Book",
            PublicationYear : 2020,
            IllustratorId : 0,
            ISBN : "9781234567890",
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Action }
        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Illustrator is mandatory. Invalid Id"));
    }

    [Fact]
    public async Task ValidateAsync_PublicationYearInFuture_ReturnsError()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Test Book",
            PublicationYear : DateTime.Now.Year + 1,
            IllustratorId : 1,
            ISBN : "9781234567890",
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Action }
        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("cannot be in the future"));
    }

    [Fact]
    public async Task ValidateAsync_PublicationYearBefore1450_ReturnsError()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Ancient Book",
            PublicationYear : 1449,
            IllustratorId : 1,
            ISBN : null,
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Action }
        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("cannot have been written before 1450"));
    }

    [Fact]
    public async Task ValidateAsync_ISBNMissingForBookAfter1970_ReturnsError()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Modern Book",
            PublicationYear : 2000,
            IllustratorId : 1,
            ISBN : null,
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Action }
        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("ISBN is required"));
    }

    [Fact]
    public async Task ValidateAsync_ISBNExistingForBookBefore1970_ReturnsError()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Oldest Book",
            PublicationYear : 1655,
            IllustratorId : 1,
            ISBN : "1234567890123",
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Action }
        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("ISBN must not be provided for books published before 1970"));
    }

    [Fact]
    public async Task ValidateAsync_ISBNNotExactly13Digits_ReturnsError()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Test Book",
            PublicationYear : 2000,
            IllustratorId : 1,
            ISBN : "12345",
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Action }
        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("ISBN must be exactly 13 digits"));
    }

    [Fact]
    public async Task ValidateAsync_ISBNContainsNonDigits_ReturnsError()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Test Book",
            PublicationYear : 2000,
            IllustratorId : 1,
            ISBN : "978ABC1234567",
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Action }
        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("must contain only digits"));
    }

    [Fact]
    public async Task ValidateAsync_BookBefore1970WithoutISBN_IsValid()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Old Book",
            PublicationYear : 1960,
            IllustratorId : 1,
            ISBN : null,
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Horror }
        );

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_DuplicateISBN_ReturnsError()
    {
        var context = CreateInMemoryContext();
        context.Books.Add(new Book
        {
            Id = 1,
            Title = "Existing Book",
            PublicationYear = 2020,
            ISBN = "9781234567890",
            IllustratorId = 1
        });
        context.SaveChanges();

        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "New Book",
            PublicationYear : 2021,
            IllustratorId : 1,
            ISBN : "9781234567890",
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.ScienceFiction }
        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("already exists"));
    }

    [Fact]
    public async Task ValidateAsync_DuplicateTitleYearAuthor_ReturnsError()
    {
        var context = CreateInMemoryContext();
        
        var existingBook = new Book
        {
            Id = 1,
            Title = "The Shining",
            PublicationYear = 1977,
            ISBN = "9781234567890",
            IllustratorId = 1
        };
        context.Books.Add(existingBook);
        
        context.BookAuthors.Add(new BookAuthor
        {
            BookId = 1,
            AuthorId = 1
        });
        
        context.SaveChanges();

        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title:"The Shining",
            PublicationYear : 1977,
            IllustratorId : 1,
            ISBN : "9780987654321",
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.ScienceFiction, Genre.Comedy }

        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("same title, publication year, and author"));
    }

    [Fact]
    public async Task ValidateAsync_ValidBook_ReturnsSuccess()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Valid Book",
            PublicationYear : 2020,
            IllustratorId : 1,
            ISBN : "9781234567890",
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Horror, Genre.Drama }
        );

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_NonExistentAuthor_ReturnsError()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Test Book",
            PublicationYear : 2020,
            IllustratorId : 1,
            ISBN : "9781234567890",
            AuthorIds : new List<int> { 999 },
            Genres : new List<Genre> { Genre.Drama }

        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("author IDs do not exist"));
    }

    [Fact]
    public async Task ValidateAsync_NonExistentIllustrator_ReturnsError()
    {
        var context = CreateInMemoryContext();
        var validator = new BookValidator(context);
        var request = new CreateBookDto
        (
            Title : "Test Book",
            PublicationYear : 2020,
            IllustratorId : 999,
            ISBN : "9781234567890",
            AuthorIds : new List<int> { 1 },
            Genres : new List<Genre> { Genre.Action }

        );

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Illustrator with ID 999 does not exist"));
    }
    // TODO: Test duplicated AuthorId within the same book on book's creation
    }