namespace TodoApi.Dtos;

public record TodoGetDto(int Id, string Title, bool IsCompleted);
public record TodoCreateDto(string Title);
public record TodoUpdateDto(string Title, bool IsCompleted);
