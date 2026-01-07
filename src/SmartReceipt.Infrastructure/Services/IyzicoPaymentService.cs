using System.Linq;
using Iyzipay.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;
using IyzipayPaymentRequest = Iyzipay.Request.CreatePaymentRequest;
using IyzipayRefundRequest = Iyzipay.Request.CreateRefundRequest;
using RetrievePaymentRequest = Iyzipay.Request.RetrievePaymentRequest;

namespace SmartReceipt.Infrastructure.Services;

public class IyzicoPaymentService : IPaymentService
{
    private readonly PaymentOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IyzicoPaymentService> _logger;
    private readonly Iyzipay.Options _iyzipayOptions;

    public IyzicoPaymentService(
        IOptions<PaymentOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<IyzicoPaymentService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        
        _iyzipayOptions = new Iyzipay.Options
        {
            ApiKey = _options.IyzicoApiKey,
            SecretKey = _options.IyzicoSecretKey,
            BaseUrl = _options.IyzicoBaseUrl
        };
    }

    public async Task<PaymentResult> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating iyzico payment for user {UserId}, amount: {Amount}", 
                request.UserId, request.Amount);

            var createPaymentRequest = new IyzipayPaymentRequest
            {
                Locale = Iyzipay.Model.Locale.TR.ToString(),
                ConversationId = request.UserId.ToString(),
                Price = request.Amount.ToString("F2"),
                PaidPrice = request.Amount.ToString("F2"),
                Currency = Iyzipay.Model.Currency.TRY.ToString(),
                Installment = 1,
                BasketId = request.SubscriptionId?.ToString() ?? request.UserId.ToString(),
                PaymentChannel = Iyzipay.Model.PaymentChannel.WEB.ToString(),
                PaymentGroup = Iyzipay.Model.PaymentGroup.PRODUCT.ToString()
            };

            var buyer = new Buyer
            {
                Id = request.UserId.ToString(),
                Name = "SmartReceipt",
                Surname = "User",
                Email = "user@smartreceipt.com",
                IdentityNumber = "11111111111",
                RegistrationAddress = "Istanbul",
                City = "Istanbul",
                Country = "Turkey",
                Ip = "127.0.0.1"
            };

            var address = new Address
            {
                ContactName = "SmartReceipt User",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Istanbul"
            };

            var basketItem = new BasketItem
            {
                Id = request.SubscriptionId?.ToString() ?? "1",
                Name = request.Description ?? "Subscription Payment",
                Category1 = "Subscription",
                ItemType = BasketItemType.VIRTUAL.ToString(),
                Price = request.Amount.ToString("F2")
            };

            createPaymentRequest.Buyer = buyer;
            createPaymentRequest.ShippingAddress = address;
            createPaymentRequest.BillingAddress = address;
            createPaymentRequest.BasketItems = new List<BasketItem> { basketItem };

            if (!string.IsNullOrEmpty(request.CardToken))
            {
                var paymentCard = new PaymentCard
                {
                    CardToken = request.CardToken,
                    CardUserKey = request.CardUserKey
                };
                createPaymentRequest.PaymentCard = paymentCard;
            }
            else
            {
                _logger.LogWarning("CardToken is missing. Payment cannot be processed without card token.");
                return new PaymentResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Kart bilgileri eksik. Lütfen kart bilgilerinizi girin.",
                    Status = PaymentStatus.Failed
                };
            }

            if (!string.IsNullOrEmpty(request.CallbackUrl))
            {
                createPaymentRequest.CallbackUrl = request.CallbackUrl;
            }

            var payment = await Task.Run(() => Iyzipay.Model.Payment.Create(createPaymentRequest, _iyzipayOptions), cancellationToken);

            if (payment.Status == "success")
            {
                var transactionId = payment.PaymentId;
                
                if (payment.GetType().GetProperty("ItemTransactions") != null)
                {
                    var itemTransactions = payment.GetType().GetProperty("ItemTransactions")?.GetValue(payment) as System.Collections.IEnumerable;
                    if (itemTransactions != null)
                    {
                        foreach (var item in itemTransactions)
                        {
                            var paymentTransactionId = item?.GetType().GetProperty("PaymentTransactionId")?.GetValue(item)?.ToString();
                            if (!string.IsNullOrEmpty(paymentTransactionId))
                            {
                                transactionId = paymentTransactionId;
                                break;
                            }
                        }
                    }
                }
                
                return new PaymentResult
                {
                    IsSuccess = true,
                    PaymentId = payment.PaymentId,
                    TransactionId = transactionId,
                    Status = PaymentStatus.Completed,
                    RedirectUrl = null
                };
            }
            else
            {
                _logger.LogWarning("iyzico payment failed: Status={Status}, ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}", 
                    payment.Status, payment.ErrorCode, payment.ErrorMessage);
                return new PaymentResult
                {
                    IsSuccess = false,
                    ErrorMessage = payment.ErrorMessage ?? $"Ödeme başarısız: {payment.ErrorCode}",
                    Status = PaymentStatus.Failed
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create iyzico payment");
            return new PaymentResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                Status = PaymentStatus.Failed
            };
        }
    }

    public async Task<PaymentResult> ProcessPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var retrievePaymentRequest = new RetrievePaymentRequest
            {
                PaymentId = paymentId
            };

            var payment = await Task.Run(() => Iyzipay.Model.Payment.Retrieve(retrievePaymentRequest, _iyzipayOptions), cancellationToken);

            if (payment.Status == "success")
            {
                var status = payment.PaymentStatus == "SUCCESS" 
                    ? PaymentStatus.Completed 
                    : payment.PaymentStatus == "FAILURE" 
                        ? PaymentStatus.Failed 
                        : PaymentStatus.Pending;

                var transactionId = payment.PaymentId;
                
                if (payment.GetType().GetProperty("ItemTransactions") != null)
                {
                    var itemTransactions = payment.GetType().GetProperty("ItemTransactions")?.GetValue(payment) as System.Collections.IEnumerable;
                    if (itemTransactions != null)
                    {
                        foreach (var item in itemTransactions)
                        {
                            var paymentTransactionId = item?.GetType().GetProperty("PaymentTransactionId")?.GetValue(item)?.ToString();
                            if (!string.IsNullOrEmpty(paymentTransactionId))
                            {
                                transactionId = paymentTransactionId;
                                break;
                            }
                        }
                    }
                }

                return new PaymentResult
                {
                    IsSuccess = payment.Status == "success",
                    PaymentId = payment.PaymentId,
                    TransactionId = transactionId,
                    Status = status
                };
            }
            else
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    PaymentId = paymentId,
                    ErrorMessage = payment.ErrorMessage,
                    Status = PaymentStatus.Failed
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment {PaymentId}", paymentId);
            return new PaymentResult
            {
                IsSuccess = false,
                PaymentId = paymentId,
                ErrorMessage = ex.Message,
                Status = PaymentStatus.Failed
            };
        }
    }

    public async Task<PaymentResult> GetPaymentStatusAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        return await ProcessPaymentAsync(paymentId, cancellationToken);
    }

    public async Task<RefundResult> CreateRefundAsync(CreateRefundRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating refund for payment {PaymentId}, amount: {Amount}", 
                request.PaymentId, request.Amount);

            var retrievePaymentRequest = new RetrievePaymentRequest
            {
                PaymentId = request.PaymentId.ToString()
            };

            var payment = await Task.Run(() => Iyzipay.Model.Payment.Retrieve(retrievePaymentRequest, _iyzipayOptions), cancellationToken);

            if (payment.Status != "success")
            {
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = payment.ErrorMessage ?? "Payment not found",
                    Status = RefundStatus.Failed
                };
            }

            var paymentTransactionId = payment.PaymentId;
            
            if (payment.GetType().GetProperty("ItemTransactions") != null)
            {
                var itemTransactions = payment.GetType().GetProperty("ItemTransactions")?.GetValue(payment) as System.Collections.IEnumerable;
                if (itemTransactions != null)
                {
                    foreach (var item in itemTransactions)
                    {
                        var transactionId = item?.GetType().GetProperty("PaymentTransactionId")?.GetValue(item)?.ToString();
                        if (!string.IsNullOrEmpty(transactionId))
                        {
                            paymentTransactionId = transactionId;
                            break;
                        }
                    }
                }
            }

            var createRefundRequest = new IyzipayRefundRequest
            {
                Locale = Iyzipay.Model.Locale.TR.ToString(),
                ConversationId = request.PaymentId.ToString(),
                PaymentTransactionId = paymentTransactionId,
                Price = request.Amount.ToString("F2"),
                Currency = Iyzipay.Model.Currency.TRY.ToString(),
                Ip = "127.0.0.1"
            };

            var refund = await Task.Run(() => Iyzipay.Model.Refund.Create(createRefundRequest, _iyzipayOptions), cancellationToken);

            if (refund.Status == "success")
            {
                return new RefundResult
                {
                    IsSuccess = true,
                    RefundId = refund.PaymentId,
                    Status = RefundStatus.Completed
                };
            }
            else
            {
                _logger.LogWarning("Refund failed: {ErrorMessage}", refund.ErrorMessage);
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = refund.ErrorMessage,
                    Status = RefundStatus.Failed
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create refund");
            return new RefundResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                Status = RefundStatus.Failed
            };
        }
    }

    public Task<bool> VerifyWebhookAsync(string signature, string payload, PaymentProvider provider)
    {
        return Task.FromResult(true);
    }
}

