import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { NgFor, NgIf } from '@angular/common';
import { PropuestaService } from '../../../core/services/propuesta.service';

@Component({
  selector: 'app-propuesta-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, NgFor, NgIf],
  styles: [`
    .formulario {
      max-width: 900px;
      margin: 0 auto;
      font-family: Arial, sans-serif;
      font-size: 0.85rem;
      color: #212121;
    }
    .f-header { display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:4px; }
    .f-header-left strong { display:block; font-size:1rem; color:#000; }
    .f-header-left span   { font-size:0.78rem; color:#333; }
    .f-code   { font-weight:700; font-size:1rem; color:#000; }
    .f-title  { text-align:center; font-weight:700; font-size:0.95rem; color:#000; margin:4px 0 1px; }
    .f-subtitle { text-align:center; font-weight:700; font-size:0.82rem; color:#000; margin-bottom:8px; }
    .f-divider  { border:none; border-top:1.5px solid #000; margin-bottom:10px; }

    .f-table { width:100%; border-collapse:collapse; margin-bottom:8px; }
    .f-table th.seccion {
      background:#D9D9D9; color:#000; font-weight:700;
      font-size:0.82rem; padding:6px 8px; text-align:center;
      border:0.5px solid #000;
    }
    .f-table td.lbl {
      background:#FFFFFF; font-weight:700; font-size:0.8rem;
      padding:5px 8px; border:0.5px solid #000;
      width:32%; vertical-align:middle; color:#000;
    }
    .f-table td.val {
      background:#fff; font-size:0.82rem;
      padding:4px 6px; border:0.5px solid #000;
      vertical-align:middle; color:#000;
    }
    .f-table td.val input,
    .f-table td.val textarea {
      width:100%; border:none; outline:none; background:transparent;
      font-size:0.82rem; font-family:Arial,sans-serif;
      color:#212121; resize:vertical; padding:1px 2px;
    }
    .f-table td.val textarea { min-height:70px; line-height:1.5; }
    .f-table td.val input::placeholder,
    .f-table td.val textarea::placeholder { color:#BDBDBD; }
    .f-table td.val.fijo { color:#000; }
    .f-table td.contenido {
      background:#fff; padding:6px 8px;
      border:0.5px solid #000; vertical-align:top;
    }
    .f-table td.contenido textarea {
      width:100%; border:none; outline:none; background:transparent;
      font-size:0.82rem; font-family:Arial,sans-serif;
      color:#212121; resize:vertical; line-height:1.6;
    }
    .sub-lbl { font-weight:700; font-size:0.78rem; color:#000; display:block; margin:6px 0 2px; }

    .est-header {
      background:#FFFFFF; color:#000; font-weight:700;
      font-size:0.83rem; padding:5px 8px; border:0.5px solid #000;
    }
    .act-table { width:100%; border-collapse:collapse; font-size:0.78rem; }
    .act-table th {
      background:#D9D9D9; color:#000; font-weight:700;
      padding:3px 6px; border:0.5px solid #000; text-align:left;
    }
    .act-table td { padding:4px 6px; border:0.5px solid #000; height:18px; color:#000; }
    .act-table tr.total td { background:#FFFFFF; font-weight:700; }

    .nota-comp {
      background:#F7F9FC; color:#616161; font-style:italic;
      font-size:0.8rem; padding:8px; border:0.5px solid #000; margin-bottom:8px;
    }

    .error-msg { font-size:0.75rem; color:#E31D1A; margin-top:2px; display:block; }
    .campo-requerido::after { content:' *'; color:#E31D1A; }
    .input-invalido input, .input-invalido textarea { border-bottom:1px solid #E31D1A !important; }

    .f-footer {
      display:flex; justify-content:space-between;
      font-size:0.72rem; color:#9E9E9E;
      margin-top:12px; padding-top:6px;
      border-top:1px solid #E0E0E0;
    }
    .acciones {
      display:flex; gap:.75rem; align-items:center;
      margin-top:1rem; justify-content:flex-end;
    }
    .codigo-sistema {
      max-width:900px; margin:0 auto .75rem;
      display:flex; flex-wrap:wrap; align-items:center; gap:.5rem;
      background:#F7F9FC; border:1px dashed #B0BEC5; border-radius:6px;
      padding:.5rem .75rem;
    }
    .codigo-sistema label { font-size:.8rem; font-weight:700; color:#37474F; }
    .codigo-sistema .cs-req { color:#E31D1A; }
    .codigo-sistema input {
      border:1px solid #CCC; border-radius:4px; padding:.3rem .55rem;
      font-size:.82rem; width:160px; outline:none;
    }
    .codigo-sistema input:focus { border-color:#0E2240; }
    .codigo-sistema .cs-nota { font-size:.72rem; color:#90A4AE; }
    .codigo-sistema .cs-error { font-size:.75rem; color:#E31D1A; font-weight:600; }
    .codigo-sistema.cs-invalido { border-color:#E31D1A; background:#FFF5F5; }
    .codigo-sistema.cs-invalido input { border-color:#E31D1A; }
  `],
  template: `
    <!-- Barra superior -->
    <div style="margin-bottom:1rem;display:flex;align-items:center;justify-content:space-between;flex-wrap:wrap;gap:.5rem">
      <a routerLink="/propuestas" class="btn btn-ghost btn-sm">&larr; Volver a propuestas</a>
      <span style="font-size:.8rem;color:#9E9E9E">
        {{ esEdicion ? 'Editando propuesta existente' : 'Registrando nueva propuesta' }}
      </span>
    </div>

    <div *ngIf="error()" class="alert alert-danger" style="max-width:900px;margin:0 auto .75rem">
      {{ error() }}
    </div>

    <form [formGroup]="form" (ngSubmit)="submit()">

      <!-- Código de la propuesta — uso interno del sistema, NO es parte del formulario oficial F_AA_233A -->
      <div *ngIf="!esEdicion" class="codigo-sistema" [class.cs-invalido]="f['codigo'].touched && f['codigo'].invalid">
        <label for="codigoInput">Código de la propuesta <span class="cs-req">*</span></label>
        <input id="codigoInput" formControlName="codigo" placeholder="TIC-2024-001" autocomplete="off"/>
        <span class="cs-nota">Identificador único de uso interno (no aparece en el formulario oficial).</span>
        <span class="cs-error" *ngIf="f['codigo'].touched && f['codigo'].invalid">El código es obligatorio.</span>
      </div>

      <div class="formulario">

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
              <td class="val fijo">Facultad de Ingeniería de Sistemas</td>
            </tr>
            <tr>
              <td class="lbl">Carrera:</td>
              <td class="val fijo">Ingeniería de Software</td>
            </tr>

            <!-- Título del proyecto (editable) -->
            <tr [class.input-invalido]="f['titulo'].touched && f['titulo'].invalid">
              <td class="lbl campo-requerido">Proyecto:</td>
              <td class="val">
                <input formControlName="titulo"
                       placeholder="Ingrese el nombre del proyecto de trabajo de integración curricular…"/>
                <span class="error-msg" *ngIf="f['titulo'].touched && f['titulo'].invalid">
                  El título del proyecto es obligatorio.
                </span>
              </td>
            </tr>

            <!-- Número de estudiantes propuestos (editable, 0 a 5) -->
            <tr [class.input-invalido]="f['estudiantesPropuestos'].touched && f['estudiantesPropuestos'].invalid">
              <td class="lbl">Número de participantes:</td>
              <td class="val">
                <input type="number" formControlName="estudiantesPropuestos"
                       min="0" max="5" step="1" style="max-width:90px"
                       placeholder="0"/>
                <span style="color:#616161;font-size:.75rem;margin-left:.5rem">de 5 cupos (0 a 5)</span>
                <span class="error-msg" *ngIf="f['estudiantesPropuestos'].touched && f['estudiantesPropuestos'].invalid">
                  Ingrese un número entre 0 y 5. No se permiten más de 5 estudiantes.
                </span>
              </td>
            </tr>
            <tr>
              <td class="lbl">Departamento:</td>
              <td class="val fijo">Departamento de Informática y Ciencias de la Computación</td>
            </tr>
            <tr>
              <td class="lbl">Línea de investigación:</td>
              <td class="val fijo">Ingeniería de Software</td>
            </tr>
            <tr>
              <td class="lbl">Asignaturas:</td>
              <td class="val fijo" style="white-space:pre-line">{{ asignaturas }}</td>
            </tr>
            <tr>
              <td class="lbl">Profesor:</td>
              <td class="val fijo">Asignado automáticamente al guardar</td>
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
                <textarea formControlName="descripcion" rows="4"
                  placeholder="Realizar una descripción clara y concisa del proyecto…">
                </textarea>
                <span class="sub-lbl">Problema identificado:</span>
                <textarea formControlName="problema" rows="3"
                  placeholder="Planteamiento del problema que motiva el proyecto…">
                </textarea>
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
              <td class="contenido">
                <textarea formControlName="objetivoGeneral" rows="3"
                  placeholder="Objetivo general que se busca alcanzar con el proyecto…">
                </textarea>
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
              <td class="contenido">
                <textarea formControlName="alcance" rows="3"
                  placeholder="Definir claramente el alcance del proyecto…">
                </textarea>
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

        <div class="nota-comp">
          Los componentes, actividades, horas y productos por estudiante se completan tras la aprobación.
          A continuación se muestra la estructura según el número de participantes indicado.
        </div>

        <div *ngIf="slots().length === 0" class="nota-comp" style="color:#F57F17;background:#FFF8E1">
          Sin estudiantes propuestos. La propuesta quedará disponible para asignación (0/5).
        </div>

        <ng-container *ngFor="let letra of slots()">
          <table class="f-table" style="margin-bottom:2px">
            <tbody>
              <tr>
                <td colspan="2" class="est-header">Estudiante {{ letra }}:</td>
              </tr>
              <tr>
                <td class="lbl" style="width:32%">Módulo / Componente</td>
                <td class="val" style="height:26px"></td>
              </tr>
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
              <tr>
                <td class="lbl">Producto(s) esperado(s)</td>
                <td class="val" style="height:40px"></td>
              </tr>
              <tr>
                <td class="lbl">Nombre del estudiante propuesto</td>
                <td class="val" style="height:24px"></td>
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
              <td class="val fijo">Asignado automáticamente al guardar</td>
            </tr>
            <tr>
              <td class="lbl">Estudiantes propuestos:</td>
              <td class="val" style="height:30px"></td>
            </tr>
            <tr>
              <td class="lbl">Resolución de la CPGIC:</td>
              <td class="val" style="height:45px"></td>
            </tr>
            <tr>
              <td class="lbl">Presidente de la CPGIC:</td>
              <td class="val" style="height:35px"></td>
            </tr>
            <tr>
              <td class="lbl">Fecha de aprobación:</td>
              <td class="val fijo">—</td>
            </tr>
          </tbody>
        </table>

        <!-- ── PIE ── -->
        <div class="f-footer">
          <span>Sistema TIC-FIS — Escuela Politécnica Nacional</span>
          <span>{{ esEdicion ? 'Modo edición' : 'Nuevo registro' }}</span>
        </div>

        <!-- ── BOTONES DE ACCIÓN ── -->
        <div class="acciones">
          <a routerLink="/propuestas" class="btn btn-outline">Cancelar</a>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">
            {{ saving() ? 'Guardando…' : (esEdicion ? 'Actualizar propuesta' : 'Guardar propuesta') }}
          </button>
        </div>

      </div>
    </form>
  `,
})
export class PropuestaFormComponent implements OnInit {
  private svc    = inject(PropuestaService);
  private route  = inject(ActivatedRoute);
  private router = inject(Router);
  private fb     = inject(FormBuilder);

