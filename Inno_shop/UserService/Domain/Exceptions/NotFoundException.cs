namespace UserService.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity) : base($"{entity} not found.") { }
}
