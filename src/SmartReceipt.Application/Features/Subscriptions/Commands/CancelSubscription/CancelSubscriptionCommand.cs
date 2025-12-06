using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Application.DTOs;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Application.Features.Subscriptions.Commands.CancelSubscription;

public record CancelSubscriptionCommand(Guid UserId, string? Reason = null) : IRequest<SubscriptionDto>;

public class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID gereklidir");
    }
}

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;

    public CancelSubscriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDto> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.UserId == request.UserId)
            .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
        {
            throw new Exception("Aktif abonelik bulunamadı");
        }

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.CancellationReason = request.Reason;
        subscription.AutoRenew = false;
        subscription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new SubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            Plan = subscription.SubscriptionPlan.Adapt<SubscriptionPlanDto>(),
            Status = subscription.Status,
            BillingPeriod = subscription.BillingPeriod,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            CancelledAt = subscription.CancelledAt,
            CancellationReason = subscription.CancellationReason,
            NextBillingDate = subscription.NextBillingDate,
            AutoRenew = subscription.AutoRenew,
            CreatedAt = subscription.CreatedAt
        };
    }
}

