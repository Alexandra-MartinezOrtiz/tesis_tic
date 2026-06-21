import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NgIf, NgFor, NgClass, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { PropuestaService } from '../../../core/services/propuesta.service';
import { PropuestaDetailDto, ESTADOS_PROPUESTA } from '../../../core/models/propuesta.models';

@Component({
  selector: 'app-propuesta-detalle',
  standalone: true,
  imports: [RouterLink, NgIf, NgFor, NgClass, DatePipe, ReactiveFormsModule],
  template: `
    <div *ngIf="loading()" class="empty-state">Cargando…</div>
    <div *ngIf="error()" class="empty-state" style="color:var(--color-danger)">{{ error() }}</div>

    <ng-container *ngIf="propuesta() as p">
      <div class="page-header">
        <div>
          <h2 class="page-title">{{ p.titulo }}</h2>
          <code style="color:#757575">{{ p.codigo }}</code>
        </div>
        <div class="gap-sm">
          <a routerLink="/propuestas" class="btn btn-outline">← Volver</a>
          <a [routerLink]="['editar']" class="btn btn-outline" *ngIf="puedoEditar(p.estadoActual)">
            Editar
          </a>
        </div>
      </div>

      <!-- Estado y acciones de transición -->
      <div class="card" style="margin-bottom:1rem">
        <div style="display:flex;align-items:center;gap:1rem;flex-wrap:wrap">
          <span class="badge" [ngClass]="badgeClass(p.estadoActual)" style="font-size:.9rem;padding:.3rem .8rem">
            {{ p.estadoActual }}
          </span>
          <button *ngIf="p.estadoActual === estados.Borrador"
            class="btn btn-primary btn-sm" (click)="transicion('enviar')">
            Enviar a revisión
          </button>
          <button *ngIf="p.estadoActual === estados.EnRevision"
            class="btn btn-success btn-sm" (click)="transicion('aprobar')">
            Aprobar
          </button>
          <button *ngIf="p.estadoActual === estados.EnRevision"
            class="btn btn-danger btn-sm" (click)="transicion('rechazar')">
            Rechazar
          </button>
          <button *ngIf="p.estadoActual === estados.EnRevision"
            class="btn btn-outline btn-sm" (click)="transicion('pendiente')">
            Marcar pendiente
          </button>
        </div>

        <!-- Comentario de transición -->
        <div *ngIf="accionActiva()" style="margin-top:1rem" [formGroup]="comentarioForm">
          <div class="form-group" style="margin-bottom:.5rem">
            <label>Comentario (opcional)</label>
            <textarea formControlName="comentario" rows="2"></textarea>
          </div>
          <div class="gap-sm">
            <button class="btn btn-primary btn-sm" (click)="confirmarTransicion()" [disabled]="transicionando()">
              {{ transicionando() ? 'Procesando…' : 'Confirmar' }}
            </button>
            <button class="btn btn-outline btn-sm" (click)="accionActiva.set('')">Cancelar</button>
          </div>
        </div>
      </div>

      <!-- Datos generales -->
      <div class="card" style="margin-bottom:1rem">
        <h3 style="margin:0 0 1rem">Datos generales</h3>
        <dl class="dl-grid">
          <dt>Carrera</dt><dd>{{ p.carrera || '—' }}</dd>
          <dt>Asignaturas</dt><dd style="white-space:pre-line">{{ p.asignaturas || '—' }}</dd>
          <dt>Descripción</dt><dd>{{ p.descripcion || '—' }}</dd>
          <dt>Problema</dt><dd>{{ p.problema || '—' }}</dd>
          <dt>Objetivo general</dt><dd>{{ p.objetivoGeneral || '—' }}</dd>
          <dt>Alcance</dt><dd>{{ p.alcance || '—' }}</dd>
          <dt *ngIf="p.autorizadoPor">Autorizado por</dt>
          <dd *ngIf="p.autorizadoPor">{{ p.autorizadoPor }}<span *ngIf="p.fechaAutorizacion"> · {{ p.fechaAutorizacion }}</span></dd>
          <dt>Fecha de envío</dt><dd>{{ (p.fechaEnvio | date:'dd/MM/yyyy') || '—' }}</dd>
          <dt>Última actualización</dt><dd>{{ p.fechaUltimaActualizacion | date:'dd/MM/yyyy HH:mm' }}</dd>
        </dl>
      </div>

      <!-- Estudiantes asignados -->
      <div class="card" style="margin-bottom:1rem" *ngIf="p.estudiantes.length > 0">
        <h3 style="margin:0 0 .75rem">Estudiantes asignados</h3>
        <table>
          <thead><tr><th>Nombre</th><th>Correo</th><th>Fecha asignación</th></tr></thead>
          <tbody>
            <tr *ngFor="let e of p.estudiantes">
              <td>{{ e.nombreCompleto }}</td>
              <td>{{ e.email }}</td>
              <td>{{ e.fechaAsignacion | date:'dd/MM/yyyy' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Observaciones -->
      <div class="card" *ngIf="p.observaciones.length > 0">
        <h3 style="margin:0 0 .75rem">Observaciones</h3>
        <div *ngFor="let o of p.observaciones" style="border-bottom:1px solid #eee;padding:.75rem 0;last-child:border:none">
          <p style="margin:0 0 .25rem">{{ o.texto }}</p>
          <small class="text-muted">{{ o.creadoEn | date:'dd/MM/yyyy HH:mm' }}</small>
        </div>
      </div>
    </ng-container>
  `,
  styles: [`
    .dl-grid { display: grid; grid-template-columns: 180px 1fr; gap: .4rem .75rem; margin: 0; }
    dt { font-weight: 600; font-size: .875rem; color: #424242; }
    dd { margin: 0; font-size: .9rem; }
  `],
})
export class PropuestaDetalleComponent implements OnInit {
  private svc = inject(PropuestaService);
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  loading = signal(true);
  error = signal('');
  propuesta = signal<PropuestaDetailDto | null>(null);
  accionActiva = signal('');
  transicionando = signal(false);
  estados = ESTADOS_PROPUESTA;

