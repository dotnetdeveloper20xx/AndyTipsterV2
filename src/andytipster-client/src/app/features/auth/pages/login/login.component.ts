import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthActions } from '../../../../store/auth/auth.actions';
import { selectAuthIsLoading, selectAuthError } from '../../../../store/auth/auth.selectors';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="min-h-screen flex items-center justify-center px-4">
      <div class="card w-full max-w-md bg-base-100 shadow-xl">
        <div class="card-body">
          <h1 class="card-title text-2xl font-bold justify-center mb-2">Sign In</h1>
          <p class="text-center text-base-content/60 mb-6">Welcome back! Please enter your credentials.</p>

          @if (error$ | async; as error) {
            <div class="alert alert-error mb-4" role="alert">
              <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-5 w-5" fill="none" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>{{ error }}</span>
            </div>
          }

          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="space-y-4">
            <div class="form-control">
              <label class="label" for="email">
                <span class="label-text">Email</span>
              </label>
              <input
                id="email"
                type="email"
                formControlName="email"
                class="input input-bordered w-full"
                [class.input-error]="loginForm.get('email')?.touched && loginForm.get('email')?.invalid"
                placeholder="you&#64;example.com"
                autocomplete="email"
              />
              @if (loginForm.get('email')?.touched && loginForm.get('email')?.hasError('required')) {
                <label class="label">
                  <span class="label-text-alt text-error">Email is required</span>
                </label>
              }
              @if (loginForm.get('email')?.touched && loginForm.get('email')?.hasError('email')) {
                <label class="label">
                  <span class="label-text-alt text-error">Please enter a valid email</span>
                </label>
              }
            </div>

            <div class="form-control">
              <label class="label" for="password">
                <span class="label-text">Password</span>
              </label>
              <input
                id="password"
                type="password"
                formControlName="password"
                class="input input-bordered w-full"
                [class.input-error]="loginForm.get('password')?.touched && loginForm.get('password')?.invalid"
                placeholder="••••••••"
                autocomplete="current-password"
              />
              @if (loginForm.get('password')?.touched && loginForm.get('password')?.hasError('required')) {
                <label class="label">
                  <span class="label-text-alt text-error">Password is required</span>
                </label>
              }
              <label class="label">
                <a routerLink="/auth/forgot-password" class="label-text-alt link link-hover link-primary">Forgot password?</a>
              </label>
            </div>

            <button
              type="submit"
              class="btn btn-primary w-full"
              [disabled]="loginForm.invalid || (isLoading$ | async)"
            >
              @if (isLoading$ | async) {
                <span class="loading loading-spinner loading-sm"></span>
              }
              Sign In
            </button>
          </form>

          <div class="divider">OR</div>

          <div class="space-y-2">
            <button class="btn btn-outline w-full gap-2" (click)="socialLogin('google')" [disabled]="isLoading$ | async">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 24 24">
                <path fill="currentColor" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                <path fill="currentColor" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                <path fill="currentColor" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
                <path fill="currentColor" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
              </svg>
              Continue with Google
            </button>
            <button class="btn btn-outline w-full gap-2" (click)="socialLogin('facebook')" [disabled]="isLoading$ | async">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 24 24" fill="currentColor">
                <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/>
              </svg>
              Continue with Facebook
            </button>
          </div>

          <p class="text-center mt-4">
            Don't have an account?
            <a routerLink="/auth/register" class="link link-primary">Sign up</a>
          </p>
        </div>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);

  readonly isLoading$ = this.store.select(selectAuthIsLoading);
  readonly error$ = this.store.select(selectAuthError);

  loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.store.dispatch(AuthActions.clearError());
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.getRawValue();
      this.store.dispatch(AuthActions.login({ email, password }));
    }
  }

  socialLogin(provider: string): void {
    // Social login would typically open a popup/redirect for OAuth flow
    // For now, dispatch with empty token - integration would provide the token
    this.store.dispatch(AuthActions.socialLogin({ provider, accessToken: '' }));
  }
}
