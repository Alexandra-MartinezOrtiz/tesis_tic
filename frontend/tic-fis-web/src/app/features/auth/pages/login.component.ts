import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgIf } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  template: `
    <div class="login-wrap">
      <div class="login-card">
        <div class="login-brand">
          <div class="brand-shield-lg">FIS</div>
          <div>
            <div style="font-size:1.3rem;font-weight:700;color:var(--navy)">TIC-FIS</div>
            <div style="font-size:.72rem;color:#9e9e9e">Escuela Politécnica Nacional</div>
          </div>
        </div>
        <p class="login-sub">Gestión de Propuestas de Trabajo de Integración Curricular</p>

        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="form-group">
            <label for="email">Correo institucional</label>
            <input
              id="email"
              type="email"
              formControlName="email"
              placeholder="usuario@universidad.edu.ec"
              autocomplete="email"
            />
            <span class="form-error" *ngIf="form.controls.email.touched && form.controls.email.invalid">
              Ingresa un correo válido.
            </span>
          </div>

          <div class="form-group">
            <label for="password">Contraseña</label>
            <input
              id="password"
              type="password"
              formControlName="password"
              placeholder="••••••••"
              autocomplete="current-password"
            />
            <span class="form-error" *ngIf="form.controls.password.touched && form.controls.password.invalid">
              La contraseña es obligatoria.
            </span>
          </div>

          <div class="form-error mb" *ngIf="errorMsg()">{{ errorMsg() }}</div>

          <button type="submit" class="btn btn-primary btn-block" [disabled]="loading()">
            {{ loading() ? 'Ingresando…' : 'Ingresar' }}
          </button>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .login-wrap {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--navy);
      background: linear-gradient(135deg, #0E2240 0%, #1a3a6b 100%);
    }
    .login-card {
      background: #fff;
      border-radius: 12px;
      box-shadow: 0 12px 40px rgba(0,0,0,.35);
      padding: 2.25rem 2.5rem;
      width: 100%;
      max-width: 390px;
      border-top: 4px solid var(--gold);
    }
    .login-brand {
      display: flex;
      align-items: center;
      gap: .85rem;
      margin-bottom: .75rem;
    }
    .brand-shield-lg {
      width: 46px;
      height: 46px;
      background: var(--navy);
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 1rem;
      color: var(--gold);
      flex-shrink: 0;
    }
    .login-sub {
      margin: 0 0 1.75rem;
      color: #9e9e9e;
      font-size: .75rem;
      line-height: 1.5;
    }
    .btn-block { width: 100%; justify-content: center; padding: .65rem; font-size: .88rem; }
    .mb { margin-bottom: .75rem; }
  `],
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  loading = signal(false);
  errorMsg = signal('');

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.errorMsg.set('');

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/propuestas']),
      error: () => {
        this.errorMsg.set('Credenciales incorrectas. Intenta nuevamente.');
        this.loading.set(false);
      },
    });
  }
}
