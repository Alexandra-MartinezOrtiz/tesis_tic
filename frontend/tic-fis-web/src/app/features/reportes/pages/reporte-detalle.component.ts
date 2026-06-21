import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NgFor, NgIf, DatePipe } from '@angular/common';
import { ReporteService } from '../../../core/services/reporte.service';
import { PropuestaReporteDetalleDto } from '../../../core/models/reporte.models';

@Component({
  selector: 'app-reporte-detalle',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, DatePipe, RouterLink],
  styles: [`
    .formulario {
      max-width: 900px;
      margin: 0 auto;
      font-family: Arial, sans-serif;
      font-size: 0.85rem;
      color: #212121;
    }
    .f-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 4px;
    }
    .f-header-left strong { display: block; font-size: 1rem; color: #000; }
    .f-header-left span  { font-size: 0.78rem; color: #333; }
    .f-code { font-weight: 700; font-size: 1rem; color: #000; }
    .f-title  { text-align: center; font-weight: 700; font-size: 0.95rem; color: #000; margin: 4px 0 1px; }
    .f-subtitle { text-align: center; font-weight: 700; font-size: 0.82rem; color: #000; margin-bottom: 8px; }
    .f-divider { border: none; border-top: 1.5px solid #000; margin-bottom: 10px; }

    .f-table {
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 8px;
    }
    .f-table th.seccion {
      background: #D9D9D9;
      color: #000;
      font-weight: 700;
      font-size: 0.82rem;
      padding: 6px 8px;
      text-align: center;
      border: 0.5px solid #000;
    }
    .f-table td.lbl {
      background: #FFFFFF;
      font-weight: 700;
      font-size: 0.8rem;
      padding: 5px 8px;
      border: 0.5px solid #000;
      width: 32%;
      vertical-align: top;
      color: #000;
    }
    .f-table td.val {
      background: #FFFFFF;
      font-size: 0.82rem;
      padding: 5px 8px;
      border: 0.5px solid #000;
      vertical-align: top;
      color: #000;
    }
    .f-table td.contenido {
      background: #FFFFFF;
      font-size: 0.82rem;
      padding: 8px 10px;
      border: 0.5px solid #000;
      min-height: 50px;
      line-height: 1.6;
      color: #000;
    }
    .f-table td.contenido p { margin: 0 0 8px; }
    .f-table td.contenido p:last-child { margin: 0; }

    .est-header {
      background: #FFFFFF;
      color: #000;
      font-weight: 700;
      font-size: 0.83rem;
      padding: 5px 8px;
      border: 0.5px solid #000;
    }
    .pendiente { color: #9E9E9E; font-style: italic; }

    .act-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.78rem;
    }
    .act-table th {
      background: #D9D9D9;
      color: #000;
      font-weight: 700;
      padding: 3px 6px;
      border: 0.5px solid #000;
      text-align: left;
    }
    .act-table td {
      padding: 4px 6px;
      border: 0.5px solid #000;
      height: 18px;
      color: #000;
    }
    .act-table tr.total td { background: #FFFFFF; font-weight: 700; }

    .f-footer {
      display: flex;
      justify-content: space-between;
      font-size: 0.72rem;
      color: #9E9E9E;
      margin-top: 12px;
      padding-top: 6px;
      border-top: 1px solid #E0E0E0;
    }
  `],
  template: `
    <!-- Barra superior -->
    <div class="barra-acciones" style="margin-bottom:1rem;display:flex;align-items:center;justify-content:space-between;flex-wrap:wrap;gap:.5rem">
      <a routerLink="/reportes"
        style="display:flex;align-items:center;gap:.4rem;padding:.45rem 1rem;
               background:transparent;color:#0E2240;border:1.5px solid #0E2240;
               border-radius:6px;font-size:.85rem;font-weight:600;cursor:pointer;
               text-decoration:none;transition:background .2s"
        onmouseover="this.style.background='#f0f3f8'"
        onmouseout="this.style.background='transparent'">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
          <path fill-rule="evenodd" d="M15 8a.5.5 0 0 0-.5-.5H2.707l3.147-3.146a.5.5 0 1 0-.708-.708l-4 4a.5.5 0 0 0 0 .708l4 4a.5.5 0 0 0 .708-.708L2.707 8.5H14.5A.5.5 0 0 0 15 8z"/>
        </svg>
        Volver al listado
      </a>
      <button (click)="imprimir()" title="Imprimir formulario F_AA_233A"
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
        Imprimir formulario
      </button>
    </div>

    <div *ngIf="loading()" class="card empty-state">Cargando propuesta…</div>
    <div *ngIf="error()" class="alert alert-danger"
         style="display:flex;align-items:center;justify-content:space-between;gap:1rem">
      <span>⚠️ {{ error() }}</span>
      <a routerLink="/reportes" class="btn btn-sm btn-outline"
         style="white-space:nowrap;flex-shrink:0">
        ← Volver al listado
      </a>
    </div>
    <ng-container *ngIf="!loading() && !error() && propuesta() as p">
      <div class="formulario print-area">

        <!-- ── ENCABEZADO OFICIAL ── -->
        <div class="f-header">
          <div class="f-header-left">
            <strong>ESCUELA POLITÉCNICA NACIONAL</strong>
            <span>Facultad de Ingeniería de Sistemas</span>
          </div>
          <span class="f-code">F_AA_233A</span>
        </div>
        <div class="f-title">CONSEJO DE DOCENCIA</div>
        <div class="f-subtitle">FORMULARIO DEL PROYECTO DE TRABAJO DE INTEGRACIÓN CURRICULAR</div>
        <hr class="f-divider">

        <!-- ── DATOS GENERALES ── -->
        <table class="f-table">
          <thead>
            <tr><th colspan="2" class="seccion">DATOS GENERALES</th></tr>
          </thead>
          <tbody>
            <tr>
              <td class="lbl">Unidad Académica:</td>
              <td class="val">Facultad de Ingeniería de Sistemas</td>
            </tr>
            <tr>
              <td class="lbl">Carrera:</td>
              <td class="val">{{ p.carrera || '(No especificada)' }}</td>
            </tr>
            <tr>
              <td class="lbl">Proyecto:</td>
              <td class="val"><strong>{{ p.titulo }}</strong></td>
            </tr>
            <tr>
              <td class="lbl">Número de participantes:</td>
              <td class="val">{{ estudiantes(p) }}</td>
            </tr>
            <tr>
              <td class="lbl">Cupos / Disponible:</td>
              <td class="val">
                <strong>{{ cupos(p) }}</strong>&nbsp;—&nbsp;
                <span style="font-weight:700" [style.color]="disponible(p) ? 'var(--green)' : 'var(--gray)'">
                  {{ disponible(p) ? 'Sí' : 'No' }}
                </span>
              </td>
            </tr>
            <tr>
              <td class="lbl">Departamento:</td>
              <td class="val">Departamento de Informática y Ciencias de la Computación</td>
            </tr>
            <tr>
              <td class="lbl">Asignaturas:</td>
              <td class="val" style="white-space:pre-line">{{ p.asignaturas || '(No especificadas)' }}</td>
            </tr>
            <tr>
              <td class="lbl">Profesor:</td>
              <td class="val">Docente (ref. usuario #{{ p.docenteUsuarioIdReferencia }})</td>
            </tr>
          </tbody>
        </table>

        <!-- ── DESCRIPCIÓN DEL PROYECTO ── -->
        <table class="f-table">
          <thead>
            <tr><th class="seccion">DESCRIPCIÓN DEL PROYECTO</th></tr>
          </thead>
          <tbody>
            <tr>
              <td class="contenido">
                <p *ngIf="p.descripcion">{{ p.descripcion }}</p>
                <p *ngIf="p.problema">
                  <strong>Problema identificado:</strong><br>{{ p.problema }}
                </p>
                <span *ngIf="!p.descripcion && !p.problema" class="text-muted">—</span>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- ── OBJETIVO DEL PROYECTO ── -->
        <table class="f-table">
          <thead>
            <tr><th class="seccion">OBJETIVO DEL PROYECTO</th></tr>
          </thead>
          <tbody>
            <tr>
              <td class="contenido" style="min-height:40px">
                {{ p.objetivoGeneral || '—' }}
              </td>
            </tr>
          </tbody>
        </table>

        <!-- ── ALCANCE DEL PROYECTO ── -->
        <table class="f-table">
          <thead>
            <tr><th class="seccion">ALCANCE DEL PROYECTO</th></tr>
          </thead>
          <tbody>
            <tr>
              <td class="contenido" style="min-height:40px">
                {{ p.alcance || '—' }}
              </td>
            </tr>
          </tbody>
        </table>

        <!-- ── COMPONENTES, ACTIVIDADES Y PRODUCTOS ── -->
        <table class="f-table">
          <thead>
            <tr><th class="seccion">COMPONENTES, ACTIVIDADES Y PRODUCTOS</th></tr>
          </thead>
        </table>

        <div *ngIf="estudiantes(p) === 0"
             style="background:#FFF8E1;padding:10px 12px;border:0.5px solid #CCCCCC;color:#F57F17;font-style:italic;font-size:.82rem;margin-bottom:8px">
          Sin estudiantes propuestos. La propuesta está disponible para asignación (0/{{ cupoMaximo(p) }}).
        </div>

        <ng-container *ngFor="let letra of slots(p); let i = index">
          <table class="f-table" style="margin-bottom:2px">
            <tbody>
              <!-- Cabecera del estudiante -->
              <tr>
                <td colspan="2" class="est-header">Estudiante {{ letra }}:</td>
              </tr>
              <!-- Módulo / Componente -->
              <tr>
                <td class="lbl" style="width:32%">Módulo / Componente</td>
                <td class="val" style="height:26px"></td>
              </tr>
              <!-- Actividades -->
              <tr>
                <td class="lbl" style="vertical-align:top;padding-top:6px">
                  Actividades específicas<br>y horas asignadas
                </td>
                <td class="val" style="padding:4px">
                  <table class="act-table">
                    <thead>
                      <tr>
                        <th style="width:40px;text-align:center">No.</th>
                        <th>Actividades específicas</th>
                        <th style="width:60px;text-align:center">Horas</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr *ngFor="let r of actividades">
                        <td style="text-align:center">{{ r }}</td><td></td><td></td>
                      </tr>
                      <tr class="total">
                        <td colspan="2" style="text-align:right">Total</td><td></td>
                      </tr>
                    </tbody>
                  </table>
                </td>
              </tr>
              <!-- Productos -->
              <tr>
                <td class="lbl">Producto(s) esperado(s)</td>
                <td class="val" style="height:40px"></td>
              </tr>
              <!-- Nombre del estudiante -->
              <tr>
                <td class="lbl">Nombre del estudiante propuesto</td>
                <td class="val">
                  <strong>{{ nombreSlot(p, i) }}</strong>
                  <span *ngIf="emailSlot(p, i)" style="margin-left:1rem;font-size:.78rem;color:#616161">{{ emailSlot(p, i) }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </ng-container>

        <!-- ── SOLICITUD DE PARTICIPACIÓN < 2 o > 5 ESTUDIANTES (Opcional) ── -->
        <table class="f-table" style="margin-top:8px">
          <thead>
            <tr><th colspan="2" class="seccion">SOLICITUD DE PARTICIPACIÓN DE MENOS DE 2 O MÁS DE 5 ESTUDIANTES (Opcional)</th></tr>
          </thead>
          <tbody>
            <tr>
              <td class="lbl" style="width:32%">Autorizado por:</td>
              <td class="val" style="height:30px"></td>
            </tr>
            <tr>
              <td class="lbl">Fecha:</td>
              <td class="val" style="height:24px"></td>
            </tr>
          </tbody>
        </table>

        <!-- ── RECOMENDACIONES Y APROBACIONES ── -->
        <table class="f-table">
          <thead>
            <tr><th colspan="2" class="seccion">RECOMENDACIONES Y APROBACIONES</th></tr>
          </thead>
          <tbody>
            <tr>
              <td class="lbl" style="width:32%">Presentado por:</td>
              <td class="val">{{ p.presentadoPor || ('Docente (ref. usuario #' + p.docenteUsuarioIdReferencia + ')') }}</td>
            </tr>
            <tr>
              <td class="lbl">Estudiantes propuestos:</td>
              <td class="val" style="white-space:pre-line">{{ p.estudiantesNombres || nombresEstudiantes(p) }}</td>
            </tr>
            <tr>
              <td class="lbl">Resolución de la CPGIC:</td>
              <td class="val" style="white-space:pre-line;min-height:45px">{{ p.resolucionCpgic || '—' }}</td>
            </tr>
            <tr>
              <td class="lbl">Presidente de la CPGIC:</td>
              <td class="val" style="min-height:35px">{{ p.presidenteCpgic || '—' }}</td>
            </tr>
            <tr>
              <td class="lbl">Fecha de aprobación:</td>
              <td class="val">{{ p.fechaAprobacion || (p.fechaEnvio ? (p.fechaEnvio | date:'dd/MM/yyyy') : '—') }}</td>
            </tr>
          </tbody>
        </table>

        <!-- ── PIE ── -->
        <div class="f-footer">
          <span>Sistema TIC-FIS — Escuela Politécnica Nacional</span>
          <span>Código: {{ p.codigo }}</span>
          <span>Actualizado: {{ p.fechaUltimaActualizacion | date:'dd/MM/yyyy HH:mm' }}</span>
        </div>

      </div>
    </ng-container>
  `,
})
export class ReporteDetalleComponent implements OnInit {
  private svc = inject(ReporteService);
  private route = inject(ActivatedRoute);

