using AndyTipster.Application.GDPR.DTOs;

namespace AndyTipster.Application.GDPR.Services;

public interface IGdprService
{
    // Data Export (Right to Access)
    Task<DataExportStatusDto> RequestDataExportAsync(Guid userId, DataExportRequestDto request);
    Task<DataExportStatusDto> GetExportStatusAsync(Guid userId, Guid requestId);
    Task<byte[]?> DownloadExportAsync(Guid userId, Guid requestId);

    // Account Deletion (Right to Erasure)
    Task<AccountDeletionStatusDto> RequestAccountDeletionAsync(Guid userId, AccountDeletionRequestDto request);
    Task<AccountDeletionStatusDto> GetDeletionStatusAsync(Guid userId);
    Task<AccountDeletionStatusDto> CancelDeletionAsync(Guid userId);
    Task PurgeSoftDeletedAccountsAsync(); // Background job: permanent purge after 30 days

    // Right to Rectification
    Task RectifyPersonalDataAsync(Guid userId, RectificationRequestDto request);

    // Consent Management
    Task<List<ConsentRecordDto>> GetConsentRecordsAsync(Guid userId);
    Task RecordConsentAsync(Guid userId, string consentType, bool isGranted, string? ipAddress, string? userAgent);

    // Data Processing Records
    Task<List<DataProcessingRecordDto>> GetProcessingRecordsAsync(Guid userId);

    // Breach Notification
    Task SendBreachNotificationAsync(BreachNotificationDto notification);
}
