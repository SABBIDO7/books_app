using books_app.api.Application.enums;

namespace books_app.api.Application.Dtos;

public record class CreateBookDto(
    string Title,
    int PublicationYear,
    List<int> AuthorIds,
    int IllustratorId,
    List<Genre> Genres,
    string? ISBN
);