  loading   = signal(true);
  error     = signal('');
  propuesta = signal<PropuestaReporteDetalleDto | null>(null);

  readonly letras = ['A', 'B', 'C', 'D', 'E'];
  readonly actividades = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
  readonly maxCupos = 5;

  ngOnInit() {
    const raw = this.route.snapshot.paramMap.get('id');
    const id  = Number(raw);

    if (!raw || isNaN(id) || id <= 0) {
      this.error.set('El identificador de la propuesta no es válido. Vuelva al listado.');
      this.loading.set(false);
      return;
    }

    this.svc.getPropuestaById(id).subscribe({
      next:  (data) => { this.propuesta.set(data); this.loading.set(false); },
      error: ()     => { this.error.set('No se pudo cargar el detalle de la propuesta. Verifique su conexión o vuelva al listado.'); this.loading.set(false); },
    });
  }

  imprimir() {
    if (confirm('¿Desea imprimir el formulario F_AA_233A de esta propuesta?')) {
      window.print();
    }
  }

  cupoMaximo(p: PropuestaReporteDetalleDto): number {
    return p.cupoMaximo ?? this.maxCupos;
  }

  estudiantes(p: PropuestaReporteDetalleDto): number {
    const n = p.estudiantesPropuestos ?? p.estudiantes?.length ?? 0;
    return Math.min(Math.max(n, 0), this.cupoMaximo(p));
  }

  cupos(p: PropuestaReporteDetalleDto): string {
    return `${this.estudiantes(p)}/${this.cupoMaximo(p)}`;
  }

  disponible(p: PropuestaReporteDetalleDto): boolean {
    return this.estudiantes(p) < this.cupoMaximo(p);
  }

  // Devuelve un array de letras (A, B, C…) según el número de estudiantes propuestos.
  slots(p: PropuestaReporteDetalleDto): string[] {
    const n = this.estudiantes(p);
    return this.letras.slice(0, n);
  }

  // Nombre real del estudiante asignado en la posición i, si existe; si no, vacío (plantilla).
  nombreSlot(p: PropuestaReporteDetalleDto, i: number): string {
    return p.estudiantes?.[i]?.nombreCompleto ?? '';
  }

  emailSlot(p: PropuestaReporteDetalleDto, i: number): string {
    return p.estudiantes?.[i]?.email ?? '';
  }

  nombresEstudiantes(p: PropuestaReporteDetalleDto): string {
    return p.estudiantes && p.estudiantes.length > 0
      ? p.estudiantes.map(e => e.nombreCompleto).join(';  ')
      : '—';
  }
}
