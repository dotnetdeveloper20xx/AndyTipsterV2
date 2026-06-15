using AndyTipster.Application.Tips.DTOs;
using AndyTipster.Application.Tips.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Domain.Exceptions;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services;

public class TipService : ITipService
{
    private readonly AndyTipsterDbContext _context;

    public TipService(AndyTipsterDbContext context)
    {
        _context = context;
    }

    public async Task<TipDto> CreateTipAsync(CreateTipDto dto, Guid userId)
    {
        var errors = ValidateCreateTip(dto);
        if (errors.Count > 0)
            throw new ValidationException(errors);

        var category = await _context.TipCategories.FindAsync(dto.CategoryId)
            ?? throw new NotFoundException("Category not found.");

        var tip = new Tip
        {
            Id = Guid.NewGuid(),
            EventDate = dto.EventDate,
            RaceName = dto.RaceName.Trim(),
            Selection = dto.Selection.Trim(),
            Odds = dto.Odds,
            Stake = dto.Stake,
            CategoryId = dto.CategoryId,
            Commentary = dto.Commentary?.Trim(),
            Status = TipStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.Tips.Add(tip);
        await _context.SaveChangesAsync();

        return MapToDto(tip, category.Name);
    }

    public async Task<TipDto> UpdateTipAsync(Guid tipId, UpdateTipDto dto)
    {
        var tip = await _context.Tips.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == tipId)
            ?? throw new NotFoundException("Tip not found.");

        if (tip.Status != TipStatus.Draft)
            throw new BusinessRuleException("Only draft tips can be edited.");

        var errors = new Dictionary<string, string[]>();

        if (dto.EventDate.HasValue)
            tip.EventDate = dto.EventDate.Value;

        if (dto.RaceName != null)
        {
            if (dto.RaceName.Length < 1 || dto.RaceName.Length > 200)
                errors["raceName"] = new[] { "Race name must be between 1 and 200 characters." };
            else
                tip.RaceName = dto.RaceName.Trim();
        }

        if (dto.Selection != null)
        {
            if (dto.Selection.Length < 1 || dto.Selection.Length > 200)
                errors["selection"] = new[] { "Selection must be between 1 and 200 characters." };
            else
                tip.Selection = dto.Selection.Trim();
        }

        if (dto.Odds.HasValue)
        {
            if (dto.Odds.Value < 1.01m || dto.Odds.Value > 1000.00m)
                errors["odds"] = new[] { "Odds must be between 1.01 and 1000.00." };
            else
                tip.Odds = dto.Odds.Value;
        }

        if (dto.Stake.HasValue)
        {
            if (dto.Stake.Value < 1 || dto.Stake.Value > 10)
                errors["stake"] = new[] { "Stake must be between 1 and 10." };
            else
                tip.Stake = dto.Stake.Value;
        }

        if (dto.CategoryId.HasValue)
        {
            var category = await _context.TipCategories.FindAsync(dto.CategoryId.Value);
            if (category == null)
                errors["categoryId"] = new[] { "Invalid category." };
            else
                tip.CategoryId = dto.CategoryId.Value;
        }

        if (dto.Commentary != null)
        {
            if (dto.Commentary.Length > 5000)
                errors["commentary"] = new[] { "Commentary must be at most 5000 characters." };
            else
                tip.Commentary = dto.Commentary.Trim();
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);

        await _context.SaveChangesAsync();

        var categoryName = tip.Category?.Name ?? (await _context.TipCategories.FindAsync(tip.CategoryId))?.Name ?? "";
        return MapToDto(tip, categoryName);
    }

