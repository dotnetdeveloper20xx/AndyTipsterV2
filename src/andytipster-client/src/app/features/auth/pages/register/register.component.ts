import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthActions } from '../../../../store/auth/auth.actions';
import { selectAuthIsLoading, selectAuthError } from '../../../../store/auth/auth.selectors';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="min-h-screen flex items-center justify-center px-4">
      <div class="card w-full max-w-md bg-base-100 shadow-xl">
        <div class="card-body">
          <h1 class="card-title text-2xl font-bold justify-center mb-2">Create Account</h1>
          <p class="text-center text-base-content/60 mb-6">Join AndyTipster today.</p>

          @if (error$ | async; as error) {
            <div class="alert alert-error mb-4" role="alert">
              <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-5 w-5" fill="none" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>{{ error }}</span>
            </div>
          }

          <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="space-y-4">
            <div class="form-control">
              <label class="label" for="displayName">
                <span class="label-text">Display Name</span>
              </label>
              <input
                id="displayName"
                type="text"
                formControlName="displayName"
                class="input input-bordered w-full"
                [class.input-error]="registerForm.get('displayName')?.touched && registerForm.get('displayName')?.invalid"
                placeholder="Your name"
                autocomplete="name"
              />
              @if (registerForm.get('displayName')?.touched && registerForm.get('displayName')?.hasError('required')) {
                <label class="label">
                  <span class="label-text-alt text-error">Display name is required</span>
                </label>
              }
            </div>

            <div class="form-control">
              <label class="label" for="email">
                <span class="label-text">Email</span>
              </label>
              <input
                id="email"
                type="email"
                formControlName="email"
                class="input input-bordered w-full"
                [class.input-error]="registerForm.get('email')?.touched && registerForm.get('email')?.invalid"
                placeholder="you&#64;example.com"
                autocomplete="email"
              />
              @if (registerForm.get('email')?.touched && registerForm.get('email')?.hasError('required')) {
                <label class="label">
                  <span class="label-text-alt text-error">Email is required</span>
                </label>
              }
              @if (registerForm.get('email')?.touched && registerForm.get('email')?.hasError('email')) {
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
                [class.input-error]="registerForm.get('password')?.touched && registerForm.get('password')?.invalid"
                placeholder="••••••••"
                autocomplete="new-password"
              />
              @if (registerForm.get('password')?.touched && registerForm.get('password')?.hasError('required')) {
                <label class="label">
                  <span class="label-text-alt text-error">Password is required</span>
                </label>
              }
              @if (registerForm.get('password')?.touched && registerForm.get('password')?.hasError('minlength')) {
                <label class="label">
                  <span class="label-text-alt text-error">Password must be at least 8 characters</span>
                </label>
              }
              <!-- Password strength indicator -->
              @if (registerForm.get('password')?.value) {
                <div class="mt-2">
                  <progress
                    class="progress w-full"
                    [class.progress-error]="passwordStrength <= 1"
                    [class.progress-warning]="passwordStrength === 2"
                    [class.progress-success]="passwordStrength >= 3"
                    [value]="passwordStrength * 25"
                    max="100"
                  ></progress>
                  <span class="label-text-alt">
                    {{ passwordStrengthLabel }}
                  </span>
                </div>
              }
            </div>

            <div class="form-control">
              <label class="label" for="confirmPassword">
                <span class="label-text">Confirm Password</span>
              </label>
              <input
                id="confirmPassword"
                type="password"
                formControlName="confirmPassword"
                class="input input-bordered w-full"
                [class.input-error]="registerForm.get('confirmPassword')?.touched && registerForm.get('confirmPassword')?.invalid"
                placeholder="••••••••"
                autocomplete="new-password"
              />
              @if (registerForm.get('confirmPassword')?.touched && registerForm.get('confirmPassword')?.hasError('required')) {
                <label class="label">
                  <span class="label-text-alt text-error">Please confirm your password</span>
                </label>
              }
              @if (registerForm.get('confirmPassword')?.touched && registerForm.get('confirmPassword')?.hasError('passwordMismatch')) {
                <label class="label">
                  <span class="label-text-alt text-error">Passwords do not match</span>
                </label>
              }
            </div>

            <button
              type="submit"
              class="btn btn-primary w-full"
              [disabled]="registerForm.invalid || (isLoading$ | async)"
            >
              @if (isLoading$ | async) {
                <span class="loading loading-spinner loading-sm"></span>
              }
              Create Account
            </button>
          </form>

          <p class="text-center mt-4">
            Already have an account?
            <a routerLink="/auth/login" class="link link-primary">Sign in</a>
          </p>
        </div>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);

  readonly isLoading$ = this.store.select(selectAuthIsLoading);
  readonly error$ = this.store.select(selectAuthError);

  registerForm = this.fb.nonNullable.group({
    displayName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required, this.confirmPasswordValidator.bind(this)]],
  });

  ngOnInit(): void {
    this.store.dispatch(AuthActions.clearError());
  }

  get passwordStrength(): number {
    const password = this.registerForm.get('password')?.value ?? '';
    let strength = 0;
    if (password.length >= 8) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^A-Za-z0-9]/.test(password)) strength++;
    return strength;
  }

  get passwordStrengthLabel(): string {
    switch (this.passwordStrength) {
      case 0: return 'Very weak';
      case 1: return 'Weak';
      case 2: return 'Fair';
      case 3: return 'Strong';
      case 4: return 'Very strong';
      default: return '';
    }
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      const { email, password, displayName } = this.registerForm.getRawValue();
      this.store.dispatch(AuthActions.register({ email, password, displayName }));
    }
  }

  private confirmPasswordValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.parent) return null;
    const password = control.parent.get('password')?.value;
    const confirmPassword = control.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }
}
