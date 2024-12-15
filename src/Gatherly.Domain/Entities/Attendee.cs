namespace Gatherly.Domain.Entities;

public class Attendee
{
    public Guid GatheringId { get; set; }

    public Guid MemberId { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}