  comentarioForm = this.fb.nonNullable.group({ comentario: [''] });

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.svc.getById(id).subscribe({
      next: (data) => { this.propuesta.set(data); this.loading.set(false); },
      error: () => { this.error.set('No se pudo cargar la propuesta.'); this.loading.set(false); },
    });
  }

  transicion(accion: string) {
    this.accionActiva.set(accion);
    this.comentarioForm.reset();
  }

  confirmarTransicion() {
    const p = this.propuesta();
    if (!p) return;
    const comentario = this.comentarioForm.getRawValue().comentario || undefined;
    const body = { comentario };
    this.transicionando.set(true);

    const call$ = this.accionActiva() === 'enviar' ? this.svc.enviarRevision(p.id, body)
      : this.accionActiva() === 'aprobar' ? this.svc.aprobar(p.id, body)
      : this.accionActiva() === 'rechazar' ? this.svc.rechazar(p.id, body)
      : this.svc.pendiente(p.id, body);

    call$.subscribe({
      next: () => {
        this.accionActiva.set('');
        this.transicionando.set(false);
        this.svc.getById(p.id).subscribe((updated) => this.propuesta.set(updated));
      },
      error: () => this.transicionando.set(false),
    });
  }

  badgeClass(estado: string): string {
    const map: Record<string, string> = {
      Borrador: 'badge-borrador',
      EnRevision: 'badge-revision',
      Aprobada: 'badge-aprobada',
      Rechazada: 'badge-rechazada',
      Pendiente: 'badge-pendiente',
    };
    return map[estado] ?? 'badge-borrador';
  }

  puedoEditar(estado: string): boolean {
    return estado === ESTADOS_PROPUESTA.Borrador || estado === ESTADOS_PROPUESTA.Pendiente;
  }
}
