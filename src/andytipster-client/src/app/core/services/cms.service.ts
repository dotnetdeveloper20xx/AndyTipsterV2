import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BlockDto {
  id: string;
  blockType: string;
  contentJson: string;
  sortOrder: number;
}

export interface PageDto {
  id: string;
  title: string;
  slug: string;
  status: string;
  metaTitle?: string;
  metaDescription?: string;
  ogImageUrl?: string;
  canonicalUrl?: string;
  noIndex: boolean;
  publishedAt?: string;
  scheduledPublishAt?: string;
  expiresAt?: string;
  currentVersion: number;
  blocks: BlockDto[];
  createdAt: string;
  updatedAt?: string;
  createdByUserName: string;
}

export interface CreatePageRequest {
  title: string;
  slug: string;
  metaTitle?: string;
  metaDescription?: string;
  ogImageUrl?: string;
  canonicalUrl?: string;
  noIndex?: boolean;
  blocks: BlockDto[];
}

export interface UpdatePageRequest {
  title?: string;
  slug?: string;
  metaTitle?: string;
  metaDescription?: string;
  ogImageUrl?: string;
  canonicalUrl?: string;
  noIndex?: boolean;
  blocks?: BlockDto[];
  changeSummary?: string;
}

export interface PublishPageRequest {
  scheduledPublishAt?: string;
  expiresAt?: string;
  timezone?: string;
}

export interface PageVersionDto {
  id: string;
  versionNumber: number;
  blocksJson: string;
  changeSummary?: string;
  authorUserName: string;
  createdAt: string;
}

export interface PublishingQueueItem {
  pageId: string;
  title: string;
  slug: string;
  scheduledPublishAt?: string;
  expiresAt?: string;
  status: string;
}

export interface NavigationMenuDto {
  id: string;
  name: string;
  location: string;
  isActive: boolean;
  items: MenuItemDto[];
  createdAt: string;
  updatedAt?: string;
}

export interface MenuItemDto {
  id: string;
  parentItemId?: string;
  label: string;
  url: string;
  icon?: string;
  target?: string;
  sortOrder: number;
  isVisible: boolean;
  requiredRole?: string;
  requiredSubscriptionStatus?: string;
  children: MenuItemDto[];
}

export interface SiteSettingsDto {
  siteName: string;
  tagline?: string;
  logoLightUrl?: string;
  logoDarkUrl?: string;
  faviconUrl?: string;
  maintenanceMode: boolean;
  maintenanceMessage?: string;
  analyticsScript?: string;
  analyticsRequiresCookieConsent: boolean;
  redirects: RedirectDto[];
}

export interface RedirectDto {
  id: string;
  fromPath: string;
  toPath: string;
  isPermanent: boolean;
  isActive: boolean;
}

export interface PageSeoDto {
  pageId: string;
  title: string;
  slug: string;
  metaTitle?: string;
  metaDescription?: string;
  ogImageUrl?: string;
  canonicalUrl?: string;
  noIndex: boolean;
  structuredDataJson?: string;
}

@Injectable({ providedIn: 'root' })
export class CmsService {
  private readonly http = inject(HttpClient);
  private readonly pagesUrl = `${environment.apiUrl}/api/cms/pages`;
  private readonly navigationUrl = `${environment.apiUrl}/api/navigation`;
  private readonly seoUrl = `${environment.apiUrl}/api/seo`;
  private readonly settingsUrl = `${environment.apiUrl}/api/site-settings`;

