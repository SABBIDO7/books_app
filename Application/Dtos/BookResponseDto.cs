namespace books_app.api.Application.Dtos;

public record class BookResponseDto(
    int Id ,string Title,
    int PublicationYear,
    List<string> Authors,
    string Illustrator,
    List<string> Genres,
    string? ISBN
);