using AndyTipster.Application.Community.DTOs;
using AndyTipster.Application.Community.Services;
using AndyTipster.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

public class PollService : IPollService
{
    private readonly ILogger<PollService> _logger;

    // In-memory poll store (would be DB tables in production)
    private static readonly List<PollEntity> _polls = new();
    private static readonly Dictionary<(Guid PollId, Guid UserId), Guid> _votes = new();
    private static readonly object _lock = new();

    public PollService(ILogger<PollService> logger)
    {
        _logger = logger;
    }

    public Task<PollDto> CreatePollAsync(CreatePollDto dto, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(dto.Question))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["question"] = new[] { "Question is required." }
            });

        if (dto.Options.Count < 2)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["options"] = new[] { "At least 2 options are required." }
            });

        var poll = new PollEntity
        {
            Id = Guid.NewGuid(),
            Question = dto.Question.Trim(),
            Options = dto.Options.Select(o => new PollOptionEntity
            {
                Id = Guid.NewGuid(),
                Text = o.Trim(),
                VoteCount = 0
            }).ToList(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        lock (_lock)
        {
            _polls.Add(poll);
        }

        return Task.FromResult(MapToDto(poll, null));
    }

    public Task<PollDto> GetPollAsync(Guid pollId, Guid? userId = null)
    {
        lock (_lock)
        {
            var poll = _polls.FirstOrDefault(p => p.Id == pollId)
                ?? throw new NotFoundException("Poll not found.");

            return Task.FromResult(MapToDto(poll, userId));
        }
    }

    public Task<List<PollDto>> GetActivePollsAsync(Guid? userId = null)
    {
        lock (_lock)
        {
            var polls = _polls
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => MapToDto(p, userId))
                .ToList();

            return Task.FromResult(polls);
        }
    }

    public Task<PollDto> VoteAsync(VoteDto dto, Guid userId)
    {
        lock (_lock)
        {
            var poll = _polls.FirstOrDefault(p => p.Id == dto.PollId)
                ?? throw new NotFoundException("Poll not found.");

            if (!poll.IsActive)
                throw new BusinessRuleException("This poll is closed.");

            var key = (dto.PollId, userId);
            if (_votes.ContainsKey(key))
                throw new BusinessRuleException("You have already voted on this poll.");

            var option = poll.Options.FirstOrDefault(o => o.Id == dto.OptionId)
                ?? throw new NotFoundException("Poll option not found.");

            option.VoteCount++;
            _votes[key] = dto.OptionId;

            return Task.FromResult(MapToDto(poll, userId));
        }
    }

    public Task ClosePollAsync(Guid pollId)
    {
        lock (_lock)
        {
            var poll = _polls.FirstOrDefault(p => p.Id == pollId)
                ?? throw new NotFoundException("Poll not found.");

            poll.IsActive = false;
            poll.ClosedAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    private PollDto MapToDto(PollEntity poll, Guid? userId)
    {
        var totalVotes = poll.Options.Sum(o => o.VoteCount);
        Guid? userVotedOptionId = null;

        if (userId.HasValue)
        {
            var key = (poll.Id, userId.Value);
            if (_votes.TryGetValue(key, out var votedOptionId))
                userVotedOptionId = votedOptionId;
        }

        return new PollDto
        {
            Id = poll.Id,
            Question = poll.Question,
            Options = poll.Options.Select(o => new PollOptionDto
            {
                Id = o.Id,
                Text = o.Text,
                VoteCount = o.VoteCount,
                Percentage = totalVotes > 0 ? Math.Round((double)o.VoteCount / totalVotes * 100, 1) : 0
            }).ToList(),
            TotalVotes = totalVotes,
            IsActive = poll.IsActive,
            CreatedAt = poll.CreatedAt,
            ClosedAt = poll.ClosedAt,
            UserVotedOptionId = userVotedOptionId
        };
    }

    private class PollEntity
    {
        public Guid Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<PollOptionEntity> Options { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
    }

    private class PollOptionEntity
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int VoteCount { get; set; }
    }
}
