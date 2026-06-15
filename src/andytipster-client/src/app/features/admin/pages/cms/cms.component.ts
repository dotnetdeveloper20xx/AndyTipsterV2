import { ChangeDetectionStrategy, Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { inject } from '@angular/core';
import { CmsService, PageDto, BlockDto, CreatePageRequest, UpdatePageRequest } from '../../../../core/services/cms.service';
import { Subject, interval, takeUntil, switchMap, catchError, of } from 'rxjs';

interface EditorState {
  page: PageDto | null;
  blocks: BlockDto[];
  undoStack: BlockDto[][];
  redoStack: BlockDto[][];
  isDirty: boolean;
  previewMode: 'desktop' | 'tablet' | 'mobile';
  selectedBlockIndex: number | null;
  autoSaveEnabled: boolean;
  lastSaved: Date | null;
  saveError: string | null;
}

@Component({
  selector: 'app-cms',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col h-full">
      <!-- Toolbar -->
      <div class="navbar bg-base-200 border-b border-base-300 px-4 gap-2">
        <div class="flex-1">
          <h1 class="text-lg font-bold">CMS Page Builder</h1>
        </div>
        <div class="flex items-center gap-2">
          <!-- Preview toggles -->
          <div class="btn-group">
            <button class="btn btn-sm" [class.btn-active]="state().previewMode === 'desktop'" (click)="setPreviewMode('desktop')" aria-label="Desktop preview">
              🖥️
            </button>
            <button class="btn btn-sm" [class.btn-active]="state().previewMode === 'tablet'" (click)="setPreviewMode('tablet')" aria-label="Tablet preview">
              📱
            </button>
            <button class="btn btn-sm" [class.btn-active]="state().previewMode === 'mobile'" (click)="setPreviewMode('mobile')" aria-label="Mobile preview">
              📲
            </button>
          </div>
          <!-- Undo/Redo -->
          <button class="btn btn-sm btn-ghost" [disabled]="!canUndo()" (click)="undo()" aria-label="Undo">↩️</button>
          <button class="btn btn-sm btn-ghost" [disabled]="!canRedo()" (click)="redo()" aria-label="Redo">↪️</button>
          <!-- Save status -->
          @if (state().saveError) {
            <span class="badge badge-error text-xs">Save failed</span>
          } @else if (state().isDirty) {
            <span class="badge badge-warning text-xs">Unsaved</span>
          } @else if (state().lastSaved) {
            <span class="badge badge-success text-xs">Saved</span>
          }
          <!-- Actions -->
          <button class="btn btn-sm btn-primary" (click)="savePage()">Save</button>
          <button class="btn btn-sm btn-accent" (click)="publishPage()">Publish</button>
        </div>
      </div>

      <!-- Main content area -->
      <div class="flex flex-1 overflow-hidden">
        <!-- Block palette (left sidebar) -->
        <div class="w-64 bg-base-100 border-r border-base-300 overflow-y-auto p-4">
          <h2 class="font-semibold mb-3 text-sm">Content Blocks</h2>
          @for (blockType of blockTypes; track blockType.type) {
            <button
              class="btn btn-sm btn-ghost w-full justify-start mb-1"
              (click)="addBlock(blockType.type)"
              [attr.aria-label]="'Add ' + blockType.label + ' block'"
            >
              <span class="mr-2">{{ blockType.icon }}</span>
              {{ blockType.label }}
            </button>
          }
        </div>

        <!-- Canvas (center) -->
        <div class="flex-1 overflow-y-auto p-6 bg-base-200">
          @if (state().page && !isPageListView()) {
            <div
              class="mx-auto bg-base-100 shadow-lg rounded-lg min-h-96 p-4"
              [style.max-width]="previewWidth()"
            >
              <!-- Page title -->
              <input
                class="input input-ghost w-full text-2xl font-bold mb-4"
                [ngModel]="state().page?.title"
                (ngModelChange)="updatePageTitle($event)"
                placeholder="Page Title"
                aria-label="Page title"
              />

              <!-- Blocks -->
              @for (block of state().blocks; track block.id; let i = $index) {
                <div
                  class="group relative border border-transparent hover:border-primary rounded-lg p-3 mb-2 transition-colors"
                  [class.border-primary]="state().selectedBlockIndex === i"
                  [class.bg-primary/5]="state().selectedBlockIndex === i"
                  (click)="selectBlock(i)"
                  role="button"
                  [attr.aria-label]="'Block ' + (i + 1) + ': ' + block.blockType"
                >
                  <div class="flex items-center justify-between">
                    <span class="text-sm font-medium text-base-content/70">{{ getBlockLabel(block.blockType) }}</span>
                    <div class="opacity-0 group-hover:opacity-100 transition-opacity flex gap-1">
                      <button class="btn btn-xs btn-ghost" (click)="moveBlockUp(i); $event.stopPropagation()" [disabled]="i === 0" aria-label="Move up">⬆</button>
                      <button class="btn btn-xs btn-ghost" (click)="moveBlockDown(i); $event.stopPropagation()" [disabled]="i === state().blocks.length - 1" aria-label="Move down">⬇</button>
                      <button class="btn btn-xs btn-ghost btn-error" (click)="deleteBlock(i); $event.stopPropagation()" aria-label="Delete block">✕</button>
                    </div>
                  </div>
                  <div class="text-xs text-base-content/50 mt-1">{{ getBlockPreview(block) }}</div>
                </div>
              }

              @if (state().blocks.length === 0) {
                <div class="text-center text-base-content/50 py-12">
                  <p class="text-lg mb-2">No blocks yet</p>
                  <p class="text-sm">Add blocks from the palette on the left</p>
                </div>
              }
            </div>
          } @else {
            <!-- Page list view -->
            <div class="max-w-4xl mx-auto">
              <div class="flex justify-between items-center mb-4">
                <h2 class="text-xl font-bold">Pages</h2>
                <button class="btn btn-primary btn-sm" (click)="createNewPage()">+ New Page</button>
              </div>
              @for (page of pages(); track page.id) {
                <div class="card bg-base-100 shadow-sm mb-2 cursor-pointer hover:shadow-md transition-shadow" (click)="openPage(page)">
                  <div class="card-body py-3 px-4 flex-row items-center justify-between">
                    <div>
                      <h3 class="font-medium">{{ page.title }}</h3>
                      <p class="text-xs text-base-content/60">/{{ page.slug }}</p>
                    </div>
                    <span class="badge" [class.badge-success]="page.status === 'Published'" [class.badge-warning]="page.status === 'Scheduled'" [class.badge-ghost]="page.status === 'Draft'">
                      {{ page.status }}
                    </span>
                  </div>
                </div>
              }
            </div>
          }
        </div>

        <!-- Block configuration panel (right sidebar) -->
        @if (state().selectedBlockIndex !== null && state().page) {
          <div class="w-80 bg-base-100 border-l border-base-300 overflow-y-auto p-4">
            <h2 class="font-semibold mb-3 text-sm">Block Settings</h2>
            @if (selectedBlock()) {
              <div class="form-control mb-3">
                <label class="label"><span class="label-text text-xs">Block Type</span></label>
                <input class="input input-sm input-bordered" [value]="selectedBlock()!.blockType" disabled />
              </div>
              <div class="form-control mb-3">
                <label class="label"><span class="label-text text-xs">Content (JSON)</span></label>
                <textarea
                  class="textarea textarea-bordered textarea-sm h-48 font-mono text-xs"
                  [ngModel]="selectedBlock()!.contentJson"
                  (ngModelChange)="updateBlockContent($event)"
                  aria-label="Block content JSON"
                ></textarea>
              </div>
            }
          </div>
        }
      </div>
    </div>
  `
})
export class CmsComponent implements OnInit, OnDestroy {
  private readonly cmsService = inject(CmsService);
  private readonly destroy$ = new Subject<void>();

  state = signal<EditorState>({
    page: null,
    blocks: [],
    undoStack: [],
    redoStack: [],
    isDirty: false,
    previewMode: 'desktop',
    selectedBlockIndex: null,
    autoSaveEnabled: true,
    lastSaved: null,
    saveError: null,
  });

  pages = signal<PageDto[]>([]);

  blockTypes = [
    { type: 'hero', label: 'Hero Section', icon: '🎯' },
    { type: 'rich-text', label: 'Rich Text', icon: '📝' },
    { type: 'image', label: 'Image', icon: '🖼️' },
    { type: 'gallery', label: 'Image Gallery', icon: '🎨' },
    { type: 'video', label: 'Video Embed', icon: '🎬' },
    { type: 'cta', label: 'Call to Action', icon: '📢' },
    { type: 'pricing-table', label: 'Pricing Table', icon: '💰' },
    { type: 'testimonials', label: 'Testimonials', icon: '💬' },
    { type: 'faq', label: 'FAQ Accordion', icon: '❓' },
    { type: 'stats-counter', label: 'Stats Counter', icon: '📊' },
    { type: 'social-feed', label: 'Social Feed', icon: '📱' },
    { type: 'contact-form', label: 'Contact Form', icon: '✉️' },
    { type: 'divider', label: 'Divider', icon: '➖' },
    { type: 'html-embed', label: 'HTML Embed', icon: '🔧' },
    { type: 'blog-list', label: 'Blog Post List', icon: '📰' },
    { type: 'tip-of-day', label: 'Tip of the Day', icon: '🏇' },
    { type: 'countdown', label: 'Countdown Timer', icon: '⏰' },
    { type: 'banner-alert', label: 'Banner Alert', icon: '🚨' },
  ];

  canUndo = computed(() => this.state().undoStack.length > 0);
  canRedo = computed(() => this.state().redoStack.length > 0);
  selectedBlock = computed(() => {
    const idx = this.state().selectedBlockIndex;
    return idx !== null ? this.state().blocks[idx] ?? null : null;
  });

  ngOnInit(): void {
    this.loadPages();
    this.setupAutoSave();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  isPageListView(): boolean {
    return this.state().page === null;
  }

  previewWidth(): string {
    switch (this.state().previewMode) {
      case 'mobile': return '375px';
      case 'tablet': return '768px';
      default: return '100%';
    }
  }

  loadPages(): void {
    this.cmsService.getPages().subscribe(pages => this.pages.set(pages));
  }

  openPage(page: PageDto): void {
    this.cmsService.getPage(page.id).subscribe(fullPage => {
      this.state.update(s => ({
        ...s,
        page: fullPage,
        blocks: fullPage.blocks,
        undoStack: [],
        redoStack: [],
        isDirty: false,
        selectedBlockIndex: null,
      }));
    });
  }

  createNewPage(): void {
    const request: CreatePageRequest = {
      title: 'Untitled Page',
      slug: 'untitled-page-' + Date.now(),
      blocks: [],
    };
    this.cmsService.createPage(request).subscribe(page => {
      this.pages.update(p => [page, ...p]);
      this.openPage(page);
    });
  }

  setPreviewMode(mode: 'desktop' | 'tablet' | 'mobile'): void {
    this.state.update(s => ({ ...s, previewMode: mode }));
  }

  addBlock(blockType: string): void {
    this.pushUndoState();
    const newBlock: BlockDto = {
      id: crypto.randomUUID(),
      blockType,
      contentJson: this.getDefaultContent(blockType),
      sortOrder: this.state().blocks.length,
    };
    this.state.update(s => ({
      ...s,
      blocks: [...s.blocks, newBlock],
      isDirty: true,
      selectedBlockIndex: s.blocks.length,
    }));
  }

  selectBlock(index: number): void {
    this.state.update(s => ({ ...s, selectedBlockIndex: index }));
  }

  moveBlockUp(index: number): void {
    if (index === 0) return;
    this.pushUndoState();
    this.state.update(s => {
      const blocks = [...s.blocks];
      [blocks[index - 1], blocks[index]] = [blocks[index], blocks[index - 1]];
      return { ...s, blocks, isDirty: true, selectedBlockIndex: index - 1 };
    });
  }

  moveBlockDown(index: number): void {
    if (index >= this.state().blocks.length - 1) return;
    this.pushUndoState();
    this.state.update(s => {
      const blocks = [...s.blocks];
      [blocks[index], blocks[index + 1]] = [blocks[index + 1], blocks[index]];
      return { ...s, blocks, isDirty: true, selectedBlockIndex: index + 1 };
    });
  }

  deleteBlock(index: number): void {
    this.pushUndoState();
    this.state.update(s => ({
      ...s,
      blocks: s.blocks.filter((_, i) => i !== index),
      isDirty: true,
      selectedBlockIndex: null,
    }));
  }

  updateBlockContent(contentJson: string): void {
    const idx = this.state().selectedBlockIndex;
    if (idx === null) return;
    this.pushUndoState();
    this.state.update(s => {
      const blocks = [...s.blocks];
      blocks[idx] = { ...blocks[idx], contentJson };
      return { ...s, blocks, isDirty: true };
    });
  }

  updatePageTitle(title: string): void {
    this.state.update(s => ({
      ...s,
      page: s.page ? { ...s.page, title } : null,
      isDirty: true,
    }));
  }

  undo(): void {
    this.state.update(s => {
      if (s.undoStack.length === 0) return s;
      const newUndoStack = [...s.undoStack];
      const previousBlocks = newUndoStack.pop()!;
      return {
        ...s,
        redoStack: [...s.redoStack, s.blocks],
        blocks: previousBlocks,
        undoStack: newUndoStack,
        isDirty: true,
      };
    });
  }

  redo(): void {
    this.state.update(s => {
      if (s.redoStack.length === 0) return s;
      const newRedoStack = [...s.redoStack];
      const nextBlocks = newRedoStack.pop()!;
      return {
        ...s,
        undoStack: [...s.undoStack, s.blocks],
        blocks: nextBlocks,
        redoStack: newRedoStack,
        isDirty: true,
      };
    });
  }

  savePage(): void {
    const s = this.state();
    if (!s.page) return;

    const request: UpdatePageRequest = {
      title: s.page.title,
      blocks: s.blocks.map((b, i) => ({ ...b, sortOrder: i })),
      changeSummary: 'Content updated',
    };

    this.cmsService.updatePage(s.page.id, request).pipe(
      catchError(err => {
        this.state.update(st => ({ ...st, saveError: err.message }));
        return of(null);
      })
    ).subscribe(result => {
      if (result) {
        this.state.update(st => ({
          ...st,
          page: result,
          isDirty: false,
          lastSaved: new Date(),
          saveError: null,
        }));
      }
    });
  }

  publishPage(): void {
    const page = this.state().page;
    if (!page) return;
    this.savePage();
    this.cmsService.publishPage(page.id).subscribe(result => {
      this.state.update(s => ({ ...s, page: result }));
    });
  }

  getBlockLabel(type: string): string {
    return this.blockTypes.find(b => b.type === type)?.label ?? type;
  }

  getBlockPreview(block: BlockDto): string {
    try {
      const content = JSON.parse(block.contentJson);
      return content.heading || content.text?.substring(0, 60) || content.title || block.blockType;
    } catch {
      return block.blockType;
    }
  }

  private pushUndoState(): void {
    this.state.update(s => ({
      ...s,
      undoStack: [...s.undoStack.slice(-49), s.blocks],
      redoStack: [],
    }));
  }

  private setupAutoSave(): void {
    interval(30000).pipe(
      takeUntil(this.destroy$),
      switchMap(() => {
        if (this.state().isDirty && this.state().page && this.state().autoSaveEnabled) {
          const s = this.state();
          return this.cmsService.updatePage(s.page!.id, {
            title: s.page!.title,
            blocks: s.blocks.map((b, i) => ({ ...b, sortOrder: i })),
            changeSummary: 'Auto-save',
          }).pipe(catchError(() => of(null)));
        }
        return of(null);
      })
    ).subscribe(result => {
      if (result) {
        this.state.update(s => ({ ...s, isDirty: false, lastSaved: new Date(), saveError: null }));
      }
    });
  }

  private getDefaultContent(blockType: string): string {
    const defaults: Record<string, object> = {
      'hero': { heading: 'Welcome', subheading: 'Your subtitle here', ctaText: 'Get Started', ctaUrl: '/', backgroundImage: '' },
      'rich-text': { html: '<p>Your content here...</p>' },
      'image': { src: '', alt: '', caption: '' },
      'gallery': { images: [] },
      'video': { url: '', provider: 'youtube' },
      'cta': { heading: 'Ready to get started?', buttonText: 'Sign Up', buttonUrl: '/auth/register' },
      'pricing-table': { autoFromPlans: true, plans: [] },
      'testimonials': { items: [] },
      'faq': { items: [{ question: 'Sample question?', answer: 'Sample answer.' }] },
      'stats-counter': { items: [{ label: 'Users', value: 1000 }] },
      'social-feed': { platform: 'twitter', handle: '' },
      'contact-form': { fields: ['name', 'email', 'message'], recipientEmail: '' },
      'divider': { style: 'solid' },
      'html-embed': { code: '' },
      'blog-list': { count: 5, showExcerpt: true },
      'tip-of-day': { showFreePreview: true },
      'countdown': { targetDate: '', label: 'Time remaining' },
      'banner-alert': { message: '', type: 'info' },
    };
    return JSON.stringify(defaults[blockType] ?? {});
  }
}
