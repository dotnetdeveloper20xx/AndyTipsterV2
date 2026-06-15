using AndyTipster.Application.HelpBot.DTOs;
using AndyTipster.Application.HelpBot.Services;
using AndyTipster.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

public class HelpBotService : IHelpBotService
{
    private readonly ILogger<HelpBotService> _logger;

    // In-memory conversation and flow store
    private static readonly List<FlowEntity> _flows = new();
    private static readonly Dictionary<Guid, ConversationEntity> _conversations = new();
    private static readonly object _lock = new();

    // Default welcome flow
    static HelpBotService()
    {
        _flows.Add(new FlowEntity
        {
            Id = Guid.NewGuid(),
            Name = "Welcome",
            TriggerKeyword = "__welcome__",
            IsActive = true,
            Priority = 0,
            CreatedAt = DateTime.UtcNow,
            Steps = new List<FlowStepEntity>
            {
                new() { Id = Guid.NewGuid(), Order = 1, Type = "message", Content = "👋 Welcome to AndyTipster! How can I help you today?" },
                new() { Id = Guid.NewGuid(), Order = 2, Type = "quick-reply", Content = "Choose a topic:", Options = new List<string> { "Subscription", "Tips", "Account", "Other" } }
            }
        });

        _flows.Add(new FlowEntity
        {
            Id = Guid.NewGuid(),
            Name = "Subscription Help",
            TriggerKeyword = "subscription",
            IsActive = true,
            Priority = 1,
            CreatedAt = DateTime.UtcNow,
            Steps = new List<FlowStepEntity>
            {
                new() { Id = Guid.NewGuid(), Order = 1, Type = "message", Content = "I can help with subscription questions! What would you like to know?" },
                new() { Id = Guid.NewGuid(), Order = 2, Type = "quick-reply", Content = "Select an option:", Options = new List<string> { "Pricing", "Cancel", "Upgrade", "Talk to support" } }
            }
        });

        _flows.Add(new FlowEntity
        {
            Id = Guid.NewGuid(),
            Name = "Tips Help",
            TriggerKeyword = "tips",
            IsActive = true,
            Priority = 1,
            CreatedAt = DateTime.UtcNow,
            Steps = new List<FlowStepEntity>
            {
                new() { Id = Guid.NewGuid(), Order = 1, Type = "message", Content = "Need help with tips? Here are some common questions:" },
                new() { Id = Guid.NewGuid(), Order = 2, Type = "quick-reply", Content = "Select a topic:", Options = new List<string> { "How tips work", "Results", "Categories", "Talk to support" } }
            }
        });
    }

    public HelpBotService(ILogger<HelpBotService> logger)
    {
        _logger = logger;
    }

