using AndyTipster.Application.GDPR.DTOs;
using AndyTipster.Application.GDPR.Services;
using AndyTipster.Application.Notifications.Services;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace AndyTipster.Infrastructure.Services;

public class GdprService : IGdprService
{
    private readonly AndyTipsterDbContext _db;
    private readonly INotificationService _notificationService;

    public GdprService(AndyTipsterDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    // === Data Export (Right to Access) ===

    public async Task<DataExportStatusDto> RequestDataExportAsync(Guid userId, DataExportRequestDto request)
    {
        var exportRequest = new Domain.Entities.DataExportRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Format = request.Format,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };

        _db.DataExportRequests.Add(exportRequest);
        await _db.SaveChangesAsync();

        // In production, this would be handled by a background job
        // For now, process inline
        await ProcessDataExportAsync(exportRequest);

        return new DataExportStatusDto
        {
            RequestId = exportRequest.Id,
            Status = exportRequest.Status,
            RequestedAt = exportRequest.RequestedAt,
            CompletedAt = exportRequest.CompletedAt,
            ExpiresAt = exportRequest.ExpiresAt,
            DownloadUrl = exportRequest.DownloadUrl
        };
    }

    public async Task<DataExportStatusDto> GetExportStatusAsync(Guid userId, Guid requestId)
    {
        var exportRequest = await _db.DataExportRequests
            .FirstOrDefaultAsync(e => e.Id == requestId && e.UserId == userId)
            ?? throw new InvalidOperationException("Export request not found.");

        return new DataExportStatusDto
        {
            RequestId = exportRequest.Id,
            Status = exportRequest.Status,
            RequestedAt = exportRequest.RequestedAt,
            CompletedAt = exportRequest.CompletedAt,
            ExpiresAt = exportRequest.ExpiresAt,
            DownloadUrl = exportRequest.Status == "Ready" ? exportRequest.DownloadUrl : null
        };
    }

    public async Task<byte[]?> DownloadExportAsync(Guid userId, Guid requestId)
    {
        var exportRequest = await _db.DataExportRequests
            .FirstOrDefaultAsync(e => e.Id == requestId && e.UserId == userId && e.Status == "Ready")
            ?? throw new InvalidOperationException("Export not ready or not found.");

        if (exportRequest.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Export download link has expired.");

        // Generate the export data
        return await GenerateExportDataAsync(userId, exportRequest.Format);
    }

    // === Account Deletion (Right to Erasure) ===

    public async Task<AccountDeletionStatusDto> RequestAccountDeletionAsync(Guid userId, AccountDeletionRequestDto request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        var scheduledDate = DateTime.UtcNow.AddDays(30);

        user.IsMarkedForDeletion = true;
        user.DeletionScheduledAt = scheduledDate;
        user.DeletionReason = request.Reason;

        await _db.SaveChangesAsync();

        // Send confirmation email
        await _notificationService.SendNotificationAsync(new Application.Notifications.DTOs.SendNotificationDto
        {
            UserId = userId,
            Type = "GdprDeletion",
            Title = "Account Deletion Scheduled",
            Body = $"Your account is scheduled for permanent deletion on {scheduledDate:dd MMM yyyy}. " +
                   "You can cancel this by logging in before that date."
        });

        return new AccountDeletionStatusDto
        {
            UserId = userId,
            Status = "scheduled",
            ScheduledDeletionDate = scheduledDate,
            CanCancel = true
        };
    }

    public async Task<AccountDeletionStatusDto> GetDeletionStatusAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        if (!user.IsMarkedForDeletion)
            return new AccountDeletionStatusDto { UserId = userId, Status = "none", CanCancel = false };

        return new AccountDeletionStatusDto
        {
            UserId = userId,
            Status = "scheduled",
            ScheduledDeletionDate = user.DeletionScheduledAt ?? DateTime.UtcNow,
            CanCancel = user.DeletionScheduledAt > DateTime.UtcNow
        };
    }

    public async Task<AccountDeletionStatusDto> CancelDeletionAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        if (!user.IsMarkedForDeletion)
            throw new InvalidOperationException("No deletion request found.");

        if (user.DeletionScheduledAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Deletion grace period has expired. Cannot cancel.");

        user.IsMarkedForDeletion = false;
        user.DeletionScheduledAt = null;
        user.DeletionReason = null;

        await _db.SaveChangesAsync();

        return new AccountDeletionStatusDto
        {
            UserId = userId,
            Status = "cancelled",
            CancelledAt = DateTime.UtcNow,
            CanCancel = false
        };
    }

    public async Task PurgeSoftDeletedAccountsAsync()
    {
        var usersToDelete = await _db.Users
            .Where(u => u.IsMarkedForDeletion && u.DeletionScheduledAt <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var user in usersToDelete)
        {
            // Remove associated data
            var subscriptions = await _db.Subscriptions.Where(s => s.UserId == user.Id).ToListAsync();
            var subscriptionIds = subscriptions.Select(s => s.Id).ToList();
            _db.Subscriptions.RemoveRange(subscriptions);

            var payments = await _db.Payments.Where(p => subscriptionIds.Contains(p.SubscriptionId)).ToListAsync();
            _db.Payments.RemoveRange(payments);

            var consents = await _db.GdprConsents.Where(c => c.UserId == user.Id).ToListAsync();
            _db.GdprConsents.RemoveRange(consents);

            var notifications = await _db.Notifications.Where(n => n.UserId == user.Id).ToListAsync();
            _db.Notifications.RemoveRange(notifications);

            var comments = await _db.Comments.Where(c => c.UserId == user.Id).ToListAsync();
            _db.Comments.RemoveRange(comments);

            _db.Users.Remove(user);
        }

        await _db.SaveChangesAsync();
    }

