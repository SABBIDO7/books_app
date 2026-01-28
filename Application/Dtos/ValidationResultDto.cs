namespace books_app.api.Application.Dtos;

public class ValidationResultDto
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = [];
}