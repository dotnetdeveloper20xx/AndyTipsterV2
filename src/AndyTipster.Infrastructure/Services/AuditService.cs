using AndyTipster.Application.Audit.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services;

/// <summary>
/// Append-only audit logging service. Logs all admin actions with actor, target, action type,
/// timestamp, and before/after values. No UPDATE or DELETE operations are exposed.
/// Configured with 2-year retention policy (handled at database level).
/// </summary>
public class AuditService : IAuditService
{
    private readonly AndyTipsterDbContext _context;

    public AuditService(AndyTipsterDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task LogActionAsync(
        Guid actorUserId,
        string actionType,
        string targetEntity,
        string? targetEntityId = null,
        string? beforeJson = null,
        string? afterJson = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            ActorUserId = actorUserId,
            ActionType = actionType,
            TargetEntity = targetEntity,
            TargetEntityId = targetEntityId,
            BeforeJson = beforeJson,
            AfterJson = afterJson,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuditLogResponse> GetAuditLogsAsync(AuditLogRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .Include(a => a.ActorUser)
            .AsNoTracking()
            .AsQueryable();

        // Apply search filter (searches action type, target entity, and actor name)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(a =>
                a.ActionType.ToLower().Contains(search) ||
                a.TargetEntity.ToLower().Contains(search) ||
                a.ActorUser.DisplayName.ToLower().Contains(search) ||
                (a.TargetEntityId != null && a.TargetEntityId.ToLower().Contains(search)));
        }

        // Apply action type filter
        if (!string.IsNullOrWhiteSpace(request.ActionTypeFilter))
        {
            query = query.Where(a => a.ActionType == request.ActionTypeFilter);
        }

        // Apply actor filter
        if (!string.IsNullOrWhiteSpace(request.ActorFilter))
        {
            if (Guid.TryParse(request.ActorFilter, out var actorId))
            {
                query = query.Where(a => a.ActorUserId == actorId);
            }
        }

        // Apply target entity filter
        if (!string.IsNullOrWhiteSpace(request.TargetEntityFilter))
        {
            query = query.Where(a => a.TargetEntity == request.TargetEntityFilter);
        }

        // Apply date range filter
        if (request.DateFrom.HasValue)
        {
            query = query.Where(a => a.Timestamp >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(a => a.Timestamp <= request.DateTo.Value);
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "actiontype" => request.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(a => a.ActionType)
                : query.OrderByDescending(a => a.ActionType),
            "targetentity" => request.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(a => a.TargetEntity)
                : query.OrderByDescending(a => a.TargetEntity),
            _ => request.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(a => a.Timestamp)
                : query.OrderByDescending(a => a.Timestamp),
        };

        // Apply pagination
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var page = Math.Max(request.Page, 1);
        var entries = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogEntryDto
            {
                Id = a.Id,
                ActorUserId = a.ActorUserId,
                ActorName = a.ActorUser.DisplayName,
                ActionType = a.ActionType,
                TargetEntity = a.TargetEntity,
                TargetEntityId = a.TargetEntityId,
                BeforeJson = a.BeforeJson,
                AfterJson = a.AfterJson,
                Timestamp = a.Timestamp,
                IpAddress = a.IpAddress
            })
            .ToListAsync(cancellationToken);

        return new AuditLogResponse
        {
            Entries = entries,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
