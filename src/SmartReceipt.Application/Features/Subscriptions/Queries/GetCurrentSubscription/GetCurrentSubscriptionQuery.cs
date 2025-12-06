using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Application.DTOs;

namespace SmartReceipt.Application.Features.Subscriptions.Queries.GetCurrentSubscription;

public record GetCurrentSubscriptionQuery(Guid UserId) : IRequest<SubscriptionDto?>;

public class GetCurrentSubscriptionQueryHandler : IRequestHandler<GetCurrentSubscriptionQuery, SubscriptionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCurrentSubscriptionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDto?> Handle(GetCurrentSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.UserId == request.UserId)
            .Where(s => s.Status == Domain.Enums.SubscriptionStatus.Active || 
                       s.Status == Domain.Enums.SubscriptionStatus.Trial)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
        {
            return null;
        }

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

