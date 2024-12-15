using Gatherly.Domain.Entities;
using Gatherly.Domain.Repositories;
using MediatR;

namespace Gatherly.Application.Gatherings.Commands.CreateGathering;

internal sealed class CreateGatheringCommandHandler : IRequestHandler<CreateGatheringCommand>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IGatheringRepository _gatheringRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGatheringCommandHandler(
        IMemberRepository memberRepository,
        IGatheringRepository gatheringRepository,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _gatheringRepository = gatheringRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CreateGatheringCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);

        if (member is null)
        {
            return Unit.Value;
        }

        // Create gathering
        var gathering = new Gathering
        {
            Id = Guid.NewGuid(),
            Creator = member,
            Type = request.Type,
            ScheduledAtUtc = request.ScheduledAtUtc,
            Name = request.Name,
            Location = request.Location
        };

        // Calculate gathering type details
        switch (gathering.Type)
        {
            case GatheringType.WithFixedNumberOfAttendees:
                if (request.MaximumNumberOfAttendees is null)
                {
                    throw new Exception(
                        $"{nameof(request.MaximumNumberOfAttendees)} can't be null.");
                }

                gathering.MaximumNumberOfAttendees = request.MaximumNumberOfAttendees;
                break;
            case GatheringType.WithExpirationForInvitations:
                if (request.InvitationsValidBeforeInHours is null)
                {
                    throw new Exception(
                        $"{nameof(request.InvitationsValidBeforeInHours)} can't be null.");
                }

                gathering.InvitationsExpireAtUtc =
                    gathering.ScheduledAtUtc.AddHours(-request.InvitationsValidBeforeInHours.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(GatheringType));
        }

        _gatheringRepository.Add(gathering);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}