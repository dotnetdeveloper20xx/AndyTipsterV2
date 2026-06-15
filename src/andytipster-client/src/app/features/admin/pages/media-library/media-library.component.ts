import { ChangeDetectionStrategy, Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MediaService, MediaAssetDto } from '../../../../core/services/media.service';

@Component({
  selector: 'app-media-library',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold">Media Library</h1>
        <div class="flex gap-2">
          <input
            type="file"
            #fileInput
            multiple
            class="hidden"
            (change)="onFilesSelected($event)"
            accept="image/*,application/pdf,video/mp4"
            aria-label="Upload files"
          />
          <button class="btn btn-primary btn-sm" (click)="fileInput.click()">
            📤 Upload
          </button>
        </div>
      </div>

      <!-- Search & Filters -->
      <div class="flex gap-3 mb-4">
        <input
          class="input input-bordered input-sm flex-1"
          placeholder="Search by filename or tags..."
          [(ngModel)]="searchQuery"
          (ngModelChange)="search()"
          aria-label="Search media"
        />
        <select class="select select-bordered select-sm" [(ngModel)]="filterType" (ngModelChange)="search()" aria-label="Filter by type">
          <option value="">All types</option>
          <option value="image">Images</option>
          <option value="application">Documents</option>
          <option value="video">Videos</option>
        </select>
      </div>

      <!-- Upload progress -->
      @if (uploading()) {
        <div class="alert alert-info mb-4">
          <span>Uploading... {{ uploadProgress() }}%</span>
          <progress class="progress progress-primary w-56" [value]="uploadProgress()" max="100"></progress>
        </div>
      }

      <!-- Media grid -->
      <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
        @for (asset of assets(); track asset.id) {
          <div
            class="card bg-base-100 shadow-sm cursor-pointer hover:shadow-md transition-shadow"
            [class.ring-2]="selectedAsset()?.id === asset.id"
            [class.ring-primary]="selectedAsset()?.id === asset.id"
            (click)="selectAsset(asset)"
            role="button"
            [attr.aria-label]="asset.fileName"
          >
            <figure class="h-32 bg-base-200 flex items-center justify-center overflow-hidden">
              @if (isImage(asset)) {
                <img [src]="asset.cdnUrl || asset.blobUrl" [alt]="asset.altText || asset.fileName" class="object-cover w-full h-full" />
              } @else if (isVideo(asset)) {
                <span class="text-3xl">🎬</span>
              } @else {
                <span class="text-3xl">📄</span>
              }
            </figure>
            <div class="p-2">
              <p class="text-xs truncate" [title]="asset.fileName">{{ asset.fileName }}</p>
              <p class="text-xs text-base-content/50">{{ formatSize(asset.fileSizeBytes) }}</p>
            </div>
          </div>
        }
      </div>

      @if (assets().length === 0 && !uploading()) {
        <div class="text-center py-12 text-base-content/50">
          <p class="text-lg mb-2">No media files</p>
          <p class="text-sm">Upload files to get started</p>
        </div>
      }

      <!-- Asset detail panel -->
      @if (selectedAsset()) {
        <div class="fixed inset-y-0 right-0 w-80 bg-base-100 shadow-xl border-l border-base-300 p-4 overflow-y-auto z-50">
          <div class="flex justify-between items-center mb-4">
            <h2 class="font-semibold">Asset Details</h2>
            <button class="btn btn-sm btn-ghost" (click)="selectedAsset.set(null)" aria-label="Close panel">✕</button>
          </div>

          @if (isImage(selectedAsset()!)) {
            <img [src]="selectedAsset()!.cdnUrl || selectedAsset()!.blobUrl" [alt]="selectedAsset()!.altText" class="w-full rounded-lg mb-4" />
          }

          <div class="form-control mb-3">
            <label class="label"><span class="label-text text-xs">File name</span></label>
            <input class="input input-sm input-bordered" [value]="selectedAsset()!.fileName" readonly />
          </div>

          <div class="form-control mb-3">
            <label class="label"><span class="label-text text-xs">Alt text</span></label>
            <input
              class="input input-sm input-bordered"
              [(ngModel)]="editAltText"
              placeholder="Descriptive alt text"
              aria-label="Alt text"
            />
            <label class="label"><span class="label-text-alt">{{ editAltText.length }}/125</span></label>
          </div>

          <div class="form-control mb-3">
            <label class="label"><span class="label-text text-xs">Tags</span></label>
            <input
              class="input input-sm input-bordered"
              [(ngModel)]="editTags"
              placeholder="tag1, tag2, tag3"
              aria-label="Tags"
            />
          </div>

          <div class="flex gap-2 mt-4">
            <button class="btn btn-sm btn-primary flex-1" (click)="saveAssetChanges()">Save</button>
            <button class="btn btn-sm btn-error" (click)="deleteAsset()" aria-label="Delete asset">🗑️</button>
          </div>

          <div class="mt-4 text-xs text-base-content/50">
            <p>Size: {{ formatSize(selectedAsset()!.fileSizeBytes) }}</p>
            <p>Type: {{ selectedAsset()!.contentType }}</p>
            @if (selectedAsset()!.width && selectedAsset()!.height) {
              <p>Dimensions: {{ selectedAsset()!.width }}×{{ selectedAsset()!.height }}</p>
            }
            <p>Uploaded: {{ selectedAsset()!.createdAt | date:'medium' }}</p>
            <p>By: {{ selectedAsset()!.uploadedByUserName }}</p>
          </div>
        </div>
      }
    </div>
  `
})
export class MediaLibraryComponent implements OnInit {
  private readonly mediaService = inject(MediaService);

  assets = signal<MediaAssetDto[]>([]);
  selectedAsset = signal<MediaAssetDto | null>(null);
  uploading = signal(false);
  uploadProgress = signal(0);
  searchQuery = '';
  filterType = '';
  editAltText = '';
  editTags = '';

  ngOnInit(): void {
    this.search();
  }

  search(): void {
    this.mediaService.search({
      query: this.searchQuery || undefined,
      contentType: this.filterType || undefined,
      page: 1,
      pageSize: 50,
    }).subscribe(assets => this.assets.set(assets));
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const files = Array.from(input.files).slice(0, 20);
    this.uploading.set(true);
    this.uploadProgress.set(0);

    // Batch upload
    this.mediaService.batchUpload(files, 'Uploaded media').subscribe({
      next: (result) => {
        this.uploading.set(false);
        this.uploadProgress.set(100);
        this.search();
        if (result.failed.length > 0) {
          console.warn('Some files failed:', result.failed);
        }
      },
      error: () => {
        this.uploading.set(false);
      }
    });

    input.value = '';
  }

  selectAsset(asset: MediaAssetDto): void {
    this.selectedAsset.set(asset);
    this.editAltText = asset.altText ?? '';
    this.editTags = asset.tags?.join(', ') ?? '';
  }

  saveAssetChanges(): void {
    const asset = this.selectedAsset();
    if (!asset) return;

    this.mediaService.updateAsset(asset.id, {
      altText: this.editAltText,
      tags: this.editTags.split(',').map(t => t.trim()).filter(t => t),
    }).subscribe(updated => {
      this.selectedAsset.set(updated);
      this.search();
    });
  }

  deleteAsset(): void {
    const asset = this.selectedAsset();
    if (!asset) return;

    this.mediaService.checkInUse(asset.id).subscribe(({ inUse, pages }) => {
      if (inUse) {
        alert(`Cannot delete. Referenced by: ${pages.join(', ')}`);
        return;
      }
      if (confirm(`Delete ${asset.fileName}?`)) {
        this.mediaService.deleteAsset(asset.id).subscribe(() => {
          this.selectedAsset.set(null);
          this.search();
        });
      }
    });
  }

  isImage(asset: MediaAssetDto): boolean {
    return asset.contentType.startsWith('image/');
  }

  isVideo(asset: MediaAssetDto): boolean {
    return asset.contentType.startsWith('video/');
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
