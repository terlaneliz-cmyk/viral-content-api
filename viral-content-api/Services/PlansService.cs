using Microsoft.EntityFrameworkCore;
using ViralContentApi.Data;
using ViralContentApi.DTOs;
using ViralContentApi.Models;

namespace ViralContentApi.Services
{
    public class PlansService : IPlansService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IAiUsageLimitService _aiUsageLimitService;
        private readonly IUserSubscriptionService _userSubscriptionService;
        private readonly IBillingProviderService _billingProviderService;

        public PlansService(
            IConfiguration configuration,
            AppDbContext context,
            IAiUsageLimitService aiUsageLimitService,
            IUserSubscriptionService userSubscriptionService,
            IBillingProviderService billingProviderService)
        {
            _configuration = configuration;
            _context = context;
            _aiUsageLimitService = aiUsageLimitService;
            _userSubscriptionService = userSubscriptionService;
            _billingProviderService = billingProviderService;
        }

        public List<PublicPlanInfoResponse> GetPlans()
        {
            var configuredLimits = GetConfiguredLimits();
            var preferredOrder = new[] { "Free", "Pro", "Agency" };

            return configuredLimits
                .Select(x => BuildPlan(x.Key, x.Value))
                .OrderBy(x =>
                {
                    var index = Array.IndexOf(preferredOrder, x.Name);
                    return index == -1 ? int.MaxValue : index;
                })
                .ThenBy(x => x.Name)
                .ToList();
        }

        public PublicPlanInfoResponse? GetPlanByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var configuredLimits = GetConfiguredLimits();

            if (!configuredLimits.TryGetValue(name, out var dailyLimit))
            {
                return null;
            }

