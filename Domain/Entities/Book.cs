namespace books_app.api.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public string? ISBN { get; set; } = string.Empty;
    
    // Navigation properties
    public int IllustratorId { get; set; }
    public Illustrator Illustrator { get; set; } = null!;
    
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
}
