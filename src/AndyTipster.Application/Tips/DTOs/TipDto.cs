namespace AndyTipster.Application.Tips.DTOs;

public record TipDto
{
    public Guid Id { get; init; }
    public DateTime EventDate { get; init; }
    public string RaceName { get; init; } = string.Empty;
    public string Selection { get; init; } = string.Empty;
    public decimal Odds { get; init; }
    public int Stake { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string? Commentary { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Result { get; init; }
    public decimal? ProfitLoss { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ScheduledPublishAt { get; init; }
    public Guid CreatedByUserId { get; init; }
}

public record CreateTipDto
{
    public DateTime EventDate { get; init; }
    public string RaceName { get; init; } = string.Empty;
    public string Selection { get; init; } = string.Empty;
    public decimal Odds { get; init; }
    public int Stake { get; init; }
    public Guid CategoryId { get; init; }
    public string? Commentary { get; init; }
}

public record UpdateTipDto
{
    public DateTime? EventDate { get; init; }
    public string? RaceName { get; init; }
    public string? Selection { get; init; }
    public decimal? Odds { get; init; }
    public int? Stake { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Commentary { get; init; }
}

public record PublishTipRequest
{
    public DateTime? ScheduledPublishAt { get; init; }
}

public record RecordResultRequest
{
    public string Result { get; init; } = string.Empty;
}

public record TipFilterDto
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Result { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}

public record PnLSummaryDto
{
    public decimal TotalProfitLoss { get; init; }
    public int TotalTips { get; init; }
    public int Won { get; init; }
    public int Lost { get; init; }
    public int Void { get; init; }
    public int Push { get; init; }
    public decimal StrikeRate { get; init; }
    public List<PnLPeriodDto> Periods { get; init; } = new();
    public List<PnLCategoryDto> Categories { get; init; } = new();
}

public record PnLPeriodDto
{
    public string Period { get; init; } = string.Empty;
    public decimal ProfitLoss { get; init; }
    public int TipCount { get; init; }
}

public record PnLCategoryDto
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal ProfitLoss { get; init; }
    public int TipCount { get; init; }
}

public record BulkImportResultDto
{
    public int TotalRows { get; init; }
    public int SuccessCount { get; init; }
    public int ErrorCount { get; init; }
    public List<BulkImportErrorDto> Errors { get; init; } = new();
}

public record BulkImportErrorDto
{
    public int RowNumber { get; init; }
    public string Field { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
}

public record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateCategoryDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record UpdateCategoryDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsActive { get; init; }
}
