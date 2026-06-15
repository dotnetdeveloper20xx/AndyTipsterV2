namespace AndyTipster.Application.Community.DTOs;

public record CommentDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? AuthorAvatarUrl { get; init; }
    public Guid TipId { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsApproved { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateCommentDto
{
    public Guid TipId { get; init; }
    public string Content { get; init; } = string.Empty;
}

public record PollDto
{
    public Guid Id { get; init; }
    public string Question { get; init; } = string.Empty;
    public List<PollOptionDto> Options { get; init; } = new();
    public int TotalVotes { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public Guid? UserVotedOptionId { get; init; }
}

public record PollOptionDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public int VoteCount { get; init; }
    public double Percentage { get; init; }
}

public record CreatePollDto
{
    public string Question { get; init; } = string.Empty;
    public List<string> Options { get; init; } = new();
}

public record VoteDto
{
    public Guid PollId { get; init; }
    public Guid OptionId { get; init; }
}

public record MessageDto
{
    public Guid Id { get; init; }
    public Guid SenderId { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public Guid RecipientId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record SendMessageDto
{
    public Guid RecipientId { get; init; }
    public string Content { get; init; } = string.Empty;
}

public record ConversationDto
{
    public Guid ParticipantId { get; init; }
    public string ParticipantName { get; init; } = string.Empty;
    public string? ParticipantAvatarUrl { get; init; }
    public string LastMessage { get; init; } = string.Empty;
    public DateTime LastMessageAt { get; init; }
    public int UnreadCount { get; init; }
}
