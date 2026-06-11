import { Component, inject, signal, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { UsuarioService } from '../../../core/services/usuario.service';
import { UsuarioListItemDto } from '../../../core/models/usuario.models';

@Component({
  selector: 'app-usuarios-home',
  standalone: true,
  imports: [NgFor, NgIf, ReactiveFormsModule],
  template: `
    <div class="page-header">
      <h2 class="page-title">Gestión de Usuarios</h2>
      <button class="btn btn-primary" (click)="mostrarFormNuevo.set(!mostrarFormNuevo())">
        {{ mostrarFormNuevo() ? 'Cancelar' : '+ Nuevo usuario' }}
      </button>
    </div>

    <!-- Formulario crear usuario -->
    <div class="card" style="max-width:560px;margin-bottom:1.5rem" *ngIf="mostrarFormNuevo()" [formGroup]="nuevoForm">
      <h3 style="margin:0 0 1rem">Nuevo usuario</h3>
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:.75rem">
        <div class="form-group" style="margin:0">
          <label>Nombres *</label>
          <input formControlName="nombres" />
          <span class="form-error" *ngIf="nf['nombres'].touched && nf['nombres'].invalid">Obligatorio.</span>
        </div>
        <div class="form-group" style="margin:0">
          <label>Apellidos *</label>
          <input formControlName="apellidos" />
          <span class="form-error" *ngIf="nf['apellidos'].touched && nf['apellidos'].invalid">Obligatorio.</span>
        </div>
      </div>
      <div class="form-group" style="margin-top:.75rem">
        <label>Correo *</label>
        <input type="email" formControlName="email" />
        <span class="form-error" *ngIf="nf['email'].touched && nf['email'].invalid">Correo válido requerido.</span>
      </div>
      <div class="form-group">
        <label>Contraseña *</label>
        <input type="password" formControlName="password" />
        <span class="form-error" *ngIf="nf['password'].touched && nf['password'].invalid">Mínimo 8 caracteres.</span>
      </div>
      <div class="form-error" *ngIf="nuevoError()">{{ nuevoError() }}</div>
      <button class="btn btn-primary" (click)="crearUsuario()" [disabled]="guardandoNuevo()">
        {{ guardandoNuevo() ? 'Guardando…' : 'Crear usuario' }}
      </button>
    </div>

    <!-- Tabla de usuarios -->
    <div *ngIf="loading()" class="empty-state">Cargando…</div>
    <div *ngIf="error()" class="empty-state" style="color:var(--color-danger)">{{ error() }}</div>

    <div class="table-wrap" *ngIf="!loading() && !error()">
      <table>
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Correo</th>
            <th>Roles</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let u of usuarios()">
            <td>{{ u.nombres }} {{ u.apellidos }}</td>
            <td>{{ u.email }}</td>
            <td>{{ u.roles.join(', ') || '—' }}</td>
            <td>
              <span class="badge" [class.badge-aprobada]="u.activo" [class.badge-rechazada]="!u.activo">
                {{ u.activo ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td>
              <button class="btn btn-outline btn-sm" (click)="toggleActivo(u)">
                {{ u.activo ? 'Desactivar' : 'Activar' }}
              </button>
            </td>
          </tr>
          <tr *ngIf="usuarios().length === 0">
            <td colspan="5" class="empty-state">No hay usuarios registrados.</td>
          </tr>
        </tbody>
      </table>
    </div>
  `,
})
export class UsuariosHomeComponent implements OnInit {
  private svc = inject(UsuarioService);
  private fb = inject(FormBuilder);

  loading = signal(true);
  error = signal('');
  usuarios = signal<UsuarioListItemDto[]>([]);
  mostrarFormNuevo = signal(false);
  guardandoNuevo = signal(false);
  nuevoError = signal('');

  nuevoForm = this.fb.nonNullable.group({
    nombres: ['', Validators.required],
    apellidos: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  get nf() { return this.nuevoForm.controls; }

  ngOnInit() { this.cargar(); }

  cargar() {
    this.svc.getAll().subscribe({
      next: (data) => { this.usuarios.set(data); this.loading.set(false); },
      error: () => { this.error.set('Error al cargar usuarios.'); this.loading.set(false); },
    });
  }

  crearUsuario() {
    if (this.nuevoForm.invalid) { this.nuevoForm.markAllAsTouched(); return; }
    this.guardandoNuevo.set(true);
    this.nuevoError.set('');
    this.svc.create(this.nuevoForm.getRawValue()).subscribe({
      next: () => {
        this.guardandoNuevo.set(false);
        this.mostrarFormNuevo.set(false);
        this.nuevoForm.reset();
        this.cargar();
      },
      error: () => { this.nuevoError.set('Error al crear usuario.'); this.guardandoNuevo.set(false); },
    });
  }

  toggleActivo(u: UsuarioListItemDto) {
    this.svc.update(u.id, {
      nombres: u.nombres,
      apellidos: u.apellidos,
      email: u.email,
      activo: !u.activo,
    }).subscribe({ next: () => this.cargar() });
  }
}
