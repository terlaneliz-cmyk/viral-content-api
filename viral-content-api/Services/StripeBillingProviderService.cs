using Stripe;
using Stripe.Checkout;
using ViralContentApi.DTOs;

namespace ViralContentApi.Services
{
    public class StripeBillingProviderService : IBillingProviderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeBillingProviderService> _logger;

        public StripeBillingProviderService(
            IConfiguration configuration,
            ILogger<StripeBillingProviderService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var secretKey = _configuration["StripeBilling:SecretKey"];
            if (!string.IsNullOrWhiteSpace(secretKey))
            {
                StripeConfiguration.ApiKey = secretKey;
            }
        }

        public async Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(BillingCheckoutSessionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var secretKey = _configuration["StripeBilling:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("Stripe SecretKey is missing.");
            }

            StripeConfiguration.ApiKey = secretKey;

            if (request.UserId <= 0)
            {
                throw new ArgumentException("UserId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            {
                throw new ArgumentException("CustomerEmail is required.");
            }

            if (string.IsNullOrWhiteSpace(request.PlanName))
            {
                throw new ArgumentException("PlanName is required.");
            }

            if (string.IsNullOrWhiteSpace(request.BillingCycle))
            {
                throw new ArgumentException("BillingCycle is required.");
            }

            if (string.IsNullOrWhiteSpace(request.StripePriceLookupKey))
            {
                throw new ArgumentException("StripePriceLookupKey is required.");
            }

            var defaultSuccessUrl = "https://viral-content-api-mcnr.onrender.com/billing/success";
            var defaultCancelUrl = "https://viral-content-api-mcnr.onrender.com/billing/cancel";

            var successUrl = request.SuccessUrl;
            var cancelUrl = request.CancelUrl;

            if (string.IsNullOrWhiteSpace(successUrl) ||
                successUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                successUrl = defaultSuccessUrl;
            }

            if (string.IsNullOrWhiteSpace(cancelUrl) ||
                cancelUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                cancelUrl = defaultCancelUrl;
            }

            var sessionOptions = new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                CustomerEmail = request.CustomerEmail,
                ClientReferenceId = request.UserId.ToString(),
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = request.StripePriceLookupKey,
                        Quantity = 1
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    ["userId"] = request.UserId.ToString(),
                    ["planName"] = request.PlanName,
                    ["billingCycle"] = request.BillingCycle
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["userId"] = request.UserId.ToString(),
                        ["planName"] = request.PlanName,
                        ["billingCycle"] = request.BillingCycle
                    }
                }
            };

            var sessionService = new SessionService();

            var idempotencyKey =
                $"checkout:{request.UserId}:{request.PlanName}:{request.BillingCycle}:{Guid.NewGuid():N}";

            var requestOptions = new RequestOptions
            {
                IdempotencyKey = idempotencyKey
            };

            var session = await sessionService.CreateAsync(sessionOptions, requestOptions);

            _logger.LogInformation(
                "Stripe checkout session created. UserId: {UserId}, Plan: {PlanName}, BillingCycle: {BillingCycle}, SessionId: {SessionId}, IdempotencyKey: {IdempotencyKey}, SuccessUrl: {SuccessUrl}, CancelUrl: {CancelUrl}",
                request.UserId,
                request.PlanName,
                request.BillingCycle,
                session.Id,
                idempotencyKey,
                successUrl,
                cancelUrl);

            return new CreateCheckoutSessionResponse
            {
                Provider = "Stripe",
                SessionId = session.Id,
                CheckoutUrl = session.Url,
                ExpiresAtUtc = session.ExpiresAt,
                Message = "Stripe checkout session created successfully."
            };
        }

        public Task<BillingWebhookResponse> ProcessWebhookAsync(BillingWebhookRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Task.FromResult(new BillingWebhookResponse
            {
                Success = true,
                Message = "Stripe webhook request accepted for controller-level processing."
            });
        }
    }
}