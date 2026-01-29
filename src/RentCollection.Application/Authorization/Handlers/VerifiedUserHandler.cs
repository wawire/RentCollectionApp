using Microsoft.AspNetCore.Authorization;
using RentCollection.Application.Authorization.Requirements;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.Application.Authorization.Handlers;

public class VerifiedUserHandler : AuthorizationHandler<VerifiedUserRequirement>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public VerifiedUserHandler(ICurrentUserService currentUserService, IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifiedUserRequirement requirement)
    {
        if (!_currentUserService.UserIdInt.HasValue)
        {
            return;
        }

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserIdInt.Value);
        if (user == null)
        {
            return;
        }

        if (user.IsVerified)
        {
            context.Succeed(requirement);
        }
    }
}
