namespace StockFlow.Api.Domain.Exceptions;

public sealed class BusinessRuleException(string message) : Exception(message);
