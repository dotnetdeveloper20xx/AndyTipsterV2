import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BlogService, BlogPostListItemDto } from '../../../../core/services/blog.service';

@Component({
  selector: 'app-blog',
  standalone: true,
  imports: [CommonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="max-w-4xl mx-auto p-6">
      <h1 class="text-3xl font-bold mb-8">Blog</h1>

      @if (loading()) {
        <div class="flex justify-center py-12">
          <span class="loading loading-spinner loading-lg"></span>
        </div>
      }

      @if (!loading() && posts().length === 0) {
        <div class="text-center py-12">
          <p class="text-lg opacity-60">No blog posts yet. Check back soon!</p>
        </div>
      }

      <div class="grid gap-8">
        @for (post of posts(); track post.id) {
          <article class="card bg-base-100 shadow hover:shadow-md transition-shadow">
            <div class="card-body">
              <div class="flex gap-4">
                @if (post.featuredImageUrl) {
                  <img [src]="post.featuredImageUrl" [alt]="post.title"
                       class="w-32 h-24 object-cover rounded-lg flex-shrink-0" />
                }
                <div class="flex-1">
                  <a [routerLink]="['/blog', post.slug]" class="card-title hover:text-primary transition-colors cursor-pointer">
                    {{ post.title }}
                  </a>
                  @if (post.excerpt) {
                    <p class="text-sm opacity-70 mt-1 line-clamp-2">{{ post.excerpt }}</p>
                  }
                  <div class="flex items-center gap-3 mt-2 text-xs opacity-60">
                    <span>{{ post.authorName }}</span>
                    <span>•</span>
                    <time>{{ post.publishedAt | date:'mediumDate' }}</time>
                  </div>
                </div>
              </div>
            </div>
          </article>
        }
      </div>

      <!-- Pagination -->
      @if (totalCount() > posts().length || currentPage() > 1) {
        <div class="flex justify-center mt-8">
          <div class="join">
            <button class="join-item btn btn-sm" [disabled]="currentPage() === 1" (click)="loadPage(currentPage() - 1)">«</button>
            <button class="join-item btn btn-sm">Page {{ currentPage() }}</button>
            <button class="join-item btn btn-sm" [disabled]="posts().length < pageSize" (click)="loadPage(currentPage() + 1)">»</button>
          </div>
        </div>
      }
    </section>
  `,
})
export class BlogComponent implements OnInit {
  private readonly blogService = inject(BlogService);

  posts = signal<BlogPostListItemDto[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  currentPage = signal(1);
  pageSize = 10;

  ngOnInit(): void {
    this.loadPage(1);
  }

  loadPage(page: number): void {
    this.loading.set(true);
    this.currentPage.set(page);
    this.blogService.getPublishedPosts(page, this.pageSize).subscribe({
      next: (res) => {
        this.posts.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }
}