            return BuildPlan(name, dailyLimit);
        }

        public PlanComparisonResponse GetComparison()
        {
            var plans = GetPlans();
            var comparisonSettings = GetComparisonSettings();

            var features = comparisonSettings
                .Select(setting => new PlanComparisonFeatureResponse
                {
                    FeatureKey = setting.Key,
                    FeatureName = setting.Label,
                    AvailabilityByPlan = plans.ToDictionary(
                        plan => plan.Name,
                        plan => plan.FeatureKeys.Any(key => key.Equals(setting.Key, StringComparison.OrdinalIgnoreCase)),
                        StringComparer.OrdinalIgnoreCase)
                })
                .ToList();

            return new PlanComparisonResponse
            {
                Plans = plans,
                Features = features
            };
        }

        public PlanCheckoutSummaryResponse? GetCheckoutSummary(string name, string? billingCycle)
        {
            var plan = GetPlanByName(name);

            if (plan == null)
            {
                return null;
            }

            var normalizedBillingCycle = NormalizeBillingCycle(billingCycle);
            var resolvedPrice = normalizedBillingCycle == "yearly"
                ? plan.YearlyPrice
                : plan.MonthlyPrice;

            var resolvedStripePriceLookupKey = normalizedBillingCycle == "yearly"
                ? plan.StripePriceLookupKeyYearly
                : plan.StripePriceLookupKeyMonthly;

            var yearlySavingsAmount = CalculateYearlySavings(plan.MonthlyPrice, plan.YearlyPrice);

            return new PlanCheckoutSummaryResponse
            {
                PlanName = plan.Name,
                DisplayName = plan.DisplayName,
                BillingCycle = normalizedBillingCycle,
                Price = resolvedPrice,
                Currency = plan.Currency,
                StripePriceLookupKey = resolvedStripePriceLookupKey,
                DailyGenerateLimit = plan.DailyGenerateLimit,
                MonthlyPrice = plan.MonthlyPrice,
                YearlyPrice = plan.YearlyPrice,
                YearlyDiscountPercent = plan.YearlyDiscountPercent,
                YearlySavingsAmount = yearlySavingsAmount,
                IsRecommended = plan.IsRecommended,
                Description = plan.Description,
                Features = plan.Features
            };
        }

        public async Task<MyPlanResponse?> GetMyPlanAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return null;
            }

            var planName = string.IsNullOrWhiteSpace(user.Plan) ? "Free" : user.Plan;
            var plan = GetPlanByName(planName);

            if (plan == null)
            {
                return null;
            }

            var usage = await _aiUsageLimitService.GetUsageStatusAsync(userId);
            var upgrades = GetAvailableUpgrades(plan.Name);

            return new MyPlanResponse
            {
                PlanName = plan.Name,
                DisplayName = plan.DisplayName,
                DailyGenerateLimit = usage.DailyLimit,
                UsedToday = usage.UsedToday,
                RemainingToday = usage.RemainingToday,
                IsLimitReached = usage.RemainingToday <= 0,
                MonthlyPrice = plan.MonthlyPrice,
                YearlyPrice = plan.YearlyPrice,
                YearlyDiscountPercent = plan.YearlyDiscountPercent,
                Currency = plan.Currency,
                Description = plan.Description,
                Features = plan.Features,
                AvailableUpgrades = upgrades
            };
        }

        public async Task<PlanUpgradePreviewResponse?> GetUpgradePreviewAsync(int userId, PlanUpgradePreviewRequest request)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return null;
            }

            var currentPlanName = string.IsNullOrWhiteSpace(user.Plan) ? "Free" : user.Plan;
            var targetPlanName = ResolveTargetPlanName(
                request.TargetPlanName,
                GetOptionalPropertyValue(request, "PlanName"));

            if (string.IsNullOrWhiteSpace(targetPlanName))
            {
                throw new ArgumentException("TargetPlanName is required.");
            }

            var currentPlan = GetPlanByName(currentPlanName);
            var targetPlan = GetPlanByName(targetPlanName);

            if (currentPlan == null || targetPlan == null)
            {
                return null;
            }

            var normalizedBillingCycle = NormalizeBillingCycle(request.BillingCycle);
            var isSamePlan = currentPlan.Name.Equals(targetPlan.Name, StringComparison.OrdinalIgnoreCase);

            var currentRank = GetPlanRank(currentPlan.Name);
            var targetRank = GetPlanRank(targetPlan.Name);

            var isUpgrade = targetRank > currentRank;
            var isDowngrade = targetRank < currentRank;

            var stripePriceLookupKey = normalizedBillingCycle == "yearly"
                ? targetPlan.StripePriceLookupKeyYearly
                : targetPlan.StripePriceLookupKeyMonthly;

            var price = normalizedBillingCycle == "yearly"
                ? targetPlan.YearlyPrice
                : targetPlan.MonthlyPrice;

            var message = BuildUpgradePreviewMessage(
                isSamePlan,
                isUpgrade,
                isDowngrade,
                currentPlan.Name,
                targetPlan.Name);

            return new PlanUpgradePreviewResponse
            {
                CurrentPlanName = currentPlan.Name,
                TargetPlanName = targetPlan.Name,
                TargetDisplayName = targetPlan.DisplayName,
                BillingCycle = normalizedBillingCycle,
                Price = price,
                Currency = targetPlan.Currency,
                StripePriceLookupKey = stripePriceLookupKey,
                CurrentDailyGenerateLimit = currentPlan.DailyGenerateLimit,
                TargetDailyGenerateLimit = targetPlan.DailyGenerateLimit,
                IsUpgrade = isUpgrade,
                IsSamePlan = isSamePlan,
                IsDowngrade = isDowngrade,
                Message = message,
                Features = targetPlan.Features
            };
        }

        public async Task<CreateCheckoutSessionResponse?> CreateCheckoutSessionAsync(int userId, CreateCheckoutSessionRequest request)
{
    var user = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == userId);

    if (user == null)
    {
        return null;
    }

    if (string.IsNullOrWhiteSpace(user.Email))
    {
        throw new InvalidOperationException("The current user does not have a valid email address.");
    }

    var targetPlanName = ResolveTargetPlanName(
        request.TargetPlanName,
        request.PlanName);

    if (string.IsNullOrWhiteSpace(targetPlanName))
    {
        throw new ArgumentException("TargetPlanName is required.");
    }

    var preview = await GetUpgradePreviewAsync(userId, new PlanUpgradePreviewRequest
    {
        TargetPlanName = targetPlanName,
        BillingCycle = request.BillingCycle ?? "monthly"
    });

    if (preview == null)
    {
        throw new InvalidOperationException("Upgrade preview could not be created for this plan change.");
    }

    if (preview.IsSamePlan)
    {
        throw new InvalidOperationException($"You are already on the {preview.TargetPlanName} plan.");
    }

    if (preview.IsDowngrade)
    {
        throw new InvalidOperationException(
            $"Downgrade checkout is not supported through this endpoint. Current plan: {preview.CurrentPlanName}, target plan: {preview.TargetPlanName}.");
    }

    if (!preview.IsUpgrade)
    {
        throw new InvalidOperationException(
            $"This plan change is not a valid upgrade. Current plan: {preview.CurrentPlanName}, target plan: {preview.TargetPlanName}.");
    }

    var successUrl = !string.IsNullOrWhiteSpace(request.SuccessUrl)
        ? request.SuccessUrl
        : _configuration["StripeUrls:SuccessUrl"];

    var cancelUrl = !string.IsNullOrWhiteSpace(request.CancelUrl)
        ? request.CancelUrl
        : _configuration["StripeUrls:CancelUrl"];

    if (string.IsNullOrWhiteSpace(successUrl))
    {
        throw new InvalidOperationException("Stripe success URL is missing.");
    }

    if (string.IsNullOrWhiteSpace(cancelUrl))
    {
        throw new InvalidOperationException("Stripe cancel URL is missing.");
    }

    try
    {
        var providerResponse = await _billingProviderService.CreateCheckoutSessionAsync(new BillingCheckoutSessionRequest
        {
            UserId = userId,
            CustomerEmail = user.Email,
            PlanName = preview.TargetPlanName,
            BillingCycle = preview.BillingCycle,
            Price = preview.Price,
            Currency = preview.Currency,
            StripePriceLookupKey = preview.StripePriceLookupKey,
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl
        });

        if (providerResponse == null)
        {
            throw new InvalidOperationException("Billing provider returned no checkout session.");
        }

        var subscription = await _userSubscriptionService.CreateOrUpdatePendingSubscriptionAsync(
            userId,
            preview.TargetPlanName,
            preview.BillingCycle,
            preview.Price,
            preview.Currency,
            preview.StripePriceLookupKey ?? string.Empty,
            providerResponse.SessionId ?? string.Empty);

        providerResponse.Subscription = subscription;

        return providerResponse;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(
            $"Checkout session could not be created for this plan change. Details: {ex.Message}",
            ex);
    }
}
        private Dictionary<string, int> GetConfiguredLimits()
        {
            var nestedPlans = _configuration
                .GetSection("AiUsageLimits:Plans")
                .GetChildren()
                .ToDictionary(
                    x => x.Key,
                    x => int.TryParse(x.Value, out var limit) ? limit : 0,
                    StringComparer.OrdinalIgnoreCase);

            if (nestedPlans.Count > 0)
            {
                return nestedPlans;
            }

            var flatLimits = _configuration
                .GetSection("AiUsageLimits")
                .GetChildren()
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToDictionary(
                    x => x.Key,
                    x => int.TryParse(x.Value, out var limit) ? limit : 0,
                    StringComparer.OrdinalIgnoreCase);

            return flatLimits;
        }

        private List<string> GetAvailableUpgrades(string currentPlan)
        {
            var allPlans = GetPlans();
            var preferredOrder = new[] { "Free", "Pro", "Agency" };

            var currentIndex = Array.FindIndex(
                preferredOrder,
                x => x.Equals(currentPlan, StringComparison.OrdinalIgnoreCase));

            if (currentIndex < 0)
            {
                return allPlans.Select(x => x.Name).ToList();
            }

            return preferredOrder
                .Skip(currentIndex + 1)
                .Where(planName => allPlans.Any(x => x.Name.Equals(planName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private PublicPlanInfoResponse BuildPlan(string planName, int dailyLimit)
        {
            var pricing = GetPlanPricing(planName);

            return new PublicPlanInfoResponse
            {
                Name = planName,
                DisplayName = planName,
                DailyGenerateLimit = dailyLimit,
                MonthlyPrice = pricing.MonthlyPrice,
                YearlyPrice = pricing.YearlyPrice,
                YearlyDiscountPercent = pricing.YearlyDiscountPercent,
                Currency = string.IsNullOrWhiteSpace(pricing.Currency) ? "usd" : pricing.Currency,
                Description = pricing.Description ?? string.Empty,
                StripePriceLookupKeyMonthly = ResolveStripePriceId(planName, "monthly", pricing.StripePriceLookupKeyMonthly),
                StripePriceLookupKeyYearly = ResolveStripePriceId(planName, "yearly", pricing.StripePriceLookupKeyYearly),
                Features = pricing.Features ?? new List<string>(),
                FeatureKeys = pricing.FeatureKeys ?? new List<string>(),
                IsRecommended = pricing.IsRecommended
            };
        }

        private PlanPricingSettings GetPlanPricing(string planName)
        {
            var section = _configuration.GetSection($"PlanPricing:{planName}");
            var settings = section.Get<PlanPricingSettings>();
            return settings ?? new PlanPricingSettings();
        }

        private List<PlanComparisonFeatureSetting> GetComparisonSettings()
        {
            var settings = _configuration
                .GetSection("PlanComparisonFeatures")
                .Get<List<PlanComparisonFeatureSetting>>();

            return settings ?? new List<PlanComparisonFeatureSetting>();
        }

        private string ResolveStripePriceId(string planName, string billingCycle, string? fallback)
        {
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                return fallback;
            }

            if (planName.Equals("Free", StringComparison.OrdinalIgnoreCase))
            {
                return _configuration["StripeBilling:FreePriceId"] ?? string.Empty;
            }

            if (planName.Equals("Pro", StringComparison.OrdinalIgnoreCase) &&
                billingCycle.Equals("monthly", StringComparison.OrdinalIgnoreCase))
            {
                return _configuration["StripeBilling:ProMonthlyPriceId"] ?? string.Empty;
            }

            if (planName.Equals("Pro", StringComparison.OrdinalIgnoreCase) &&
                billingCycle.Equals("yearly", StringComparison.OrdinalIgnoreCase))
            {
                return _configuration["StripeBilling:ProYearlyPriceId"] ?? string.Empty;
            }

            if (planName.Equals("Agency", StringComparison.OrdinalIgnoreCase) &&
                billingCycle.Equals("monthly", StringComparison.OrdinalIgnoreCase))
            {
                return _configuration["StripeBilling:AgencyMonthlyPriceId"] ?? string.Empty;
            }

            if (planName.Equals("Agency", StringComparison.OrdinalIgnoreCase) &&
                billingCycle.Equals("yearly", StringComparison.OrdinalIgnoreCase))
            {
                return _configuration["StripeBilling:AgencyYearlyPriceId"] ?? string.Empty;
            }

            return string.Empty;
        }

        private static string NormalizeBillingCycle(string? billingCycle)
        {
            if (string.Equals(billingCycle, "yearly", StringComparison.OrdinalIgnoreCase))
            {
                return "yearly";
            }

            return "monthly";
        }

        private static decimal CalculateYearlySavings(decimal monthlyPrice, decimal yearlyPrice)
        {
            var yearlyMonthlyEquivalent = monthlyPrice * 12;
            var savings = yearlyMonthlyEquivalent - yearlyPrice;

            return savings > 0 ? savings : 0;
        }

        private static int GetPlanRank(string planName)
        {
            return planName.ToLowerInvariant() switch
            {
                "free" => 1,
                "pro" => 2,
                "agency" => 3,
                _ => 0
            };
        }

        private static string BuildUpgradePreviewMessage(
            bool isSamePlan,
            bool isUpgrade,
            bool isDowngrade,
            string currentPlanName,
            string targetPlanName)
        {
            if (isSamePlan)
            {
                return $"You are already on the {currentPlanName} plan.";
            }

            if (isUpgrade)
            {
                return $"You are upgrading from {currentPlanName} to {targetPlanName}.";
            }

            if (isDowngrade)
            {
                return $"This request would change the plan from {currentPlanName} to {targetPlanName} as a downgrade.";
            }

            return $"Plan change preview from {currentPlanName} to {targetPlanName}.";
        }

        private static string ResolveTargetPlanName(string? targetPlanName, string? planName)
        {
            if (!string.IsNullOrWhiteSpace(targetPlanName))
            {
                return targetPlanName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(planName))
            {
                return planName.Trim();
            }

            return string.Empty;
        }

        private static string? GetOptionalPropertyValue(object source, string propertyName)
        {
            var property = source.GetType().GetProperty(propertyName);
            if (property == null)
            {
                return null;
            }

            return property.GetValue(source) as string;
        }
    }
}