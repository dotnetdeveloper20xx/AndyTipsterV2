import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PlansService, PromoCode, CreatePromoCodeRequest } from '../../../../core/services/plans.service';
import { Plan } from '../../../../store/plans/plans.state';

@Component({
  selector: 'app-plan-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold">Plan Management</h1>
        <div class="flex gap-2">
          <button class="btn btn-primary" (click)="showCreatePlan.set(true)">+ New Plan</button>
          <button class="btn btn-secondary" (click)="showCreatePromo.set(true)">+ Promo Code</button>
        </div>
      </div>

      <!-- Plans List -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
        @for (plan of plans(); track plan.id) {
          <div class="card bg-base-100 shadow-xl" [class.opacity-50]="plan.isArchived">
            <div class="card-body">
              <h2 class="card-title">{{ plan.name }}
                @if (plan.isArchived) { <span class="badge badge-ghost">Archived</span> }
                @if (plan.syncStatus === 'SyncPending') { <span class="badge badge-warning">Sync Pending</span> }
                @if (plan.syncStatus === 'SyncFailed') { <span class="badge badge-error">Sync Failed</span> }
              </h2>
              <p class="text-2xl font-bold">{{ plan.currency }} {{ plan.price }} / {{ plan.billingCycle }}</p>
              @if (plan.trialPeriodDays > 0) {
                <p class="text-sm text-success">{{ plan.trialPeriodDays }}-day free trial</p>
              }
              <div class="card-actions justify-end mt-2">
                @if (plan.syncStatus === 'SyncFailed') {
                  <button class="btn btn-xs btn-warning" (click)="retrySyncPlan(plan.id)">Retry Sync</button>
                }
                <button class="btn btn-xs btn-outline" (click)="syncPlan(plan.id)">Sync PayPal</button>
                @if (!plan.isArchived) {
                  <button class="btn btn-xs btn-error btn-outline" (click)="archivePlan(plan.id)">Archive</button>
                }
              </div>
            </div>
          </div>
        }
      </div>

      <!-- Promo Codes Section -->
      <h2 class="text-2xl font-bold mb-4">Promo Codes</h2>
      <div class="overflow-x-auto">
        <table class="table">
          <thead>
            <tr><th>Code</th><th>Type</th><th>Value</th><th>Usage</th><th>Expires</th><th>Active</th><th>Actions</th></tr>
          </thead>
          <tbody>
            @for (code of promoCodes(); track code.id) {
              <tr>
                <td class="font-mono">{{ code.code }}</td>
                <td>{{ code.discountType }}</td>
                <td>{{ code.discountType === 'percentage' ? code.discountValue + '%' : '£' + code.discountValue }}</td>
                <td>{{ code.currentUses }} / {{ code.maxUses }}</td>
                <td>{{ code.expiresAt ? (code.expiresAt | date:'short') : 'Never' }}</td>
                <td><input type="checkbox" class="toggle toggle-sm" [checked]="code.isActive" (change)="togglePromoCode(code)" /></td>
                <td><button class="btn btn-xs btn-error btn-ghost" (click)="deletePromoCode(code.id)">Delete</button></td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <!-- Create Plan Modal -->
      @if (showCreatePlan()) {
        <div class="modal modal-open">
          <div class="modal-box max-w-lg">
            <h3 class="font-bold text-lg">Create New Plan</h3>
            <div class="form-control mt-4">
              <label class="label"><span class="label-text">Name</span></label>
              <input type="text" class="input input-bordered" [(ngModel)]="newPlan.name" maxlength="100" />
            </div>
            <div class="grid grid-cols-2 gap-4 mt-2">
              <div class="form-control">
                <label class="label"><span class="label-text">Price</span></label>
                <input type="number" class="input input-bordered" [(ngModel)]="newPlan.price" min="0.01" max="999999.99" step="0.01" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text">Currency</span></label>
                <select class="select select-bordered" [(ngModel)]="newPlan.currency">
                  <option value="GBP">GBP</option><option value="EUR">EUR</option><option value="USD">USD</option>
                </select>
              </div>
            </div>
            <div class="form-control mt-2">
              <label class="label"><span class="label-text">Billing Cycle</span></label>
              <select class="select select-bordered" [(ngModel)]="newPlan.billingCycle">
                <option value="Weekly">Weekly</option><option value="Monthly">Monthly</option>
                <option value="Quarterly">Quarterly</option><option value="SemiAnnual">Semi-Annual</option>
                <option value="Annual">Annual</option>
              </select>
            </div>
            <div class="grid grid-cols-3 gap-4 mt-2">
              <div class="form-control">
                <label class="label"><span class="label-text">Trial Days</span></label>
                <input type="number" class="input input-bordered" [(ngModel)]="newPlan.trialPeriodDays" min="0" max="365" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text">Setup Fee</span></label>
                <input type="number" class="input input-bordered" [(ngModel)]="newPlan.setupFee" min="0" step="0.01" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text">Grace Days</span></label>
                <input type="number" class="input input-bordered" [(ngModel)]="newPlan.gracePeriodDays" min="0" max="90" />
              </div>
            </div>
            <div class="form-control mt-2">
              <label class="label"><span class="label-text">Features (one per line)</span></label>
              <textarea class="textarea textarea-bordered" rows="4" [(ngModel)]="featuresText" placeholder="Feature 1&#10;Feature 2"></textarea>
            </div>
            <div class="modal-action">
              <button class="btn" (click)="showCreatePlan.set(false)">Cancel</button>
              <button class="btn btn-primary" (click)="createPlan()">Create Plan</button>
            </div>
          </div>
        </div>
      }

      <!-- Create Promo Code Modal -->
      @if (showCreatePromo()) {
        <div class="modal modal-open">
          <div class="modal-box">
            <h3 class="font-bold text-lg">Create Promo Code</h3>
            <div class="form-control mt-4">
              <label class="label"><span class="label-text">Code</span></label>
              <input type="text" class="input input-bordered" [(ngModel)]="newPromo.code" placeholder="SUMMER20" />
            </div>
            <div class="grid grid-cols-2 gap-4 mt-2">
              <div class="form-control">
                <label class="label"><span class="label-text">Discount Type</span></label>
                <select class="select select-bordered" [(ngModel)]="newPromo.discountType">
                  <option value="percentage">Percentage</option><option value="fixed">Fixed Amount</option>
                </select>
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text">Value</span></label>
                <input type="number" class="input input-bordered" [(ngModel)]="newPromo.discountValue" min="1" />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-4 mt-2">
              <div class="form-control">
                <label class="label"><span class="label-text">Max Uses</span></label>
                <input type="number" class="input input-bordered" [(ngModel)]="newPromo.maxUses" min="1" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text">Expires</span></label>
                <input type="date" class="input input-bordered" [(ngModel)]="newPromo.expiresAt" />
              </div>
            </div>
            <div class="modal-action">
              <button class="btn" (click)="showCreatePromo.set(false)">Cancel</button>
              <button class="btn btn-primary" (click)="createPromoCode()">Create</button>
            </div>
          </div>
        </div>
      }
    </section>
  `,
})
export class PlanManagementComponent implements OnInit {
  private readonly plansService = inject(PlansService);

  plans = signal<Plan[]>([]);
  promoCodes = signal<PromoCode[]>([]);
  showCreatePlan = signal(false);
  showCreatePromo = signal(false);

  newPlan = { name: '', price: 9.99, currency: 'GBP', billingCycle: 'Monthly', trialPeriodDays: 0, setupFee: 0, gracePeriodDays: 7 };
  featuresText = '';
  newPromo = { code: '', discountType: 'percentage', discountValue: 10, maxUses: 100, expiresAt: '' };

  ngOnInit() {
    this.loadPlans();
    this.loadPromoCodes();
  }

  private loadPlans() {
    this.plansService.getPlans().subscribe({ next: (plans) => this.plans.set(plans) });
  }

  private loadPromoCodes() {
    this.plansService.getPromoCodes().subscribe({ next: (codes) => this.promoCodes.set(codes) });
  }

  createPlan() {
    const features = this.featuresText.split('\n').map(f => f.trim()).filter(f => f.length > 0);
    this.plansService.createPlan({ ...this.newPlan, features } as Partial<Plan>).subscribe({
      next: () => { this.showCreatePlan.set(false); this.loadPlans(); },
    });
  }

  archivePlan(planId: string) {
    if (confirm('Archive this plan? Existing subscribers will not be affected.')) {
      this.plansService.archivePlan(planId).subscribe({ next: () => this.loadPlans() });
    }
  }

  syncPlan(planId: string) {
    this.plansService.syncToPayPal(planId).subscribe({ next: () => this.loadPlans() });
  }

  retrySyncPlan(planId: string) {
    this.plansService.retrySyncToPayPal(planId).subscribe({ next: () => this.loadPlans() });
  }

  createPromoCode() {
    const dto: CreatePromoCodeRequest = {
      code: this.newPromo.code,
      discountType: this.newPromo.discountType,
      discountValue: this.newPromo.discountValue,
      maxUses: this.newPromo.maxUses,
      expiresAt: this.newPromo.expiresAt || undefined,
      applicablePlanIds: [],
    };
    this.plansService.createPromoCode(dto).subscribe({
      next: () => { this.showCreatePromo.set(false); this.loadPromoCodes(); },
    });
  }

  togglePromoCode(code: PromoCode) {
    this.plansService.updatePromoCode(code.id, { isActive: !code.isActive }).subscribe({
      next: () => this.loadPromoCodes(),
    });
  }

  deletePromoCode(id: string) {
    if (confirm('Delete this promo code?')) {
      this.plansService.deletePromoCode(id).subscribe({ next: () => this.loadPromoCodes() });
    }
  }
}