    public async Task<TipDto> GetTipByIdAsync(Guid tipId)
    {
        var tip = await _context.Tips.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == tipId)
            ?? throw new NotFoundException("Tip not found.");
        return MapToDto(tip, tip.Category.Name);
    }

    public async Task<(List<TipDto> Items, int TotalCount)> GetTipsAsync(TipFilterDto filter)
    {
        var query = _context.Tips.Include(t => t.Category).AsQueryable();

        if (filter.StartDate.HasValue)
            query = query.Where(t => t.EventDate >= filter.StartDate.Value);
        if (filter.EndDate.HasValue)
            query = query.Where(t => t.EventDate <= filter.EndDate.Value);
        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        if (!string.IsNullOrEmpty(filter.Result) && Enum.TryParse<TipResult>(filter.Result, true, out var result))
            query = query.Where(t => t.Result == result);
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<TipStatus>(filter.Status, true, out var status))
            query = query.Where(t => t.Status == status);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items.Select(t => MapToDto(t, t.Category.Name)).ToList(), totalCount);
    }

    public async Task DeleteTipAsync(Guid tipId)
    {
        var tip = await _context.Tips.FindAsync(tipId)
            ?? throw new NotFoundException("Tip not found.");

        _context.Tips.Remove(tip);
        await _context.SaveChangesAsync();
    }

    public async Task<TipDto> PublishTipAsync(Guid tipId, DateTime? scheduledPublishAt)
    {
        var tip = await _context.Tips.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == tipId)
            ?? throw new NotFoundException("Tip not found.");

        if (tip.Status != TipStatus.Draft)
            throw new BusinessRuleException("Only draft tips can be published. Valid transitions: Draft → Published → Archived.");

        if (scheduledPublishAt.HasValue)
        {
            if (scheduledPublishAt.Value <= DateTime.UtcNow.AddMinutes(1))
                throw new BusinessRuleException("Scheduled publish time must be at least 1 minute in the future.");

            tip.ScheduledPublishAt = scheduledPublishAt.Value;
        }
        else
        {
            tip.Status = TipStatus.Published;
            tip.PublishedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return MapToDto(tip, tip.Category.Name);
    }

    public async Task<TipDto> ArchiveTipAsync(Guid tipId)
    {
        var tip = await _context.Tips.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == tipId)
            ?? throw new NotFoundException("Tip not found.");

        if (tip.Status != TipStatus.Published)
            throw new BusinessRuleException("Only published tips can be archived. Valid transitions: Draft → Published → Archived.");

        tip.Status = TipStatus.Archived;
        await _context.SaveChangesAsync();
        return MapToDto(tip, tip.Category.Name);
    }

    public async Task<TipDto> RecordResultAsync(Guid tipId, string resultStr)
    {
        var tip = await _context.Tips.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == tipId)
            ?? throw new NotFoundException("Tip not found.");

        if (tip.Status != TipStatus.Published)
            throw new BusinessRuleException("Results can only be recorded for published tips.");

        if (!Enum.TryParse<TipResult>(resultStr, true, out var result))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["result"] = new[] { "Result must be one of: Won, Lost, Void, Push." }
            });

        tip.Result = result;
        tip.ProfitLoss = CalculatePnL(tip.Odds, tip.Stake, result);
        await _context.SaveChangesAsync();

        return MapToDto(tip, tip.Category.Name);
    }

    public async Task<PnLSummaryDto> GetPnLSummaryAsync(DateTime? startDate, DateTime? endDate, Guid? categoryId, string groupBy = "month")
    {
        var query = _context.Tips
            .Include(t => t.Category)
            .Where(t => t.Result != null);

        if (startDate.HasValue)
            query = query.Where(t => t.EventDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(t => t.EventDate <= endDate.Value);
        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        var tips = await query.ToListAsync();

        var totalPnL = tips.Sum(t => t.ProfitLoss ?? 0);
        var won = tips.Count(t => t.Result == TipResult.Won);
        var lost = tips.Count(t => t.Result == TipResult.Lost);
        var voidCount = tips.Count(t => t.Result == TipResult.Void);
        var pushCount = tips.Count(t => t.Result == TipResult.Push);
        var decidedTips = won + lost;
        var strikeRate = decidedTips > 0 ? (decimal)won / decidedTips * 100 : 0;

        var periods = GroupByPeriod(tips, groupBy);
        var categories = tips
            .GroupBy(t => new { t.CategoryId, t.Category.Name })
            .Select(g => new PnLCategoryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                ProfitLoss = g.Sum(t => t.ProfitLoss ?? 0),
                TipCount = g.Count()
            }).ToList();

        return new PnLSummaryDto
        {
            TotalProfitLoss = totalPnL,
            TotalTips = tips.Count,
            Won = won,
            Lost = lost,
            Void = voidCount,
            Push = pushCount,
            StrikeRate = Math.Round(strikeRate, 2),
            Periods = periods,
            Categories = categories
        };
    }

    public async Task<BulkImportResultDto> BulkImportAsync(Stream csvStream, Guid userId)
    {
        using var reader = new StreamReader(csvStream);
        var content = await reader.ReadToEndAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length > 501) // 1 header + 500 data rows max
            throw new BusinessRuleException("CSV file exceeds maximum of 500 rows.");

        var errors = new List<BulkImportErrorDto>();
        var validTips = new List<Tip>();
        var categories = await _context.TipCategories.ToDictionaryAsync(c => c.Name.ToLowerInvariant(), c => c.Id);

        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Length < 6)
            {
                errors.Add(new BulkImportErrorDto { RowNumber = i + 1, Field = "row", Error = "Insufficient columns. Expected: EventDate, RaceName, Selection, Odds, Stake, Category [, Commentary]" });
                continue;
            }

            var rowErrors = new List<BulkImportErrorDto>();

            if (!DateTime.TryParse(fields[0].Trim(), out var eventDate))
                rowErrors.Add(new BulkImportErrorDto { RowNumber = i + 1, Field = "eventDate", Error = "Invalid date format." });

            var raceName = fields[1].Trim();
            if (raceName.Length < 1 || raceName.Length > 200)
                rowErrors.Add(new BulkImportErrorDto { RowNumber = i + 1, Field = "raceName", Error = "Race name must be between 1 and 200 characters." });

            var selection = fields[2].Trim();
            if (selection.Length < 1 || selection.Length > 200)
                rowErrors.Add(new BulkImportErrorDto { RowNumber = i + 1, Field = "selection", Error = "Selection must be between 1 and 200 characters." });

            if (!decimal.TryParse(fields[3].Trim(), out var odds) || odds < 1.01m || odds > 1000.00m)
                rowErrors.Add(new BulkImportErrorDto { RowNumber = i + 1, Field = "odds", Error = "Odds must be between 1.01 and 1000.00." });

            if (!int.TryParse(fields[4].Trim(), out var stake) || stake < 1 || stake > 10)
                rowErrors.Add(new BulkImportErrorDto { RowNumber = i + 1, Field = "stake", Error = "Stake must be between 1 and 10." });

            var categoryName = fields[5].Trim().ToLowerInvariant();
            Guid categoryId = Guid.Empty;
            if (!categories.TryGetValue(categoryName, out categoryId))
                rowErrors.Add(new BulkImportErrorDto { RowNumber = i + 1, Field = "category", Error = $"Category '{fields[5].Trim()}' not found." });

            var commentary = fields.Length > 6 ? fields[6].Trim() : null;
            if (commentary != null && commentary.Length > 5000)
                rowErrors.Add(new BulkImportErrorDto { RowNumber = i + 1, Field = "commentary", Error = "Commentary must be at most 5000 characters." });

            if (rowErrors.Count > 0)
            {
                errors.AddRange(rowErrors);
                continue;
            }

            validTips.Add(new Tip
            {
                Id = Guid.NewGuid(),
                EventDate = eventDate,
                RaceName = raceName,
                Selection = selection,
                Odds = odds,
                Stake = stake,
                CategoryId = categoryId,
                Commentary = commentary,
                Status = TipStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            });
        }

        if (validTips.Count > 0)
        {
            _context.Tips.AddRange(validTips);
            await _context.SaveChangesAsync();
        }

        return new BulkImportResultDto
        {
            TotalRows = lines.Length - 1,
            SuccessCount = validTips.Count,
            ErrorCount = errors.Select(e => e.RowNumber).Distinct().Count(),
            Errors = errors
        };
    }

    private static decimal CalculatePnL(decimal odds, int stake, TipResult result)
    {
        return result switch
        {
            TipResult.Won => (odds * stake) - stake,
            TipResult.Lost => -stake,
            TipResult.Void => 0,
            TipResult.Push => 0,
            _ => 0
        };
    }

    private static List<PnLPeriodDto> GroupByPeriod(List<Tip> tips, string groupBy)
    {
        return groupBy.ToLowerInvariant() switch
        {
            "day" => tips.GroupBy(t => t.EventDate.Date.ToString("yyyy-MM-dd"))
                .Select(g => new PnLPeriodDto { Period = g.Key, ProfitLoss = g.Sum(t => t.ProfitLoss ?? 0), TipCount = g.Count() })
                .OrderBy(p => p.Period).ToList(),
            "week" => tips.GroupBy(t => $"{t.EventDate.Year}-W{GetIso8601WeekOfYear(t.EventDate):D2}")
                .Select(g => new PnLPeriodDto { Period = g.Key, ProfitLoss = g.Sum(t => t.ProfitLoss ?? 0), TipCount = g.Count() })
                .OrderBy(p => p.Period).ToList(),
            "year" => tips.GroupBy(t => t.EventDate.Year.ToString())
                .Select(g => new PnLPeriodDto { Period = g.Key, ProfitLoss = g.Sum(t => t.ProfitLoss ?? 0), TipCount = g.Count() })
                .OrderBy(p => p.Period).ToList(),
            _ => tips.GroupBy(t => t.EventDate.ToString("yyyy-MM"))
                .Select(g => new PnLPeriodDto { Period = g.Key, ProfitLoss = g.Sum(t => t.ProfitLoss ?? 0), TipCount = g.Count() })
                .OrderBy(p => p.Period).ToList(),
        };
    }

    private static int GetIso8601WeekOfYear(DateTime date)
    {
        var day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            date = date.AddDays(3);
        return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    private static Dictionary<string, string[]> ValidateCreateTip(CreateTipDto dto)
    {
        var errors = new Dictionary<string, string[]>();

        if (dto.EventDate == default)
            errors["eventDate"] = new[] { "Event date is required." };

        if (string.IsNullOrWhiteSpace(dto.RaceName) || dto.RaceName.Length > 200)
            errors["raceName"] = new[] { "Race name must be between 1 and 200 characters." };

        if (string.IsNullOrWhiteSpace(dto.Selection) || dto.Selection.Length > 200)
            errors["selection"] = new[] { "Selection must be between 1 and 200 characters." };

        if (dto.Odds < 1.01m || dto.Odds > 1000.00m)
            errors["odds"] = new[] { "Odds must be between 1.01 and 1000.00." };

        if (dto.Stake < 1 || dto.Stake > 10)
            errors["stake"] = new[] { "Stake must be between 1 and 10." };

        if (dto.CategoryId == Guid.Empty)
            errors["categoryId"] = new[] { "Category is required." };

        if (dto.Commentary != null && dto.Commentary.Length > 5000)
            errors["commentary"] = new[] { "Commentary must be at most 5000 characters." };

        return errors;
    }

    private static TipDto MapToDto(Tip tip, string categoryName)
    {
        return new TipDto
        {
            Id = tip.Id,
            EventDate = tip.EventDate,
            RaceName = tip.RaceName,
            Selection = tip.Selection,
            Odds = tip.Odds,
            Stake = tip.Stake,
            CategoryId = tip.CategoryId,
            CategoryName = categoryName,
            Commentary = tip.Commentary,
            Status = tip.Status.ToString(),
            Result = tip.Result?.ToString(),
            ProfitLoss = tip.ProfitLoss,
            CreatedAt = tip.CreatedAt,
            PublishedAt = tip.PublishedAt,
            ScheduledPublishAt = tip.ScheduledPublishAt,
            CreatedByUserId = tip.CreatedByUserId
        };
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = "";
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (line[i] == ',' && !inQuotes)
            {
                fields.Add(current);
                current = "";
            }
            else
            {
                current += line[i];
            }
        }
        fields.Add(current);
        return fields.ToArray();
    }
}