    // === Right to Rectification ===

    public async Task RectifyPersonalDataAsync(Guid userId, RectificationRequestDto request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        if (!string.IsNullOrEmpty(request.DisplayName))
            user.DisplayName = request.DisplayName;
        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email;
        if (request.Bio != null)
            user.Bio = request.Bio;
        if (!string.IsNullOrEmpty(request.Timezone))
            user.Timezone = request.Timezone;

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // === Consent Management ===

    public async Task<List<ConsentRecordDto>> GetConsentRecordsAsync(Guid userId)
    {
        return await _db.GdprConsents
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.GrantedAt)
            .Select(c => new ConsentRecordDto
            {
                Id = c.Id,
                ConsentType = c.ConsentType,
                IsGranted = c.IsGranted,
                GrantedAt = c.GrantedAt,
                RevokedAt = c.RevokedAt,
                IpAddress = c.IpAddress
            })
            .ToListAsync();
    }

    public async Task RecordConsentAsync(Guid userId, string consentType, bool isGranted, string? ipAddress, string? userAgent)
    {
        // Revoke previous consent of same type if exists
        var existing = await _db.GdprConsents
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ConsentType == consentType && c.RevokedAt == null);

        if (existing != null)
            existing.RevokedAt = DateTime.UtcNow;

        var consent = new Domain.Entities.GdprConsent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConsentType = consentType,
            IsGranted = isGranted,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            GrantedAt = DateTime.UtcNow
        };

        _db.GdprConsents.Add(consent);
        await _db.SaveChangesAsync();
    }

    // === Data Processing Records ===

    public async Task<List<DataProcessingRecordDto>> GetProcessingRecordsAsync(Guid userId)
    {
        return await _db.DataProcessingRecords
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => new DataProcessingRecordDto
            {
                Id = r.Id,
                UserId = r.UserId,
                ProcessingType = r.ProcessingType,
                Purpose = r.Purpose,
                Timestamp = r.Timestamp,
                LegalBasis = r.LegalBasis
            })
            .ToListAsync();
    }

    // === Breach Notification ===

    public async Task SendBreachNotificationAsync(BreachNotificationDto notification)
    {
        List<Guid> targetUserIds;

        if (notification.NotifyAll)
        {
            targetUserIds = await _db.Users.Select(u => u.Id).ToListAsync();
        }
        else
        {
            targetUserIds = notification.AffectedUserIds ?? new List<Guid>();
        }

        foreach (var userId in targetUserIds)
        {
            await _notificationService.SendNotificationAsync(new Application.Notifications.DTOs.SendNotificationDto
            {
                UserId = userId,
                Type = "BreachNotification",
                Title = notification.Subject,
                Body = notification.Message
            });
        }
    }

    // === Private Helpers ===

    private async Task ProcessDataExportAsync(Domain.Entities.DataExportRequest exportRequest)
    {
        exportRequest.Status = "Ready";
        exportRequest.CompletedAt = DateTime.UtcNow;
        exportRequest.ExpiresAt = DateTime.UtcNow.AddDays(7);
        exportRequest.DownloadUrl = $"/api/gdpr/export/{exportRequest.Id}/download";
        await _db.SaveChangesAsync();
    }

    private async Task<byte[]> GenerateExportDataAsync(Guid userId, string format)
    {
        var user = await _db.Users.FindAsync(userId);
        var subscriptions = await _db.Subscriptions.Where(s => s.UserId == userId).ToListAsync();
        var subscriptionIds = subscriptions.Select(s => s.Id).ToList();
        var payments = await _db.Payments.Where(p => subscriptionIds.Contains(p.SubscriptionId)).ToListAsync();
        var consents = await _db.GdprConsents.Where(c => c.UserId == userId).ToListAsync();
        var comments = await _db.Comments.Where(c => c.UserId == userId).ToListAsync();

        if (format == "json")
        {
            var exportData = new
            {
                Profile = new
                {
                    user?.Email,
                    user?.DisplayName,
                    user?.Bio,
                    user?.Timezone,
                    user?.CreatedAt
                },
                Subscriptions = subscriptions.Select(s => new { s.Id, s.PlanId, s.Status, s.CreatedAt }),
                Payments = payments.Select(p => new { p.Id, p.Amount, p.Currency, p.Provider, p.Status, p.CreatedAt }),
                Consents = consents.Select(c => new { c.ConsentType, c.IsGranted, c.GrantedAt, c.RevokedAt }),
                Comments = comments.Select(c => new { c.Id, c.Content, c.CreatedAt })
            };

            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true }));
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== PROFILE ===");
            sb.AppendLine($"Email,DisplayName,Bio,Timezone,CreatedAt");
            sb.AppendLine($"{user?.Email},{user?.DisplayName},{user?.Bio},{user?.Timezone},{user?.CreatedAt}");
            sb.AppendLine();
            sb.AppendLine("=== SUBSCRIPTIONS ===");
            sb.AppendLine("Id,PlanId,Status,CreatedAt");
            foreach (var s in subscriptions)
                sb.AppendLine($"{s.Id},{s.PlanId},{s.Status},{s.CreatedAt}");
            sb.AppendLine();
            sb.AppendLine("=== PAYMENTS ===");
            sb.AppendLine("Id,Amount,Currency,Provider,Status,CreatedAt");
            foreach (var p in payments)
                sb.AppendLine($"{p.Id},{p.Amount},{p.Currency},{p.Provider},{p.Status},{p.CreatedAt}");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
