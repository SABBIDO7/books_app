using books_app.api.Application.Dtos;
using books_app.api.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace books_app.api.controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
    private readonly IBookService _bookService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookService bookService, ILogger<BooksController> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    /// Creates a new book
    [HttpPost]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookDto request)
    {
        try
        {
            var (success, book, errors) = await _bookService.CreateBookAsync(request);

            if (!success)
            {
                return BadRequest(new 
                { 
                    type = "ValidationError",
                    title = "One or more validation errors occurred.",
                    status = 400,
                    errors = errors 
                });
            }

            return CreatedAtAction(nameof(GetBooks), new { id = book!.Id }, book);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book");
            return StatusCode(500, new 
            { 
                type = "ServerError",
                title = "An error occurred while creating the book.",
                status = 500 
            });
        }
    }

    /// Gets all books with optional filtering and sorting
    /// Optional: Sort by field (title, year, genre)
    [HttpGet]
    [ProducesResponseType(typeof(List<BookResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBooks([FromQuery] int? authorId = null, [FromQuery] string? sortBy = null)
    {
        try
        {
            var books = await _bookService.GetBooksAsync(authorId, sortBy);
            return Ok(books);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books");
            return StatusCode(500, new 
            { 
                type = "ServerError",
                title = "An error occurred while retrieving books.",
                status = 500 
            });
        }
    }
}
}
