namespace books_app.api.Domain.Entities;

public class Author
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    
    public string FullName => $"{FirstName} {LastName}";
}