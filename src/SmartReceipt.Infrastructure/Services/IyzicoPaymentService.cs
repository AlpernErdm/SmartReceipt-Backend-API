using System.Linq;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;
using IyzipayPaymentRequest = Iyzipay.Request.CreatePaymentRequest;
using IyzipayRefundRequest = Iyzipay.Request.CreateRefundRequest;
using RetrievePaymentRequest = Iyzipay.Request.RetrievePaymentRequest;
using RetrieveCheckoutFormRequest = Iyzipay.Request.RetrieveCheckoutFormRequest;
using CheckoutForm = Iyzipay.Model.CheckoutForm;

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

    public async Task<PaymentResult> CreatePaymentAsync(Application.Common.Interfaces.CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating iyzico payment for user {UserId}, amount: {Amount}", 
                request.UserId, request.Amount);

            // Use Checkout Form if no card token provided
            if (string.IsNullOrEmpty(request.CardToken))
            {
                _logger.LogInformation("No card token provided, using Checkout Form");
                return await CreateCheckoutFormPaymentAsync(request, cancellationToken);
            }

            // Direct payment with card token
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

            var paymentCard = new PaymentCard
            {
                CardToken = request.CardToken,
                CardUserKey = request.CardUserKey
            };
            createPaymentRequest.PaymentCard = paymentCard;

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

    private async Task<PaymentResult> CreateCheckoutFormPaymentAsync(Application.Common.Interfaces.CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating iyzico checkout form for user {UserId}", request.UserId);

            // Format prices with invariant culture to ensure decimal separator is dot (.)
            var priceStr = request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            
            var checkoutFormRequest = new CreateCheckoutFormInitializeRequest
            {
                Locale = Iyzipay.Model.Locale.TR.ToString(),
                ConversationId = request.UserId.ToString(),
                Price = priceStr,
                PaidPrice = priceStr,
                Currency = Iyzipay.Model.Currency.TRY.ToString(),
                BasketId = request.SubscriptionId?.ToString() ?? request.UserId.ToString(),
                PaymentGroup = Iyzipay.Model.PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = request.CallbackUrl ?? "http://localhost:3000/api/payment-callback",
                EnabledInstallments = new List<int> { 1 }
            };

            var buyer = new Buyer
            {
                Id = request.UserId.ToString().Substring(0, 11), // Max 11 chars
                Name = "SmartReceipt",
                Surname = "User",
                Email = "user@smartreceipt.com",
                IdentityNumber = "11111111111",
                RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                City = "Istanbul",
                Country = "Turkey",
                Ip = "85.34.78.112"
            };

            var address = new Address
            {
                ContactName = "SmartReceipt User",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1"
            };

            var basketItem = new BasketItem
            {
                Id = "SUB1",
                Name = request.Description ?? "Subscription Payment",
                Category1 = "Subscription",
                Category2 = "Premium",
                ItemType = BasketItemType.VIRTUAL.ToString(),
                Price = priceStr
            };

            checkoutFormRequest.Buyer = buyer;
            checkoutFormRequest.ShippingAddress = address;
            checkoutFormRequest.BillingAddress = address;
            checkoutFormRequest.BasketItems = new List<BasketItem> { basketItem };

            _logger.LogInformation("Checkout form request prepared: Price={Price}, BasketId={BasketId}", 
                priceStr, checkoutFormRequest.BasketId);

            var checkoutForm = await Task.Run(() => 
                CheckoutFormInitialize.Create(checkoutFormRequest, _iyzipayOptions), 
                cancellationToken);

            _logger.LogInformation("Checkout form response: Status={Status}, ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}", 
                checkoutForm.Status, checkoutForm.ErrorCode, checkoutForm.ErrorMessage);

            if (checkoutForm.Status == "success")
            {
                _logger.LogInformation("Checkout form created successfully with token: {Token}", checkoutForm.Token);
                
                return new PaymentResult
                {
                    IsSuccess = true,
                    PaymentId = checkoutForm.Token,
                    Status = PaymentStatus.Pending,
                    RedirectUrl = checkoutForm.PaymentPageUrl
                };
            }
            else
            {
                _logger.LogWarning("Checkout form creation failed: {ErrorMessage}", checkoutForm.ErrorMessage);
                return new PaymentResult
                {
                    IsSuccess = false,
                    ErrorMessage = checkoutForm.ErrorMessage ?? "Ödeme formu oluşturulamadı",
                    Status = PaymentStatus.Failed
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create checkout form");
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
        // Check if it's a checkout form token (GUID format with dashes)
        // Checkout form tokens are GUIDs like: 5a39058a-a1ff-41b7-b459-351bdceb162c
        // Normal payment IDs are numeric strings
        if (Guid.TryParse(paymentId, out _))
        {
            _logger.LogInformation("Payment ID is GUID format, using checkout form retrieve");
            return await RetrieveCheckoutFormResultAsync(paymentId, cancellationToken);
        }
        
        _logger.LogInformation("Payment ID is numeric format, using payment retrieve");
        return await ProcessPaymentAsync(paymentId, cancellationToken);
    }

    private async Task<PaymentResult> RetrieveCheckoutFormResultAsync(string token, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving checkout form result for token: {Token}", token);

            var request = new RetrieveCheckoutFormRequest
            {
                Token = token
            };

            var checkoutForm = await Task.Run(() => 
                CheckoutForm.Retrieve(request, _iyzipayOptions), 
                cancellationToken);

            _logger.LogInformation("Checkout form result: Status={Status}, PaymentStatus={PaymentStatus}", 
                checkoutForm.Status, checkoutForm.PaymentStatus);

            if (checkoutForm.Status == "success")
            {
                var status = checkoutForm.PaymentStatus == "SUCCESS" 
                    ? PaymentStatus.Completed 
                    : checkoutForm.PaymentStatus == "FAILURE" 
                        ? PaymentStatus.Failed 
                        : PaymentStatus.Pending;

                return new PaymentResult
                {
                    IsSuccess = checkoutForm.PaymentStatus == "SUCCESS",
                    PaymentId = checkoutForm.PaymentId,
                    TransactionId = checkoutForm.PaymentId,
                    Status = status,
                    ErrorMessage = checkoutForm.ErrorMessage
                };
            }
            else
            {
                _logger.LogWarning("Checkout form retrieve failed: {ErrorMessage}", checkoutForm.ErrorMessage);
                return new PaymentResult
                {
                    IsSuccess = false,
                    PaymentId = token,
                    ErrorMessage = checkoutForm.ErrorMessage ?? "Ödeme durumu alınamadı",
                    Status = PaymentStatus.Failed
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve checkout form result");
            return new PaymentResult
            {
                IsSuccess = false,
                PaymentId = token,
                ErrorMessage = ex.Message,
                Status = PaymentStatus.Failed
            };
        }
    }

    public async Task<RefundResult> CreateRefundAsync(Application.Common.Interfaces.CreateRefundRequest request, CancellationToken cancellationToken = default)
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