  esEdicion   = false;
  private propuestaId = 0;
  saving = signal(false);
  error  = signal('');

  readonly letras = ['A', 'B', 'C', 'D', 'E'];
  readonly actividades = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

  // Asignaturas fijas, idénticas al formulario oficial F_AA_233A.
  readonly asignaturas =
    'Fundamentos de bases de datos (ISWD453)\n' +
    'Diseño de Software (ISWD523)\n' +
    'Metodologías ágiles (ISWD613)\n' +
    'Calidad de Software (ISWD652)\n' +
    'Aplicaciones Web Avanzadas (ISWD813)\n' +
    'Usabilidad y Accesibilidad (ISWD732)';

  form = this.fb.nonNullable.group({
    codigo:               ['', Validators.required],
    titulo:               ['', Validators.required],
    descripcion:          [''],
    problema:             [''],
    objetivoGeneral:      [''],
    alcance:              [''],
    estudiantesPropuestos: [0, [Validators.required, Validators.min(0), Validators.max(5)]],
  });

  get f() { return this.form.controls; }

  // Bloques de estudiante (A, B, C…) según el número de participantes indicado.
  slots(): string[] {
    const raw = Number(this.form.controls.estudiantesPropuestos.value) || 0;
    const n = Math.min(Math.max(raw, 0), this.letras.length);
    return this.letras.slice(0, n);
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.esEdicion = true;
      this.propuestaId = Number(id);
      this.form.controls.codigo.disable();
      this.svc.getById(this.propuestaId).subscribe({
        next:  (p) => this.form.patchValue(p),
        error: ()  => this.error.set('No se pudo cargar la propuesta.'),
      });
    }
  }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    this.error.set('');
    const v = this.form.getRawValue();
    const estudiantesPropuestos = Number(v.estudiantesPropuestos) || 0;

    const call$ = this.esEdicion
      ? this.svc.update(this.propuestaId, {
          titulo: v.titulo, descripcion: v.descripcion,
          problema: v.problema, objetivoGeneral: v.objetivoGeneral, alcance: v.alcance,
          estudiantesPropuestos,
        })
      : this.svc.create({ ...v, estudiantesPropuestos });

    call$.subscribe({
      next:  (res: any) => this.router.navigate(['/propuestas', this.esEdicion ? this.propuestaId : res?.id]),
      error: ()         => { this.error.set('Error al guardar. Intenta nuevamente.'); this.saving.set(false); },
    });
  }
}