    public Task<BotResponseDto> ProcessMessageAsync(UserMessageDto dto, Guid? userId)
    {
        lock (_lock)
        {
            var sessionId = dto.SessionId ?? Guid.NewGuid();

            // Get or create conversation
            if (!_conversations.TryGetValue(sessionId, out var conversation))
            {
                conversation = new ConversationEntity
                {
                    SessionId = sessionId,
                    UserId = userId,
                    StartedAt = DateTime.UtcNow,
                    Messages = new List<ChatMessageEntity>()
                };
                _conversations[sessionId] = conversation;

                // Send welcome message for new conversations
                if (string.IsNullOrEmpty(dto.Message))
                {
                    var welcomeFlow = _flows.FirstOrDefault(f => f.TriggerKeyword == "__welcome__" && f.IsActive);
                    if (welcomeFlow != null)
                    {
                        var welcomeStep = welcomeFlow.Steps.OrderBy(s => s.Order).FirstOrDefault();
                        var quickReplyStep = welcomeFlow.Steps.OrderBy(s => s.Order).Skip(1).FirstOrDefault();

                        var botMsg = new ChatMessageEntity
                        {
                            Id = Guid.NewGuid(),
                            Sender = "bot",
                            Content = welcomeStep?.Content ?? "Hello! How can I help?",
                            QuickReplies = quickReplyStep?.Options,
                            Timestamp = DateTime.UtcNow
                        };
                        conversation.Messages.Add(botMsg);

                        return Task.FromResult(new BotResponseDto
                        {
                            SessionId = sessionId,
                            Message = botMsg.Content,
                            QuickReplies = botMsg.QuickReplies,
                            IsEscalated = false
                        });
                    }
                }
            }

            // Add user message
            conversation.Messages.Add(new ChatMessageEntity
            {
                Id = Guid.NewGuid(),
                Sender = "user",
                Content = dto.Message,
                Timestamp = DateTime.UtcNow
            });

            // Check for escalation keywords
            if (dto.Message.Contains("support", StringComparison.OrdinalIgnoreCase) ||
                dto.Message.Contains("talk to", StringComparison.OrdinalIgnoreCase) ||
                dto.Message.Contains("human", StringComparison.OrdinalIgnoreCase))
            {
                conversation.IsEscalated = true;
                var escalationMsg = new ChatMessageEntity
                {
                    Id = Guid.NewGuid(),
                    Sender = "bot",
                    Content = "I'll connect you with our support team. A support ticket has been created and someone will get back to you shortly.",
                    Timestamp = DateTime.UtcNow
                };
                conversation.Messages.Add(escalationMsg);

                return Task.FromResult(new BotResponseDto
                {
                    SessionId = sessionId,
                    Message = escalationMsg.Content,
                    IsEscalated = true
                });
            }

            // Match keyword to flow
            var matchedFlow = _flows
                .Where(f => f.IsActive && f.TriggerKeyword != "__welcome__")
                .OrderByDescending(f => f.Priority)
                .FirstOrDefault(f => dto.Message.Contains(f.TriggerKeyword, StringComparison.OrdinalIgnoreCase));

            if (matchedFlow != null)
            {
                var step = matchedFlow.Steps.OrderBy(s => s.Order).FirstOrDefault();
                var quickReplyStep = matchedFlow.Steps.OrderBy(s => s.Order).Skip(1).FirstOrDefault();

                var response = step?.Content ?? "How can I help you with that?";
                var quickReplies = quickReplyStep?.Type == "quick-reply" ? quickReplyStep.Options : null;

                var botReply = new ChatMessageEntity
                {
                    Id = Guid.NewGuid(),
                    Sender = "bot",
                    Content = response,
                    QuickReplies = quickReplies,
                    Timestamp = DateTime.UtcNow
                };
                conversation.Messages.Add(botReply);

                return Task.FromResult(new BotResponseDto
                {
                    SessionId = sessionId,
                    Message = botReply.Content,
                    QuickReplies = botReply.QuickReplies,
                    IsEscalated = false
                });
            }

            // Default response
            var defaultReply = new ChatMessageEntity
            {
                Id = Guid.NewGuid(),
                Sender = "bot",
                Content = "I'm not sure I understand. Could you try rephrasing, or would you like to speak with our support team?",
                QuickReplies = new List<string> { "Subscription", "Tips", "Account", "Talk to support" },
                Timestamp = DateTime.UtcNow
            };
            conversation.Messages.Add(defaultReply);

            return Task.FromResult(new BotResponseDto
            {
                SessionId = sessionId,
                Message = defaultReply.Content,
                QuickReplies = defaultReply.QuickReplies,
                IsEscalated = false
            });
        }
    }

