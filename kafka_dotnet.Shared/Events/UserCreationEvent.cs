public sealed record UserCreationEvent
(
    Guid UserId,
    string Username,
    string Email,
    DateTime CreatedAt,
    string CorrelationId
);

public sealed record UserUpdatedEvent
(
    Guid UserId,
    string Username,
    string Email,
    DateTime UpdatedAt
);
public sealed record UserDeletedEvent
(
    Guid UserId,
    string Email
);