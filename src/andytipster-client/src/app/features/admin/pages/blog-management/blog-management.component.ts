import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BlogService, BlogPostListItemDto, CreateBlogPostDto } from '../../../../core/services/blog.service';

@Component({
  selector: 'app-blog-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="p-6 space-y-6">
      <div class="flex items-center justify-between">
        <h1 class="text-2xl font-bold">Blog Management</h1>
        <button class="btn btn-primary btn-sm" (click)="openCreateForm()">New Post</button>
      </div>

      <!-- Filter -->
      <div class="flex gap-3">
        <select class="select select-bordered select-sm" [(ngModel)]="statusFilter" (change)="loadPosts()">
          <option value="">All Statuses</option>
          <option value="Draft">Draft</option>
          <option value="Published">Published</option>
          <option value="Scheduled">Scheduled</option>
        </select>
      </div>

      <!-- Posts Table -->
      <div class="overflow-x-auto">
        <table class="table table-zebra w-full">
          <thead>
            <tr>
              <th>Title</th>
              <th>Author</th>
              <th>Status</th>
              <th>Published</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            @for (post of posts(); track post.id) {
              <tr>
                <td>{{ post.title }}</td>
                <td>{{ post.authorName }}</td>
                <td>
                  <span class="badge" [class.badge-ghost]="post.status === 'Draft'"
                        [class.badge-success]="post.status === 'Published'"
                        [class.badge-info]="post.status === 'Scheduled'">
                    {{ post.status }}
                  </span>
                </td>
                <td>{{ post.publishedAt | date:'short' }}</td>
                <td>
                  <div class="flex gap-1">
                    @if (post.status === 'Draft' || post.status === 'Scheduled') {
                      <button class="btn btn-xs btn-success" (click)="publishPost(post.id)">Publish</button>
                    }
                    @if (post.status === 'Published') {
                      <button class="btn btn-xs btn-warning" (click)="unpublishPost(post.id)">Unpublish</button>
                    }
                    <button class="btn btn-xs btn-ghost" (click)="deletePost(post.id)">Delete</button>
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <!-- Create Modal -->
      @if (showCreateForm) {
        <div class="modal modal-open">
          <div class="modal-box max-w-2xl">
            <h3 class="font-bold text-lg">Create Blog Post</h3>
            <div class="form-control mt-4 space-y-3">
              <label class="label"><span class="label-text">Title</span></label>
              <input type="text" class="input input-bordered" [(ngModel)]="newPost.title" placeholder="Post title" />

              <label class="label"><span class="label-text">Content (HTML)</span></label>
              <textarea class="textarea textarea-bordered" [(ngModel)]="newPost.content" rows="10" placeholder="Rich text content..."></textarea>

              <label class="label"><span class="label-text">Excerpt</span></label>
              <input type="text" class="input input-bordered" [(ngModel)]="newPost.excerpt" placeholder="Brief summary" />

              <label class="label"><span class="label-text">Featured Image URL</span></label>
              <input type="url" class="input input-bordered" [(ngModel)]="newPost.featuredImageUrl" placeholder="https://..." />

              <label class="label"><span class="label-text">Meta Title (SEO)</span></label>
              <input type="text" class="input input-bordered" [(ngModel)]="newPost.metaTitle" maxlength="60" />

              <label class="label"><span class="label-text">Meta Description (SEO)</span></label>
              <textarea class="textarea textarea-bordered" [(ngModel)]="newPost.metaDescription" maxlength="160" rows="2"></textarea>
            </div>
            @if (formError()) {
              <div class="alert alert-error mt-3"><span>{{ formError() }}</span></div>
            }
            <div class="modal-action">
              <button class="btn" (click)="showCreateForm = false">Cancel</button>
              <button class="btn btn-primary" (click)="createPost()">Create</button>
            </div>
          </div>
        </div>
      }
    </section>
  `,
})
export class BlogManagementComponent implements OnInit {
  private readonly blogService = inject(BlogService);

  posts = signal<BlogPostListItemDto[]>([]);
  totalCount = signal(0);
  formError = signal<string | null>(null);

  statusFilter = '';
  showCreateForm = false;
  newPost: CreateBlogPostDto = { title: '', content: '' };

  ngOnInit(): void {
    this.loadPosts();
  }

  loadPosts(): void {
    this.blogService.getPosts(this.statusFilter || undefined).subscribe({
      next: (res) => {
        this.posts.set(res.items);
        this.totalCount.set(res.totalCount);
      }
    });
  }

  openCreateForm(): void {
    this.newPost = { title: '', content: '' };
    this.formError.set(null);
    this.showCreateForm = true;
  }

  createPost(): void {
    this.formError.set(null);
    this.blogService.createPost(this.newPost).subscribe({
      next: () => {
        this.showCreateForm = false;
        this.loadPosts();
      },
      error: (err) => {
        this.formError.set(err.error?.detail || 'Failed to create post.');
      }
    });
  }

  publishPost(postId: string): void {
    this.blogService.publishPost(postId).subscribe({ next: () => this.loadPosts() });
  }

  unpublishPost(postId: string): void {
    this.blogService.unpublishPost(postId).subscribe({ next: () => this.loadPosts() });
  }

  deletePost(postId: string): void {
    if (confirm('Delete this blog post?')) {
      this.blogService.deletePost(postId).subscribe({ next: () => this.loadPosts() });
    }
  }
}
