namespace books_app.api.Domain.Entities;

public class Illustrator
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public ICollection<Book> Books { get; set; } = new List<Book>();
    
    public string FullName => $"{FirstName} {LastName}";
}