import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BlogPostDto {
  id: string;
  title: string;
  slug: string;
  content: string;
  excerpt?: string;
  featuredImageUrl?: string;
  metaTitle?: string;
  metaDescription?: string;
  status: string;
  createdAt: string;
  publishedAt?: string;
  scheduledPublishAt?: string;
  authorId: string;
  authorName: string;
}

export interface BlogPostListItemDto {
  id: string;
  title: string;
  slug: string;
  excerpt?: string;
  featuredImageUrl?: string;
  status: string;
  publishedAt?: string;
  authorName: string;
}

export interface CreateBlogPostDto {
  title: string;
  content: string;
  excerpt?: string;
  featuredImageUrl?: string;
  metaTitle?: string;
  metaDescription?: string;
}

export interface UpdateBlogPostDto {
  title?: string;
  content?: string;
  excerpt?: string;
  featuredImageUrl?: string;
  metaTitle?: string;
  metaDescription?: string;
}

export interface BlogPaginatedResponse<T> {
  items: T[];
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class BlogService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/blog`;

  getPosts(status?: string, page = 1, pageSize = 10): Observable<BlogPaginatedResponse<BlogPostListItemDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (status) params = params.set('status', status);
    return this.http.get<BlogPaginatedResponse<BlogPostListItemDto>>(this.apiUrl, { params });
  }

  getPublishedPosts(page = 1, pageSize = 10): Observable<BlogPaginatedResponse<BlogPostListItemDto>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<BlogPaginatedResponse<BlogPostListItemDto>>(`${this.apiUrl}/published`, { params });
  }

  getPostBySlug(slug: string): Observable<BlogPostDto> {
    return this.http.get<BlogPostDto>(`${this.apiUrl}/by-slug/${slug}`);
  }

  getPost(postId: string): Observable<BlogPostDto> {
    return this.http.get<BlogPostDto>(`${this.apiUrl}/${postId}`);
  }

  createPost(dto: CreateBlogPostDto): Observable<BlogPostDto> {
    return this.http.post<BlogPostDto>(this.apiUrl, dto);
  }

  updatePost(postId: string, dto: UpdateBlogPostDto): Observable<BlogPostDto> {
    return this.http.patch<BlogPostDto>(`${this.apiUrl}/${postId}`, dto);
  }

  deletePost(postId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${postId}`);
  }

  publishPost(postId: string, scheduledPublishAt?: string): Observable<BlogPostDto> {
    return this.http.post<BlogPostDto>(`${this.apiUrl}/${postId}/publish`, { scheduledPublishAt });
  }

  unpublishPost(postId: string): Observable<BlogPostDto> {
    return this.http.post<BlogPostDto>(`${this.apiUrl}/${postId}/unpublish`, {});
  }
}
