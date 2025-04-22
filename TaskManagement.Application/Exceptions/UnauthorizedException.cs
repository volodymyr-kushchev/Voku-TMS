namespace TaskManagement.Application.Exceptions;

public class UnauthorizedException(string message) : Exception(message)
{
} 