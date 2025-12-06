using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Application.DTOs;

namespace SmartReceipt.Application.Features.Auth.Queries;

public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IApplicationDbContext _context;

    public GetCurrentUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Kullanıcı bulunamadı");
        }

        return user.Adapt<UserDto>();
    }
}
