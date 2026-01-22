using books_app.api.Application.enums;

namespace books_app.api.Domain.Entities;

public class BookGenre
{
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    
    public Genre Genre { get; set; }
}