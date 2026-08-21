namespace StockFlow.Api.Domain.Exceptions;

public sealed class ResourceNotFoundException(string message) : Exception(message);
