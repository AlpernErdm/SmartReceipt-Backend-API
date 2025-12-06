using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Entities;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Infrastructure.Persistence;

public static class SeedDataExtensions
{
    public static async Task SeedSubscriptionPlansAsync(this IApplicationDbContext context)
    {
        if (await context.SubscriptionPlans.AnyAsync())
        {
            return; // Seed data zaten var
        }

        var plans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Ücretsiz",
                Description = "Temel özelliklerle başlayın",
                PlanType = PlanType.Free,
                MonthlyPrice = 0,
                YearlyPrice = 0,
                MonthlyScanLimit = 10,
                StorageLimitMB = 100,
                TrialDays = null,
                HasApiAccess = false,
                HasAdvancedAnalytics = false,
                HasTeamManagement = false,
                HasPrioritySupport = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Temel",
                Description = "Küçük işletmeler için ideal",
                PlanType = PlanType.Basic,
                MonthlyPrice = 99.00m,
                YearlyPrice = 990.00m, // 2 ay bedava
                MonthlyScanLimit = 100,
                StorageLimitMB = 1024, // 1 GB
                TrialDays = 14,
                HasApiAccess = false,
                HasAdvancedAnalytics = false,
                HasTeamManagement = false,
                HasPrioritySupport = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Profesyonel",
                Description = "Büyüyen işletmeler için",
                PlanType = PlanType.Pro,
                MonthlyPrice = 299.00m,
                YearlyPrice = 2990.00m, // 2 ay bedava
                MonthlyScanLimit = 1000,
                StorageLimitMB = 10240, // 10 GB
                TrialDays = 14,
                HasApiAccess = true,
                HasAdvancedAnalytics = true,
                HasTeamManagement = false,
                HasPrioritySupport = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Kurumsal",
                Description = "Büyük organizasyonlar için",
                PlanType = PlanType.Enterprise,
                MonthlyPrice = 999.00m,
                YearlyPrice = 9990.00m, // 2 ay bedava
                MonthlyScanLimit = -1, // Sınırsız
                StorageLimitMB = 102400, // 100 GB
                TrialDays = 30,
                HasApiAccess = true,
                HasAdvancedAnalytics = true,
                HasTeamManagement = true,
                HasPrioritySupport = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.SubscriptionPlans.AddRange(plans);
        await context.SaveChangesAsync();
    }
}