  // Pages
  getPages(status?: string, page = 1, pageSize = 25): Observable<PageDto[]> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status) params = params.set('status', status);
    return this.http.get<PageDto[]>(this.pagesUrl, { params });
  }

  getPage(pageId: string): Observable<PageDto> {
    return this.http.get<PageDto>(`${this.pagesUrl}/${pageId}`);
  }

  getPageBySlug(slug: string): Observable<PageDto> {
    return this.http.get<PageDto>(`${this.pagesUrl}/by-slug/${slug}`);
  }

  createPage(request: CreatePageRequest): Observable<PageDto> {
    return this.http.post<PageDto>(this.pagesUrl, request);
  }

  updatePage(pageId: string, request: UpdatePageRequest): Observable<PageDto> {
    return this.http.patch<PageDto>(`${this.pagesUrl}/${pageId}`, request);
  }

  deletePage(pageId: string): Observable<void> {
    return this.http.delete<void>(`${this.pagesUrl}/${pageId}`);
  }

  publishPage(pageId: string, request: PublishPageRequest = {}): Observable<PageDto> {
    return this.http.post<PageDto>(`${this.pagesUrl}/${pageId}/publish`, request);
  }

  unpublishPage(pageId: string): Observable<PageDto> {
    return this.http.post<PageDto>(`${this.pagesUrl}/${pageId}/unpublish`, {});
  }

  getVersionHistory(pageId: string): Observable<PageVersionDto[]> {
    return this.http.get<PageVersionDto[]>(`${this.pagesUrl}/${pageId}/versions`);
  }

  getVersion(pageId: string, versionNumber: number): Observable<PageVersionDto> {
    return this.http.get<PageVersionDto>(`${this.pagesUrl}/${pageId}/versions/${versionNumber}`);
  }

  rollbackToVersion(pageId: string, versionNumber: number): Observable<PageDto> {
    return this.http.post<PageDto>(`${this.pagesUrl}/${pageId}/rollback/${versionNumber}`, {});
  }

  getPublishingQueue(): Observable<PublishingQueueItem[]> {
    return this.http.get<PublishingQueueItem[]>(`${this.pagesUrl}/publishing-queue`);
  }

  // Navigation
  getMenus(): Observable<NavigationMenuDto[]> {
    return this.http.get<NavigationMenuDto[]>(this.navigationUrl);
  }

  getMenuByLocation(location: string): Observable<NavigationMenuDto> {
    return this.http.get<NavigationMenuDto>(`${this.navigationUrl}/by-location/${location}`);
  }

  createMenu(request: { name: string; location: string }): Observable<NavigationMenuDto> {
    return this.http.post<NavigationMenuDto>(this.navigationUrl, request);
  }

  updateMenu(menuId: string, request: any): Observable<NavigationMenuDto> {
    return this.http.patch<NavigationMenuDto>(`${this.navigationUrl}/${menuId}`, request);
  }

  deleteMenu(menuId: string): Observable<void> {
    return this.http.delete<void>(`${this.navigationUrl}/${menuId}`);
  }

  // SEO
  getPageSeo(pageId: string): Observable<PageSeoDto> {
    return this.http.get<PageSeoDto>(`${this.seoUrl}/pages/${pageId}`);
  }

  updatePageSeo(pageId: string, request: any): Observable<PageSeoDto> {
    return this.http.patch<PageSeoDto>(`${this.seoUrl}/pages/${pageId}`, request);
  }

  // Site Settings
  getSiteSettings(): Observable<SiteSettingsDto> {
    return this.http.get<SiteSettingsDto>(this.settingsUrl);
  }

  updateSiteSettings(request: any): Observable<SiteSettingsDto> {
    return this.http.patch<SiteSettingsDto>(this.settingsUrl, request);
  }

  getRedirects(): Observable<RedirectDto[]> {
    return this.http.get<RedirectDto[]>(`${this.settingsUrl}/redirects`);
  }

  createRedirect(request: { fromPath: string; toPath: string; isPermanent: boolean }): Observable<RedirectDto> {
    return this.http.post<RedirectDto>(`${this.settingsUrl}/redirects`, request);
  }

  deleteRedirect(redirectId: string): Observable<void> {
    return this.http.delete<void>(`${this.settingsUrl}/redirects/${redirectId}`);
  }
}
