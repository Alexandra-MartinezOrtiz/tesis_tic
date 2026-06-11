import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgFor, NgIf, NgClass, DatePipe } from '@angular/common';
import { PropuestaService } from '../../../core/services/propuesta.service';
import { PropuestaListItemDto } from '../../../core/models/propuesta.models';

@Component({
  selector: 'app-propuestas-home',
  standalone: true,
  imports: [RouterLink, NgFor, NgIf, NgClass, DatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">Propuestas TIC</h2>
      <a routerLink="nueva" class="btn btn-primary">+ Nueva propuesta</a>
    </div>

    <div *ngIf="loading()" class="empty-state">Cargando…</div>
    <div *ngIf="error()" class="empty-state" style="color:var(--color-danger)">{{ error() }}</div>

    <div class="table-wrap" *ngIf="!loading() && !error()">
      <table>
        <thead>
          <tr>
            <th>Código</th>
            <th>Título</th>
            <th>Estado</th>
            <th>Última actualización</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let p of propuestas()">
            <td><code>{{ p.codigo }}</code></td>
            <td>{{ p.titulo }}</td>
            <td>
              <span class="badge" [ngClass]="badgeClass(p.estadoActual)">
                {{ p.estadoActual }}
              </span>
            </td>
            <td>{{ p.fechaUltimaActualizacion | date:'dd/MM/yyyy HH:mm' }}</td>
            <td>
              <a [routerLink]="p.id.toString()" class="btn btn-outline btn-sm">Ver</a>
            </td>
          </tr>
          <tr *ngIf="propuestas().length === 0">
            <td colspan="5" class="empty-state">No hay propuestas registradas.</td>
          </tr>
        </tbody>
      </table>
    </div>
  `,
})
export class PropuestasHomeComponent implements OnInit {
  private svc = inject(PropuestaService);

  loading = signal(true);
  error = signal('');
  propuestas = signal<PropuestaListItemDto[]>([]);

  ngOnInit() {
    this.svc.getAll().subscribe({
      next: (data) => { this.propuestas.set(data); this.loading.set(false); },
      error: () => { this.error.set('Error al cargar propuestas.'); this.loading.set(false); },
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
}