    public Task<ConversationDto> GetConversationAsync(Guid sessionId)
    {
        lock (_lock)
        {
            if (!_conversations.TryGetValue(sessionId, out var conversation))
                throw new NotFoundException("Conversation not found.");

            return Task.FromResult(new ConversationDto
            {
                SessionId = conversation.SessionId,
                Messages = conversation.Messages.Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Sender = m.Sender,
                    Content = m.Content,
                    QuickReplies = m.QuickReplies,
                    Timestamp = m.Timestamp
                }).ToList(),
                StartedAt = conversation.StartedAt,
                EndedAt = conversation.EndedAt,
                IsEscalated = conversation.IsEscalated
            });
        }
    }

    public Task<List<FlowDto>> GetFlowsAsync()
    {
        lock (_lock)
        {
            var flows = _flows.Select(f => new FlowDto
            {
                Id = f.Id,
                Name = f.Name,
                TriggerKeyword = f.TriggerKeyword,
                Steps = f.Steps.Select(s => new FlowStepDto
                {
                    Id = s.Id,
                    Order = s.Order,
                    Type = s.Type,
                    Content = s.Content,
                    Options = s.Options
                }).ToList(),
                IsActive = f.IsActive,
                Priority = f.Priority,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            }).ToList();

            return Task.FromResult(flows);
        }
    }

    public Task<FlowDto> CreateFlowAsync(CreateFlowDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["name"] = new[] { "Flow name is required." }
            });

        var flow = new FlowEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            TriggerKeyword = dto.TriggerKeyword.Trim().ToLowerInvariant(),
            Steps = dto.Steps.Select(s => new FlowStepEntity
            {
                Id = s.Id == Guid.Empty ? Guid.NewGuid() : s.Id,
                Order = s.Order,
                Type = s.Type,
                Content = s.Content,
                Options = s.Options
            }).ToList(),
            IsActive = true,
            Priority = dto.Priority,
            CreatedAt = DateTime.UtcNow
        };

        lock (_lock)
        {
            _flows.Add(flow);
        }

        _logger.LogInformation("Help bot flow created: {Name} (keyword: {Keyword})", dto.Name, dto.TriggerKeyword);

        return Task.FromResult(MapFlowToDto(flow));
    }

    public Task<FlowDto> UpdateFlowAsync(Guid flowId, UpdateFlowDto dto)
    {
        lock (_lock)
        {
            var flow = _flows.FirstOrDefault(f => f.Id == flowId)
                ?? throw new NotFoundException("Flow not found.");

            if (dto.Name != null) flow.Name = dto.Name.Trim();
            if (dto.TriggerKeyword != null) flow.TriggerKeyword = dto.TriggerKeyword.Trim().ToLowerInvariant();
            if (dto.IsActive.HasValue) flow.IsActive = dto.IsActive.Value;
            if (dto.Priority.HasValue) flow.Priority = dto.Priority.Value;
            if (dto.Steps != null)
            {
                flow.Steps = dto.Steps.Select(s => new FlowStepEntity
                {
                    Id = s.Id == Guid.Empty ? Guid.NewGuid() : s.Id,
                    Order = s.Order,
                    Type = s.Type,
                    Content = s.Content,
                    Options = s.Options
                }).ToList();
            }

            flow.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult(MapFlowToDto(flow));
        }
    }

    public Task DeleteFlowAsync(Guid flowId)
    {
        lock (_lock)
        {
            var flow = _flows.FirstOrDefault(f => f.Id == flowId)
                ?? throw new NotFoundException("Flow not found.");

            _flows.Remove(flow);
        }

        return Task.CompletedTask;
    }

    public Task EscalateToTicketAsync(Guid sessionId, string summary)
    {
        lock (_lock)
        {
            if (_conversations.TryGetValue(sessionId, out var conversation))
            {
                conversation.IsEscalated = true;
                conversation.EndedAt = DateTime.UtcNow;
            }
        }

        _logger.LogInformation("Help bot conversation {SessionId} escalated to support ticket: {Summary}", sessionId, summary);
        return Task.CompletedTask;
    }

    private static FlowDto MapFlowToDto(FlowEntity flow)
    {
        return new FlowDto
        {
            Id = flow.Id,
            Name = flow.Name,
            TriggerKeyword = flow.TriggerKeyword,
            Steps = flow.Steps.Select(s => new FlowStepDto
            {
                Id = s.Id,
                Order = s.Order,
                Type = s.Type,
                Content = s.Content,
                Options = s.Options
            }).ToList(),
            IsActive = flow.IsActive,
            Priority = flow.Priority,
            CreatedAt = flow.CreatedAt,
            UpdatedAt = flow.UpdatedAt
        };
    }

    private class FlowEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TriggerKeyword { get; set; } = string.Empty;
        public List<FlowStepEntity> Steps { get; set; } = new();
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    private class FlowStepEntity
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string>? Options { get; set; }
    }

    private class ConversationEntity
    {
        public Guid SessionId { get; set; }
        public Guid? UserId { get; set; }
        public List<ChatMessageEntity> Messages { get; set; } = new();
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsEscalated { get; set; }
    }

    private class ChatMessageEntity
    {
        public Guid Id { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string>? QuickReplies { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
