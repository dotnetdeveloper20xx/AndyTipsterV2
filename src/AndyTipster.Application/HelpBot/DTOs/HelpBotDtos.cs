namespace AndyTipster.Application.HelpBot.DTOs;

public record FlowDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TriggerKeyword { get; init; } = string.Empty;
    public List<FlowStepDto> Steps { get; init; } = new();
    public bool IsActive { get; init; }
    public int Priority { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record FlowStepDto
{
    public Guid Id { get; init; }
    public int Order { get; init; }
    public string Type { get; init; } = string.Empty; // message, quick-reply, escalation
    public string Content { get; init; } = string.Empty;
    public List<string>? Options { get; init; }
    public string? NextStepOnSelect { get; init; }
}

public record CreateFlowDto
{
    public string Name { get; init; } = string.Empty;
    public string TriggerKeyword { get; init; } = string.Empty;
    public List<FlowStepDto> Steps { get; init; } = new();
    public int Priority { get; init; }
}

public record UpdateFlowDto
{
    public string? Name { get; init; }
    public string? TriggerKeyword { get; init; }
    public List<FlowStepDto>? Steps { get; init; }
    public bool? IsActive { get; init; }
    public int? Priority { get; init; }
}

public record ConversationDto
{
    public Guid SessionId { get; init; }
    public List<ChatMessageDto> Messages { get; init; } = new();
    public DateTime StartedAt { get; init; }
    public DateTime? EndedAt { get; init; }
    public bool IsEscalated { get; init; }
}

public record ChatMessageDto
{
    public Guid Id { get; init; }
    public string Sender { get; init; } = string.Empty; // "bot" or "user"
    public string Content { get; init; } = string.Empty;
    public List<string>? QuickReplies { get; init; }
    public DateTime Timestamp { get; init; }
}

public record UserMessageDto
{
    public string Message { get; init; } = string.Empty;
    public Guid? SessionId { get; init; }
}

public record BotResponseDto
{
    public Guid SessionId { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string>? QuickReplies { get; init; }
    public bool IsEscalated { get; init; }
}
