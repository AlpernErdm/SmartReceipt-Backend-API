using Microsoft.Extensions.Logging;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Infrastructure.Services;

public class CurrencyService : ICurrencyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CurrencyService> _logger;
    private readonly Dictionary<Currency, decimal> _exchangeRates = new();

    public CurrencyService(
        IHttpClientFactory httpClientFactory,
        ILogger<CurrencyService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        
        _exchangeRates[Currency.TRY] = 1.0m;
        _exchangeRates[Currency.USD] = 0.03m;
        _exchangeRates[Currency.EUR] = 0.028m;
        _exchangeRates[Currency.GBP] = 0.024m;
    }

    public async Task<decimal> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency, CancellationToken cancellationToken = default)
    {
        if (fromCurrency == toCurrency)
            return amount;

        var rate = await GetExchangeRateAsync(fromCurrency, toCurrency, cancellationToken);
        return amount * rate;
    }

    public async Task<Dictionary<Currency, decimal>> GetExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _exchangeRates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public async Task<decimal> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency, CancellationToken cancellationToken = default)
    {
        if (fromCurrency == toCurrency)
            return 1.0m;

        var rates = await GetExchangeRatesAsync(cancellationToken);
        
        if (fromCurrency == Currency.TRY && rates.ContainsKey(toCurrency))
        {
            return rates[toCurrency];
        }
        
        if (toCurrency == Currency.TRY && rates.ContainsKey(fromCurrency))
        {
            return 1.0m / rates[fromCurrency];
        }
        
        if (rates.ContainsKey(fromCurrency) && rates.ContainsKey(toCurrency))
        {
            var fromRate = rates[fromCurrency];
            var toRate = rates[toCurrency];
            return toRate / fromRate;
        }

        return 1.0m;
    }
}

