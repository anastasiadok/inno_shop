namespace ProductService.Domain.Exceptions;

public class UserAccessException : Exception
{
    public UserAccessException() : base("User can only manage their own products.") { }
}
