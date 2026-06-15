import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthActions } from '../../../../store/auth/auth.actions';
import { selectAuthIsLoading, selectAuthError, selectTwoFactorEmail } from '../../../../store/auth/auth.selectors';

@Component({
  selector: 'app-two-factor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="min-h-screen flex items-center justify-center px-4">
      <div class="card w-full max-w-md bg-base-100 shadow-xl">
        <div class="card-body">
          <h1 class="card-title text-2xl font-bold justify-center mb-2">Two-Factor Authentication</h1>

          @if (!useRecoveryCode()) {
            <p class="text-center text-base-content/60 mb-6">
              Enter the 6-digit code from your authenticator app.
            </p>
          } @else {
            <p class="text-center text-base-content/60 mb-6">
              Enter one of your recovery codes.
            </p>
          }

          @if (error$ | async; as error) {
            <div class="alert alert-error mb-4" role="alert">
              <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-5 w-5" fill="none" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>{{ error }}</span>
            </div>
          }

          @if (!useRecoveryCode()) {
            <form [formGroup]="totpForm" (ngSubmit)="onSubmitTotp()" class="space-y-4">
              <div class="form-control">
                <label class="label" for="code">
                  <span class="label-text">Verification Code</span>
                </label>
                <input
                  id="code"
                  type="text"
                  formControlName="code"
                  class="input input-bordered w-full text-center text-2xl tracking-widest"
                  [class.input-error]="totpForm.get('code')?.touched && totpForm.get('code')?.invalid"
                  placeholder="000000"
                  maxlength="6"
                  autocomplete="one-time-code"
                  inputmode="numeric"
                />
                @if (totpForm.get('code')?.touched && totpForm.get('code')?.hasError('required')) {
                  <label class="label">
                    <span class="label-text-alt text-error">Code is required</span>
                  </label>
                }
                @if (totpForm.get('code')?.touched && totpForm.get('code')?.hasError('pattern')) {
                  <label class="label">
                    <span class="label-text-alt text-error">Code must be 6 digits</span>
                  </label>
                }
              </div>

              <button
                type="submit"
                class="btn btn-primary w-full"
                [disabled]="totpForm.invalid || (isLoading$ | async)"
              >
                @if (isLoading$ | async) {
                  <span class="loading loading-spinner loading-sm"></span>
                }
                Verify
              </button>
            </form>

            <button class="btn btn-ghost btn-sm mt-2 w-full" (click)="toggleRecoveryCode()">
              Use a recovery code instead
            </button>
          } @else {
            <form [formGroup]="recoveryForm" (ngSubmit)="onSubmitRecovery()" class="space-y-4">
              <div class="form-control">
                <label class="label" for="recoveryCode">
                  <span class="label-text">Recovery Code</span>
                </label>
                <input
                  id="recoveryCode"
                  type="text"
                  formControlName="recoveryCode"
                  class="input input-bordered w-full text-center font-mono"
                  [class.input-error]="recoveryForm.get('recoveryCode')?.touched && recoveryForm.get('recoveryCode')?.invalid"
                  placeholder="xxxxxxxx"
                  autocomplete="off"
                />
                @if (recoveryForm.get('recoveryCode')?.touched && recoveryForm.get('recoveryCode')?.hasError('required')) {
                  <label class="label">
                    <span class="label-text-alt text-error">Recovery code is required</span>
                  </label>
                }
              </div>

              <button
                type="submit"
                class="btn btn-primary w-full"
                [disabled]="recoveryForm.invalid || (isLoading$ | async)"
              >
                @if (isLoading$ | async) {
                  <span class="loading loading-spinner loading-sm"></span>
                }
                Verify Recovery Code
              </button>
            </form>

            <button class="btn btn-ghost btn-sm mt-2 w-full" (click)="toggleRecoveryCode()">
              Use authenticator app instead
            </button>
          }

          <p class="text-center mt-4">
            <a routerLink="/auth/login" class="link link-primary">Back to Sign In</a>
          </p>
        </div>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TwoFactorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);

  readonly isLoading$ = this.store.select(selectAuthIsLoading);
  readonly error$ = this.store.select(selectAuthError);
  readonly twoFactorEmail$ = this.store.select(selectTwoFactorEmail);
  readonly useRecoveryCode = signal(false);

  private email = '';

  totpForm = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
  });

  recoveryForm = this.fb.nonNullable.group({
    recoveryCode: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.store.dispatch(AuthActions.clearError());
    this.twoFactorEmail$.subscribe((email) => {
      this.email = email ?? '';
    });
  }

  onSubmitTotp(): void {
    if (this.totpForm.valid) {
      const { code } = this.totpForm.getRawValue();
      this.store.dispatch(AuthActions.verify2FA({ email: this.email, code }));
    }
  }

  onSubmitRecovery(): void {
    if (this.recoveryForm.valid) {
      const { recoveryCode } = this.recoveryForm.getRawValue();
      this.store.dispatch(AuthActions.verifyRecoveryCode({ email: this.email, code: recoveryCode }));
    }
  }

  toggleRecoveryCode(): void {
    this.useRecoveryCode.set(!this.useRecoveryCode());
    this.store.dispatch(AuthActions.clearError());
  }
}
