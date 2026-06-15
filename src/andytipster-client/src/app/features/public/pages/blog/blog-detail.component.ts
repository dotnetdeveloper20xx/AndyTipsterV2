import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BlogService, BlogPostDto } from '../../../../core/services/blog.service';

@Component({
  selector: 'app-blog-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <article class="max-w-3xl mx-auto p-6">
      @if (loading()) {
        <div class="flex justify-center py-12">
          <span class="loading loading-spinner loading-lg"></span>
        </div>
      }

      @if (!loading() && !post()) {
        <div class="text-center py-12">
          <p class="text-lg opacity-60">Post not found.</p>
          <a routerLink="/blog" class="btn btn-ghost mt-4">← Back to Blog</a>
        </div>
      }

      @if (post(); as p) {
        <a routerLink="/blog" class="btn btn-ghost btn-sm mb-4">← Back to Blog</a>

        @if (p.featuredImageUrl) {
          <img [src]="p.featuredImageUrl" [alt]="p.title"
               class="w-full h-64 object-cover rounded-lg mb-6" />
        }

        <h1 class="text-3xl font-bold mb-2">{{ p.title }}</h1>
        <div class="flex items-center gap-3 text-sm opacity-60 mb-6">
          <span>{{ p.authorName }}</span>
          <span>•</span>
          <time>{{ p.publishedAt | date:'longDate' }}</time>
        </div>

        <div class="prose prose-lg max-w-none" [innerHTML]="p.content"></div>
      }
    </article>
  `,
})
export class BlogDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly blogService = inject(BlogService);

  post = signal<BlogPostDto | null>(null);
  loading = signal(false);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (slug) {
      this.loading.set(true);
      this.blogService.getPostBySlug(slug).subscribe({
        next: (post) => {
          this.post.set(post);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        }
      });
    }
  }
}
