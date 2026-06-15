import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthActions } from '../../../../store/auth/auth.actions';
import { selectAuthIsLoading, selectAuthError } from '../../../../store/auth/auth.selectors';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="min-h-screen flex items-center justify-center px-4">
      <div class="card w-full max-w-md bg-base-100 shadow-xl">
        <div class="card-body">
          <h1 class="card-title text-2xl font-bold justify-center mb-2">Reset Password</h1>

          @if (!submitted()) {
            <p class="text-center text-base-content/60 mb-6">
              Enter your email address and we'll send you a link to reset your password.
            </p>

            @if (error$ | async; as error) {
              <div class="alert alert-error mb-4" role="alert">
                <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-5 w-5" fill="none" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <span>{{ error }}</span>
              </div>
            }

            <form [formGroup]="forgotForm" (ngSubmit)="onSubmit()" class="space-y-4">
              <div class="form-control">
                <label class="label" for="email">
                  <span class="label-text">Email</span>
                </label>
                <input
                  id="email"
                  type="email"
                  formControlName="email"
                  class="input input-bordered w-full"
                  [class.input-error]="forgotForm.get('email')?.touched && forgotForm.get('email')?.invalid"
                  placeholder="you&#64;example.com"
                  autocomplete="email"
                />
                @if (forgotForm.get('email')?.touched && forgotForm.get('email')?.hasError('required')) {
                  <label class="label">
                    <span class="label-text-alt text-error">Email is required</span>
                  </label>
                }
                @if (forgotForm.get('email')?.touched && forgotForm.get('email')?.hasError('email')) {
                  <label class="label">
                    <span class="label-text-alt text-error">Please enter a valid email</span>
                  </label>
                }
              </div>

              <button
                type="submit"
                class="btn btn-primary w-full"
                [disabled]="forgotForm.invalid || (isLoading$ | async)"
              >
                @if (isLoading$ | async) {
                  <span class="loading loading-spinner loading-sm"></span>
                }
                Send Reset Link
              </button>
            </form>
          } @else {
            <div class="text-center py-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto text-success mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p class="text-base-content/80">
                If an account exists with that email, we've sent a password reset link. Please check your inbox.
              </p>
            </div>
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
export class ForgotPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);

  readonly isLoading$ = this.store.select(selectAuthIsLoading);
  readonly error$ = this.store.select(selectAuthError);
  readonly submitted = signal(false);

  forgotForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  ngOnInit(): void {
    this.store.dispatch(AuthActions.clearError());
  }

  onSubmit(): void {
    if (this.forgotForm.valid) {
      const { email } = this.forgotForm.getRawValue();
      this.store.dispatch(AuthActions.forgotPassword({ email }));
      this.submitted.set(true);
    }
  }
}
