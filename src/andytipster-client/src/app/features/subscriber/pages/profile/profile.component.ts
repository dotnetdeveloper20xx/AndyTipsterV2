import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../../core/services/user.service';

type ProfileTab = 'profile' | 'security' | 'notifications' | 'billing' | 'privacy' | 'appearance';

interface ActivityEntry {
  id: number;
  actionType: string;
  description: string;
  timestamp: string;
  ipAddress: string | null;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="p-6 max-w-4xl mx-auto space-y-6">
      <h1 class="text-2xl font-bold">Settings</h1>

      <!-- Tabs -->
      <div class="tabs tabs-boxed" role="tablist">
        <button class="tab" [class.tab-active]="activeTab() === 'profile'" (click)="activeTab.set('profile')" role="tab"
          [attr.aria-selected]="activeTab() === 'profile'">Profile</button>
        <button class="tab" [class.tab-active]="activeTab() === 'security'" (click)="activeTab.set('security')" role="tab"
          [attr.aria-selected]="activeTab() === 'security'">Security</button>
        <button class="tab" [class.tab-active]="activeTab() === 'notifications'" (click)="activeTab.set('notifications')" role="tab"
          [attr.aria-selected]="activeTab() === 'notifications'">Notifications</button>
        <button class="tab" [class.tab-active]="activeTab() === 'billing'" (click)="activeTab.set('billing')" role="tab"
          [attr.aria-selected]="activeTab() === 'billing'">Billing</button>
        <button class="tab" [class.tab-active]="activeTab() === 'privacy'" (click)="activeTab.set('privacy')" role="tab"
          [attr.aria-selected]="activeTab() === 'privacy'">Privacy</button>
        <button class="tab" [class.tab-active]="activeTab() === 'appearance'" (click)="activeTab.set('appearance')" role="tab"
          [attr.aria-selected]="activeTab() === 'appearance'">Appearance</button>
      </div>

