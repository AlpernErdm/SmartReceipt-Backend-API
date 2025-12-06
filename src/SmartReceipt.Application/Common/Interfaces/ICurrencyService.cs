using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Application.Common.Interfaces;

public interface ICurrencyService
{
    Task<decimal> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency, CancellationToken cancellationToken = default);
    
    Task<Dictionary<Currency, decimal>> GetExchangeRatesAsync(CancellationToken cancellationToken = default);
    
    Task<decimal> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency, CancellationToken cancellationToken = default);
}

