using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Application.DTOs;
using SmartReceipt.Domain.Entities;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Application.Features.Subscriptions.Commands.Subscribe;

public record SubscribeCommand(Guid UserId, Guid PlanId, BillingPeriod BillingPeriod) : IRequest<SubscriptionDto>;

public class SubscribeCommandValidator : AbstractValidator<SubscribeCommand>
{
    public SubscribeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID gereklidir");

        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("Plan ID gereklidir");

        RuleFor(x => x.BillingPeriod)
            .IsInEnum().WithMessage("Geçerli bir fatura dönemi seçiniz");
    }
}

public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;

    public SubscribeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDto> Handle(SubscribeCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(sp => sp.Id == request.PlanId && sp.IsActive, cancellationToken);

        if (plan == null)
        {
            throw new KeyNotFoundException("Plan bulunamadı veya aktif değil");
        }

        var existingSubscription = await _context.Subscriptions
            .Where(s => s.UserId == request.UserId)
            .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingSubscription != null)
        {
            throw new InvalidOperationException("Zaten aktif bir aboneliğiniz bulunmaktadır");
        }

        var now = DateTime.UtcNow;
        var startDate = now;
        var endDate = request.BillingPeriod == BillingPeriod.Monthly
            ? now.AddMonths(1)
            : now.AddYears(1);

        var status = plan.TrialDays.HasValue && plan.TrialDays > 0
            ? SubscriptionStatus.Trial
            : SubscriptionStatus.Active;

        if (status == SubscriptionStatus.Trial)
        {
            endDate = now.AddDays(plan.TrialDays!.Value);
        }

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SubscriptionPlanId = request.PlanId,
            Status = status,
            BillingPeriod = request.BillingPeriod,
            StartDate = startDate,
            EndDate = endDate,
            NextBillingDate = status == SubscriptionStatus.Trial ? endDate : endDate,
            AutoRenew = true,
            CreatedAt = now
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        var subscriptionWithPlan = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.Id == subscription.Id, cancellationToken);

        if (subscriptionWithPlan == null)
        {
            throw new InvalidOperationException("Abonelik oluşturuldu ancak yüklenemedi");
        }

        return new SubscriptionDto
        {
            Id = subscriptionWithPlan.Id,
            UserId = subscriptionWithPlan.UserId,
            Plan = subscriptionWithPlan.SubscriptionPlan.Adapt<SubscriptionPlanDto>(),
            Status = subscriptionWithPlan.Status,
            BillingPeriod = subscriptionWithPlan.BillingPeriod,
            StartDate = subscriptionWithPlan.StartDate,
            EndDate = subscriptionWithPlan.EndDate,
            CancelledAt = subscriptionWithPlan.CancelledAt,
            CancellationReason = subscriptionWithPlan.CancellationReason,
            NextBillingDate = subscriptionWithPlan.NextBillingDate,
            AutoRenew = subscriptionWithPlan.AutoRenew,
            CreatedAt = subscriptionWithPlan.CreatedAt
        };
    }
}