      <!-- Profile Tab -->
      @if (activeTab() === 'profile') {
        <div class="card bg-base-200 p-6 space-y-6">
          <!-- Avatar -->
          <div class="flex items-center gap-4">
            <div class="avatar placeholder">
              <div class="w-20 h-20 rounded-full bg-neutral text-neutral-content">
                @if (avatarUrl()) {
                  <img [src]="avatarUrl()" alt="User avatar" class="rounded-full" />
                } @else {
                  <span class="text-2xl">{{ displayName().charAt(0) }}</span>
                }
              </div>
            </div>
            <div>
              <input type="file" class="file-input file-input-bordered file-input-sm"
                accept=".jpg,.jpeg,.png,.webp,.gif" (change)="onAvatarChange($event)"
                aria-label="Upload avatar image" />
              <p class="text-xs text-base-content/60 mt-1">JPG, PNG, WebP, or GIF. Max 5 MB.</p>
            </div>
          </div>

          <!-- Display Name -->
          <div class="form-control">
            <label class="label" for="display-name"><span class="label-text">Display Name</span></label>
            <input id="display-name" type="text" class="input input-bordered"
              [(ngModel)]="displayName" minlength="3" maxlength="50"
              aria-describedby="name-hint" />
            <span id="name-hint" class="label-text-alt mt-1">3–50 characters ({{ displayName().length }}/50)</span>
          </div>

          <!-- Bio -->
          <div class="form-control">
            <label class="label" for="bio"><span class="label-text">Bio</span></label>
            <textarea id="bio" class="textarea textarea-bordered h-24"
              [(ngModel)]="bio" maxlength="500"
              aria-describedby="bio-hint"></textarea>
            <span id="bio-hint" class="label-text-alt mt-1">{{ bio().length }}/500 characters</span>
          </div>

          <!-- Timezone -->
          <div class="form-control">
            <label class="label" for="timezone"><span class="label-text">Timezone</span></label>
            <select id="timezone" class="select select-bordered" [(ngModel)]="timezone">
              <option value="">Select timezone</option>
              <option value="Europe/London">Europe/London (GMT/BST)</option>
              <option value="Europe/Dublin">Europe/Dublin (GMT/IST)</option>
              <option value="Europe/Paris">Europe/Paris (CET/CEST)</option>
              <option value="America/New_York">America/New York (EST/EDT)</option>
              <option value="America/Chicago">America/Chicago (CST/CDT)</option>
              <option value="America/Los_Angeles">America/Los Angeles (PST/PDT)</option>
              <option value="Australia/Sydney">Australia/Sydney (AEST/AEDT)</option>
            </select>
          </div>

          <!-- Save Button -->
          <div class="flex justify-end">
            <button class="btn btn-primary" (click)="saveProfile()" [disabled]="saving()">
              {{ saving() ? 'Saving...' : 'Save Changes' }}
            </button>
          </div>

          @if (profileMessage()) {
            <div class="alert" [class.alert-success]="!profileError()" [class.alert-error]="profileError()">
              <span>{{ profileMessage() }}</span>
            </div>
          }
        </div>

        <!-- Activity Log -->
        <div class="card bg-base-200 p-6">
          <h2 class="text-lg font-semibold mb-4">Activity Log</h2>
          @if (activityLoading()) {
            <div class="skeleton h-32 w-full"></div>
          } @else {
            <div class="overflow-x-auto">
              <table class="table table-sm" aria-label="User activity log">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Activity</th>
                    <th>IP Address</th>
                  </tr>
                </thead>
                <tbody>
                  @for (entry of activityEntries(); track entry.id) {
                    <tr>
                      <td class="whitespace-nowrap">{{ entry.timestamp | date:'medium' }}</td>
                      <td>{{ entry.description }}</td>
                      <td class="font-mono text-xs">{{ entry.ipAddress || '—' }}</td>
                    </tr>
                  } @empty {
                    <tr>
                      <td colspan="3" class="text-center py-4 text-base-content/60">No activity recorded yet.</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            @if (activityTotalPages() > 1) {
              <div class="flex justify-center mt-4">
                <div class="join">
                  <button class="join-item btn btn-sm" [disabled]="activityPage() <= 1"
                    (click)="loadActivity(activityPage() - 1)">«</button>
                  <button class="join-item btn btn-sm btn-active">{{ activityPage() }}</button>
                  <button class="join-item btn btn-sm" [disabled]="activityPage() >= activityTotalPages()"
                    (click)="loadActivity(activityPage() + 1)">»</button>
                </div>
              </div>
            }
          }
        </div>
      }

      <!-- Security Tab -->
      @if (activeTab() === 'security') {
        <div class="card bg-base-200 p-6 space-y-4">
          <h2 class="text-lg font-semibold">Security Settings</h2>
          <div class="form-control">
            <label class="label"><span class="label-text">Two-Factor Authentication</span></label>
            <p class="text-sm text-base-content/60 mb-2">Add an extra layer of security to your account.</p>
            <button class="btn btn-outline btn-sm w-fit">Configure 2FA</button>
          </div>
          <div class="divider"></div>
          <div class="form-control">
            <label class="label"><span class="label-text">Change Password</span></label>
            <button class="btn btn-outline btn-sm w-fit">Change Password</button>
          </div>
          <div class="divider"></div>
          <div class="form-control">
            <label class="label"><span class="label-text">Active Sessions</span></label>
            <p class="text-sm text-base-content/60">View and manage your active sessions.</p>
          </div>
        </div>
      }

      <!-- Notifications Tab -->
      @if (activeTab() === 'notifications') {
        <div class="card bg-base-200 p-6 space-y-4">
          <h2 class="text-lg font-semibold">Notification Preferences</h2>
          <div class="form-control">
            <label class="label cursor-pointer">
              <span class="label-text">Email notifications</span>
              <input type="checkbox" class="toggle toggle-primary" checked />
            </label>
          </div>
          <div class="form-control">
            <label class="label cursor-pointer">
              <span class="label-text">Push notifications</span>
              <input type="checkbox" class="toggle toggle-primary" />
            </label>
          </div>
          <div class="form-control">
            <label class="label cursor-pointer">
              <span class="label-text">Telegram notifications</span>
              <input type="checkbox" class="toggle toggle-primary" />
            </label>
          </div>
          <div class="form-control">
            <label class="label cursor-pointer">
              <span class="label-text">New tip alerts</span>
              <input type="checkbox" class="toggle toggle-primary" checked />
            </label>
          </div>
          <div class="form-control">
            <label class="label cursor-pointer">
              <span class="label-text">Result updates</span>
              <input type="checkbox" class="toggle toggle-primary" checked />
            </label>
          </div>
        </div>
      }

      <!-- Billing Tab -->
      @if (activeTab() === 'billing') {
        <div class="card bg-base-200 p-6 space-y-4">
          <h2 class="text-lg font-semibold">Billing & Subscription</h2>
          <p class="text-sm text-base-content/60">Manage your subscription and payment details from the billing page.</p>
          <a class="btn btn-primary btn-sm w-fit" routerLink="/subscriber/billing">Go to Billing</a>
        </div>
      }

      <!-- Privacy Tab -->
      @if (activeTab() === 'privacy') {
        <div class="card bg-base-200 p-6 space-y-4">
          <h2 class="text-lg font-semibold">Privacy & Data</h2>
          <div class="form-control">
            <label class="label"><span class="label-text">Export My Data</span></label>
            <p class="text-sm text-base-content/60 mb-2">Download a copy of your personal data.</p>
            <button class="btn btn-outline btn-sm w-fit">Request Data Export</button>
          </div>
          <div class="divider"></div>
          <div class="form-control">
            <label class="label"><span class="label-text text-error">Delete Account</span></label>
            <p class="text-sm text-base-content/60 mb-2">Permanently delete your account and all associated data. This cannot be undone.</p>
            <button class="btn btn-error btn-outline btn-sm w-fit">Delete Account</button>
          </div>
        </div>
      }

      <!-- Appearance Tab -->
      @if (activeTab() === 'appearance') {
        <div class="card bg-base-200 p-6 space-y-4">
          <h2 class="text-lg font-semibold">Appearance</h2>
          <div class="form-control">
            <label class="label"><span class="label-text">Theme</span></label>
            <div class="flex gap-4">
              <label class="label cursor-pointer gap-2">
                <input type="radio" name="theme" class="radio radio-primary" value="light"
                  [checked]="currentTheme() === 'light'" (change)="setTheme('light')" />
                <span>Light</span>
              </label>
              <label class="label cursor-pointer gap-2">
                <input type="radio" name="theme" class="radio radio-primary" value="dark"
                  [checked]="currentTheme() === 'dark'" (change)="setTheme('dark')" />
                <span>Dark</span>
              </label>
              <label class="label cursor-pointer gap-2">
                <input type="radio" name="theme" class="radio radio-primary" value="system"
                  [checked]="currentTheme() === 'system'" (change)="setTheme('system')" />
                <span>System</span>
              </label>
            </div>
          </div>
        </div>
      }
    </section>
  `,
})
export class ProfileComponent implements OnInit {
  private readonly userService = inject(UserService);

  activeTab = signal<ProfileTab>('profile');
  displayName = signal('');
  bio = signal('');
  timezone = signal('');
  avatarUrl = signal<string | null>(null);
  saving = signal(false);
  profileMessage = signal<string | null>(null);
  profileError = signal(false);
  currentTheme = signal(localStorage.getItem('theme') || 'system');

  // Activity log
  activityEntries = signal<ActivityEntry[]>([]);
  activityLoading = signal(false);
  activityPage = signal(1);
  activityTotalPages = signal(1);

  ngOnInit(): void {
    this.loadProfile();
    this.loadActivity(1);
  }

  private loadProfile(): void {
    this.userService.getProfile().subscribe({
      next: (profile: any) => {
        this.displayName.set(profile.displayName || '');
        this.bio.set(profile.bio || '');
        this.timezone.set(profile.timeZone || '');
        this.avatarUrl.set(profile.avatarUrl);
      },
    });
  }

  saveProfile(): void {
    const name = this.displayName();
    if (name.length < 3 || name.length > 50) {
      this.profileMessage.set('Display name must be between 3 and 50 characters.');
      this.profileError.set(true);
      return;
    }
    if (this.bio().length > 500) {
      this.profileMessage.set('Bio must be at most 500 characters.');
      this.profileError.set(true);
      return;
    }

    this.saving.set(true);
    this.profileMessage.set(null);

    this.userService.updateProfile({
      displayName: this.displayName(),
      bio: this.bio(),
      timezone: this.timezone(),
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.profileMessage.set('Profile updated successfully.');
        this.profileError.set(false);
      },
      error: () => {
        this.saving.set(false);
        this.profileMessage.set('Failed to update profile. Please try again.');
        this.profileError.set(true);
      },
    });
  }

  onAvatarChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files?.[0];
    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
      this.profileMessage.set('File must not exceed 5 MB.');
      this.profileError.set(true);
      return;
    }

    const validTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    if (!validTypes.includes(file.type)) {
      this.profileMessage.set('Accepted file types are JPG, PNG, WebP, and GIF.');
      this.profileError.set(true);
      return;
    }

    this.userService.uploadAvatar(file).subscribe({
      next: (res) => {
        this.avatarUrl.set(res.avatarUrl);
        this.profileMessage.set('Avatar uploaded successfully.');
        this.profileError.set(false);
      },
      error: () => {
        this.profileMessage.set('Failed to upload avatar.');
        this.profileError.set(true);
      },
    });
  }

  loadActivity(page: number): void {
    this.activityLoading.set(true);
    this.activityPage.set(page);

    // Activity is fetched from profile/activity endpoint
    const http = (this.userService as any).http;
    const apiUrl = (this.userService as any).apiUrl?.replace('/users', '/profile') || '/api/profile';

    http.get(`${apiUrl}/activity`, { params: { page } }).subscribe({
      next: (res: any) => {
        this.activityEntries.set(res.entries || []);
        this.activityTotalPages.set(res.totalPages || 1);
        this.activityLoading.set(false);
      },
      error: () => {
        this.activityLoading.set(false);
      },
    });
  }

  setTheme(theme: string): void {
    this.currentTheme.set(theme);
    localStorage.setItem('theme', theme);
    if (theme === 'system') {
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      document.documentElement.setAttribute('data-theme', prefersDark ? 'dark' : 'light');
    } else {
      document.documentElement.setAttribute('data-theme', theme);
    }
  }
}
