import { Component, inject, signal, computed, OnInit, HostListener, ElementRef, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { NgFor, NgIf, NgClass, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { ReporteService } from '../../../core/services/reporte.service';
import { PropuestaReporteItemDto } from '../../../core/models/reporte.models';

type FilterEstado = '' | 'Borrador' | 'EnRevision' | 'Aprobada' | 'Rechazada' | 'Pendiente';

@Component({
  selector: 'app-reportes-home',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, NgClass, DatePipe, ReactiveFormsModule, RouterLink],
  styles: [`
    @keyframes spin { to { transform: rotate(360deg); } }
    .toast-print {
      position:fixed;bottom:1.5rem;right:1.5rem;
      background:#0E2240;color:#fff;
      padding:.6rem 1.2rem;border-radius:8px;
      font-size:.85rem;font-weight:600;
      box-shadow:0 4px 12px rgba(0,0,0,.25);
      z-index:9999;animation:fadeIn .3s ease;
    }
    @keyframes fadeIn { from { opacity:0; transform:translateY(8px); } to { opacity:1; transform:translateY(0); } }
  `],
  template: `
    <!-- Toast impresión -->
    <div *ngIf="toastVisible" class="toast-print">✅ Preparando documento para impresión…</div>

    <!-- Page header -->
    <div class="page-header">
      <div style="display:flex;align-items:center;gap:.6rem">
        <div>
          <h2 class="page-title">Consultas y Reportes</h2>
          <p class="page-subtitle">Módulo B — Trabajo de Integración Curricular · EPN / FIS</p>
        </div>
        <!-- Botón de ayuda -->
        <div style="position:relative;display:inline-block" (mouseenter)="mostrarAyuda=true" (mouseleave)="mostrarAyuda=false">
          <button style="width:22px;height:22px;border-radius:50%;border:1.5px solid #9e9e9e;
                         background:#f5f5f5;color:#616161;font-size:.75rem;font-weight:700;
                         cursor:help;line-height:1;padding:0">?</button>
          <div *ngIf="mostrarAyuda"
               style="position:absolute;left:28px;top:-8px;width:260px;
                      background:#0E2240;color:#fff;border-radius:8px;
                      padding:10px 14px;font-size:.78rem;line-height:1.6;
                      box-shadow:0 4px 12px rgba(0,0,0,.25);z-index:100">
            <strong style="display:block;margin-bottom:4px">Guía rápida</strong>
            • Usa los botones de estado para filtrar propuestas<br>
            • Escribe en el buscador para filtrar por código, título o docente<br>
            • Presiona <kbd style="background:#fff;color:#0E2240;padding:1px 4px;border-radius:3px;font-size:.72rem">/</kbd> para enfocar el buscador<br>
            • Presiona <kbd style="background:#fff;color:#0E2240;padding:1px 4px;border-radius:3px;font-size:.72rem">Esc</kbd> para limpiar los filtros<br>
            • Haz clic en <strong>Ver →</strong> para ver el formulario F_AA_233A
          </div>
        </div>
      </div>
      <button (click)="imprimir()" title="Imprimir listado de propuestas"
        style="display:flex;align-items:center;gap:.5rem;padding:.45rem 1rem;
               background:#0E2240;color:#fff;border:none;border-radius:6px;
               font-size:.85rem;font-weight:600;cursor:pointer;
               box-shadow:0 2px 6px rgba(14,34,64,.25);transition:background .2s"
        onmouseover="this.style.background='#1a3a6b'"
        onmouseout="this.style.background='#0E2240'">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
          <path d="M2.5 8a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1z"/>
          <path d="M5 1a2 2 0 0 0-2 2v2H2a2 2 0 0 0-2 2v3a2 2 0 0 0 2 2h1v1a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2v-1h1a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-1V3a2 2 0 0 0-2-2H5zM4 3a1 1 0 0 1 1-1h6a1 1 0 0 1 1 1v2H4V3zm1 5a2 2 0 0 0-2 2v1H2a1 1 0 0 1-1-1V7a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-1v-1a2 2 0 0 0-2-2H5zm7 2v3a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1v-3a1 1 0 0 1 1-1h6a1 1 0 0 1 1 1z"/>
        </svg>
        Imprimir listado
      </button>
    </div>

    <!-- Alertas -->
    <div *ngIf="error()" class="alert alert-danger"
         style="display:flex;align-items:center;justify-content:space-between;gap:1rem">
      <span>⚠️ {{ error() }}</span>
      <button class="btn btn-sm btn-outline" (click)="cargar()"
              style="white-space:nowrap;flex-shrink:0">
        🔄 Reintentar
      </button>
    </div>

    <!-- Counter cards -->
    <div class="counter-row">
      <div class="counter-card cc-total" style="cursor:pointer" (click)="setEstado('')">
        <span class="cc-value">{{ propuestas().length }}</span>
        <span class="cc-label">Total</span>
      </div>
      <div class="counter-card cc-aprobada" style="cursor:pointer" (click)="setEstado('Aprobada')">
        <span class="cc-value">{{ contar('Aprobada') }}</span>
        <span class="cc-label">Aprobadas</span>
      </div>
      <div class="counter-card cc-revision" style="cursor:pointer" (click)="setEstado('EnRevision')">
        <span class="cc-value">{{ contar('EnRevision') }}</span>
        <span class="cc-label">En revisión</span>
      </div>
      <div class="counter-card cc-pendiente" style="cursor:pointer" (click)="setEstado('Pendiente')">
        <span class="cc-value">{{ contar('Pendiente') }}</span>
        <span class="cc-label">Pendientes</span>
      </div>
      <div class="counter-card cc-rechazada" style="cursor:pointer" (click)="setEstado('Rechazada')">
        <span class="cc-value">{{ contar('Rechazada') }}</span>
        <span class="cc-label">Rechazadas</span>
      </div>
      <div class="counter-card cc-borrador" style="cursor:pointer" (click)="setEstado('Borrador')">
        <span class="cc-value">{{ contar('Borrador') }}</span>
        <span class="cc-label">Borradores</span>
      </div>
    </div>

    <!-- Filtros -->
    <div class="card" style="margin-bottom:1rem">
      <div class="sl">Filtrar propuestas</div>

      <div class="stat-btn-row">
        <button class="stat-btn sb-todos"    [class.active]="filtroEstado() === ''"          (click)="setEstado('')">Todas</button>
        <button class="stat-btn sb-aprobada" [class.active]="filtroEstado() === 'Aprobada'"  (click)="setEstado('Aprobada')">
          <span class="dot"></span>Aprobadas
        </button>
        <button class="stat-btn sb-revision" [class.active]="filtroEstado() === 'EnRevision'" (click)="setEstado('EnRevision')">
          <span class="dot"></span>En revisión
        </button>
        <button class="stat-btn sb-pendiente" [class.active]="filtroEstado() === 'Pendiente'" (click)="setEstado('Pendiente')">
          <span class="dot"></span>Pendientes
        </button>
        <button class="stat-btn sb-rechazada" [class.active]="filtroEstado() === 'Rechazada'" (click)="setEstado('Rechazada')">
          <span class="dot"></span>Rechazadas
        </button>
        <button class="stat-btn sb-borrador"  [class.active]="filtroEstado() === 'Borrador'"  (click)="setEstado('Borrador')">
          <span class="dot"></span>Borradores
        </button>
      </div>

      <div style="display:flex;gap:.75rem;align-items:center;flex-wrap:wrap;margin-top:.5rem" [formGroup]="filtrosForm">
        <div class="search-bar" style="flex:1;min-width:200px">
          <span class="search-icon">&#128269;</span>
          <input #busquedaInput formControlName="busqueda" placeholder="Buscar por código, título o proponente… (presiona / para enfocar)" />
        </div>
        <button class="btn btn-outline btn-sm" (click)="limpiar()">Limpiar</button>
      </div>
    </div>

    <!-- Table -->
    <div class="table-wrap">
      <table class="dtable">
        <thead>
          <tr>
            <th>#</th>
            <th>Código</th>
            <th>Título</th>
            <th>Proponente</th>
            <th>Estado</th>
            <th>Últ. actualización</th>
            <th>Disponible</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr *ngIf="loading()">
            <td colspan="8" style="text-align:center;padding:2.5rem">
              <div style="display:flex;flex-direction:column;align-items:center;gap:.75rem">
                <div style="width:36px;height:36px;border:3px solid #E8EDF4;
                            border-top-color:#0E2240;border-radius:50%;
                            animation:spin 0.8s linear infinite"></div>
                <span style="color:#9e9e9e;font-size:.85rem">Cargando propuestas…</span>
              </div>
            </td>
          </tr>
          <ng-container *ngIf="!loading()">
            <tr *ngFor="let p of propuestasFiltradas(); let i = index; trackBy: trackById">
              <td style="color:#9e9e9e;font-size:.78rem">{{ i + 1 }}</td>
              <td><code>{{ p.codigo }}</code></td>
              <td style="max-width:240px;font-weight:500;line-height:1.4">{{ p.titulo }}</td>
              <td style="font-size:.82rem;color:#616161">{{ p.docenteEmail || '—' }}</td>
              <td>
                <span class="badge" [ngClass]="badgeClass(p.estadoActual)">{{ estadoLabel(p.estadoActual) }}</span>
              </td>
              <td style="font-size:.82rem;color:#757575;white-space:nowrap">{{ p.fechaUltimaActualizacion | date:'dd/MM/yyyy' }}</td>
              <td>
                <span style="font-size:.78rem;font-weight:700"
                  [style.color]="p.activa ? 'var(--green)' : 'var(--gray)'">
                  {{ p.activa ? 'Sí' : 'No' }}
                </span>
              </td>
              <td>
                <a [routerLink]="['/reportes', p.id]" class="btn btn-ghost btn-sm">
                  Ver &rarr;
                </a>
              </td>
            </tr>
            <tr *ngIf="propuestasFiltradas().length === 0 && !loading()">
              <td colspan="8" class="empty-state">
                No hay propuestas que coincidan con los filtros aplicados.
              </td>
            </tr>
          </ng-container>
        </tbody>
      </table>
    </div>

    <!-- Footer -->
    <div *ngIf="!loading()" style="margin-top:.75rem;display:flex;justify-content:space-between;align-items:center">
      <span class="text-muted">
        Mostrando <strong>{{ propuestasFiltradas().length }}</strong> de
        <strong>{{ propuestas().length }}</strong> propuestas
      </span>
      <span *ngIf="filtroEstado()" class="badge badge-revision" style="font-size:.78rem">
        Filtro: {{ estadoLabel(filtroEstado()) }}
      </span>
    </div>
  `,
})
export class ReportesHomeComponent implements OnInit {
  private svc = inject(ReporteService);
  private fb  = inject(FormBuilder);

  @ViewChild('busquedaInput') busquedaInputRef!: ElementRef<HTMLInputElement>;

  @HostListener('document:keydown', ['$event'])
  onKeydown(e: KeyboardEvent) {
    const tag = (e.target as HTMLElement).tagName;
    if (tag === 'INPUT' || tag === 'TEXTAREA') return;
    if (e.key === '/') { e.preventDefault(); this.busquedaInputRef?.nativeElement.focus(); }
    if (e.key === 'Escape') { this.limpiar(); }
  }

  mostrarAyuda = false;
  toastVisible  = false;

  loading = signal(true);
  error = signal('');
  propuestas = signal<PropuestaReporteItemDto[]>([]);
  filtroEstado = signal<FilterEstado>('');

  filtrosForm = this.fb.nonNullable.group({ busqueda: [''] });

  private busquedaValue = toSignal(
    this.filtrosForm.controls.busqueda.valueChanges,
    { initialValue: '' }
  );

  propuestasFiltradas = computed(() => {
    const estado = this.filtroEstado();
    const busq = (this.busquedaValue() ?? '').toLowerCase().trim();
    return this.propuestas().filter(p => {
      const matchEstado = !estado || p.estadoActual.toLowerCase() === estado.toLowerCase();
      const matchBusq = !busq ||
        p.codigo.toLowerCase().includes(busq) ||
        p.titulo.toLowerCase().includes(busq) ||
        (p.docenteEmail ?? '').toLowerCase().includes(busq);
      return matchEstado && matchBusq;
    });
  });

  ngOnInit() { this.cargar(); }

  cargar() {
    this.loading.set(true);
    this.error.set('');
    this.svc.getPropuestas().subscribe({
      next: (data) => { this.propuestas.set(data); this.loading.set(false); },
      error: () => {
        this.error.set('Error al cargar los reportes. Verifique la conexión al servidor.');
        this.loading.set(false);
      },
    });
  }

  limpiar() {
    this.filtrosForm.reset();
    this.filtroEstado.set('');
  }

  imprimir() {
    if (confirm('¿Desea imprimir el listado de propuestas?')) {
      this.toastVisible = true;
      setTimeout(() => { this.toastVisible = false; window.print(); }, 1200);
    }
  }

  trackById(_: number, p: PropuestaReporteItemDto) { return p.id; }

  setEstado(e: FilterEstado) { this.filtroEstado.set(e); }

  contar(estado: string) { return this.propuestas().filter(p => p.estadoActual === estado).length; }

  badgeClass(estado: string): string {
    const map: Record<string, string> = {
      Borrador: 'badge-borrador', EnRevision: 'badge-revision',
      Aprobada: 'badge-aprobada', Rechazada: 'badge-rechazada', Pendiente: 'badge-pendiente',
    };
    return map[estado] ?? 'badge-borrador';
  }

  estadoLabel(estado: string): string {
    const map: Record<string, string> = {
      Borrador: 'Borrador', EnRevision: 'En revisión',
      Aprobada: 'Aprobada', Rechazada: 'Rechazada', Pendiente: 'Pendiente',
    };
    return map[estado] ?? estado;
  }
}
