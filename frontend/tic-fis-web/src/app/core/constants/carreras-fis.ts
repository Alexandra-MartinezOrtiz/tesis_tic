/**
 * Carreras de grado vigentes de la Facultad de Ingeniería de Sistemas (FIS) de la
 * Escuela Politécnica Nacional, con sus asignaturas y códigos oficiales.
 *
 * Datos extraídos de las mallas curriculares oficiales publicadas por la FIS / EPN
 * (pénsums 2020 y 2023). Todas las carreras pertenecen al Departamento de
 * Informática y Ciencias de la Computación (DICC).
 *
 * Se incluyen los niveles intermedios y superiores (4° a 9°), que son los que
 * se usan en los Trabajos de Integración Curricular.
 */

export interface Asignatura {
  /** Código oficial de la asignatura, p.ej. "ISWD453". */
  codigo: string;
  /** Nombre de la asignatura. */
  nombre: string;
}

export interface CarreraFis {
  /** Nombre corto/clave de la carrera, p.ej. "Software". */
  clave: string;
  /** Nombre oficial mostrado en el formulario. */
  nombre: string;
  /** Título profesional que otorga. */
  titulo: string;
  /** Asignaturas seleccionables para esta carrera. */
  asignaturas: Asignatura[];
}

/** Departamento común a todas las carreras de la FIS. */
export const DEPARTAMENTO_FIS = 'Departamento de Informática y Ciencias de la Computación';

export const CARRERAS_FIS: CarreraFis[] = [
  {
    clave: 'Software',
    nombre: 'Software',
    titulo: 'Ingeniero/a de Software',
    asignaturas: [
      { codigo: 'ISWD414', nombre: 'Ingeniería de Software y de Requerimientos' },
      { codigo: 'ICCD422', nombre: 'Compiladores y Lenguajes' },
      { codigo: 'ISWD433', nombre: 'Fundamentos de Sistemas de Información' },
      { codigo: 'ICCD442', nombre: 'Estructura de Datos y Algoritmos II' },
      { codigo: 'ISWD453', nombre: 'Fundamentos de Bases de Datos' },
      { codigo: 'ISWD523', nombre: 'Diseño de Software' },
      { codigo: 'ICCD533', nombre: 'Computación Gráfica' },
      { codigo: 'ISWD543', nombre: 'Inteligencia Artificial y Aprendizaje Automático' },
      { codigo: 'ISWD553', nombre: 'Bases de Datos Distribuidas' },
      { codigo: 'ADMD511', nombre: 'Gestión Organizacional' },
      { codigo: 'ISWD613', nombre: 'Aplicaciones Web' },
      { codigo: 'ISWD622', nombre: 'Metodologías Ágiles' },
      { codigo: 'ISWD633', nombre: 'Construcción y Evolución de Software' },
      { codigo: 'ICCD643', nombre: 'Tecnologías de Seguridad' },
      { codigo: 'ISWD652', nombre: 'Calidad de Software' },
      { codigo: 'ISWD713', nombre: 'Aplicaciones Móviles' },
      { codigo: 'ISWD723', nombre: 'Interacción Humano Computador' },
      { codigo: 'ISWD732', nombre: 'Usabilidad y Accesibilidad' },
      { codigo: 'ISWD743', nombre: 'Business Intelligence' },
      { codigo: 'ISWD752', nombre: 'Verificación y Validación de Software' },
      { codigo: 'ISWD762', nombre: 'Automatización de Procesos' },
      { codigo: 'ISWD813', nombre: 'Aplicaciones Web Avanzadas' },
      { codigo: 'ISWD823', nombre: 'Desarrollo de Juegos Interactivos' },
      { codigo: 'ISWD833', nombre: 'Auditoría Informática' },
      { codigo: 'ICCD842', nombre: 'Profesionalismo en Informática' },
      { codigo: 'ISWD853', nombre: 'Desarrollo de Software Seguro' },
      { codigo: 'ISWD913', nombre: 'Sistemas Embebidos' },
      { codigo: 'ISWD922', nombre: 'Gestión de Proyectos de Software' },
      { codigo: 'TITD104', nombre: 'Diseño de Trabajo de Integración Curricular' },
      { codigo: 'TITD201', nombre: 'Trabajo de Integración Curricular' },
    ],
  },
  {
    clave: 'Computacion',
    nombre: 'Computación',
    titulo: 'Ingeniero/a en Ciencias de la Computación',
    asignaturas: [
      { codigo: 'ICCD412', nombre: 'Métodos Numéricos' },
      { codigo: 'ICCD422', nombre: 'Compiladores y Lenguajes' },
      { codigo: 'ICCD432', nombre: 'Multiprocesamiento y Arquitecturas Alternativas' },
      { codigo: 'ICCD442', nombre: 'Estructura de Datos y Algoritmos II' },
      { codigo: 'ICCD453', nombre: 'Fundamentos de Bases de Datos' },
      { codigo: 'ICCD463', nombre: 'Redes de Computadores I' },
      { codigo: 'ICCD512', nombre: 'Ingeniería de Software I' },
      { codigo: 'ICCD523', nombre: 'Inteligencia Artificial' },
      { codigo: 'ICCD533', nombre: 'Computación Gráfica' },
      { codigo: 'ISWD553', nombre: 'Bases de Datos Distribuidas' },
      { codigo: 'ICCD563', nombre: 'Redes de Computadores II' },
      { codigo: 'ADMD511', nombre: 'Gestión Organizacional' },
      { codigo: 'ISWD613', nombre: 'Aplicaciones Web' },
      { codigo: 'ICCD623', nombre: 'Data Mining y Machine Learning' },
      { codigo: 'ICCD632', nombre: 'Ingeniería de Software II' },
      { codigo: 'ICCD643', nombre: 'Tecnologías de Seguridad' },
      { codigo: 'ICCD654', nombre: 'Computación Distribuida' },
      { codigo: 'ISWD713', nombre: 'Aplicaciones Móviles' },
      { codigo: 'ISWD723', nombre: 'Interacción Humano Computador' },
      { codigo: 'ICCD733', nombre: 'Seguridad Informática' },
      { codigo: 'ISWD743', nombre: 'Business Intelligence' },
      { codigo: 'ICCD753', nombre: 'Recuperación de Información' },
      { codigo: 'ICCD814', nombre: 'Modelos y Simulación' },
      { codigo: 'ICCD823', nombre: 'Cloud Computing' },
      { codigo: 'ICCD833', nombre: 'Auditoría Informática' },
      { codigo: 'ICCD842', nombre: 'Profesionalismo en Informática' },
      { codigo: 'ICCD943', nombre: 'Gestión de Tecnologías de la Información y Comunicación' },
      { codigo: 'TITD104', nombre: 'Diseño de Trabajo de Integración Curricular' },
      { codigo: 'TITD201', nombre: 'Trabajo de Integración Curricular' },
    ],
  },
  {
    clave: 'SistemasInformacion',
    nombre: 'Sistemas de Información',
    titulo: 'Ingeniero/a en Sistemas de Información',
    asignaturas: [
      { codigo: 'ISID413', nombre: 'Administración de la Información y Datos' },
      { codigo: 'ISID423', nombre: 'Análisis y Diseño de Sistemas de Información' },
      { codigo: 'ISID432', nombre: 'Desarrollo y Mantenimiento de Software' },
      { codigo: 'ICCD442', nombre: 'Estructura de Datos y Algoritmos II' },
      { codigo: 'ISWD553', nombre: 'Bases de Datos Distribuidas' },
      { codigo: 'ICCD533', nombre: 'Computación Gráfica' },
      { codigo: 'ISWD732', nombre: 'Usabilidad y Accesibilidad' },
      { codigo: 'ISID533', nombre: 'Arquitectura Empresarial' },
      { codigo: 'ISWD723', nombre: 'Interacción Humano Computador' },
      { codigo: 'ISID553', nombre: 'Infraestructura de Tecnologías de Información' },
      { codigo: 'ADMD511', nombre: 'Gestión Organizacional' },
      { codigo: 'ICCD753', nombre: 'Recuperación de Información' },
      { codigo: 'ISID623', nombre: 'Automatización de Procesos de Negocio' },
      { codigo: 'ISID633', nombre: 'Gestión del Conocimiento' },
      { codigo: 'ISID642', nombre: 'User Experience' },
      { codigo: 'ISID653', nombre: 'Fundamentos de Ciencia de Datos' },
      { codigo: 'ADMD611', nombre: 'Gestión de Procesos y Calidad' },
      { codigo: 'ICCD523', nombre: 'Inteligencia Artificial' },
      { codigo: 'ISID723', nombre: 'Analítica Predictiva' },
      { codigo: 'ICCD733', nombre: 'Seguridad Informática' },
      { codigo: 'ISID743', nombre: 'Gobernanza y Calidad de Datos' },
      { codigo: 'ISID752', nombre: 'Liderazgo y Comunicación' },
      { codigo: 'ADMD711', nombre: 'Ingeniería Financiera' },
      { codigo: 'ICCD823', nombre: 'Cloud Computing' },
      { codigo: 'ISWD743', nombre: 'Business Intelligence' },
      { codigo: 'ICCD833', nombre: 'Auditoría Informática' },
      { codigo: 'ISID844', nombre: 'Gestión de Proyectos de Sistemas de Información' },
      { codigo: 'ISID852', nombre: 'Internet of Things' },
      { codigo: 'ISID943', nombre: 'Sistemas Empresariales' },
      { codigo: 'TITD104', nombre: 'Diseño de Trabajo de Integración Curricular' },
      { codigo: 'TITD201', nombre: 'Trabajo de Integración Curricular' },
    ],
  },
  {
    clave: 'CienciaDatosIA',
    nombre: 'Ciencia de Datos e Inteligencia Artificial',
    titulo: 'Ingeniero/a en Ciencia de Datos e Inteligencia Artificial',
    asignaturas: [
      { codigo: 'IDSD413', nombre: 'Fundamentos de Big Data' },
      { codigo: 'IDSD422', nombre: 'Estadística y Programación para Ciencias de Datos I' },
      { codigo: 'ICCD422', nombre: 'Compiladores y Lenguajes' },
      { codigo: 'ICCD442', nombre: 'Estructura de Datos y Algoritmos II' },
      { codigo: 'ISWD553', nombre: 'Bases de Datos Distribuidas' },
      { codigo: 'IDSD513', nombre: 'Infraestructura para Big Data' },
      { codigo: 'IDSD522', nombre: 'Estadística y Programación para Ciencias de Datos II' },
      { codigo: 'IDSD533', nombre: 'Computación Numérica' },
      { codigo: 'ICCD523', nombre: 'Inteligencia Artificial' },
      { codigo: 'ISID432', nombre: 'Desarrollo y Mantenimiento de Software' },
      { codigo: 'IDSD613', nombre: 'Almacenamiento de Datos Masivos' },
      { codigo: 'IDSD623', nombre: 'Computación Paralela para Big Data' },
      { codigo: 'IDSD633', nombre: 'Aprendizaje Automático I' },
      { codigo: 'ISWD613', nombre: 'Aplicaciones Web' },
      { codigo: 'ICCD643', nombre: 'Tecnologías de Seguridad' },
      { codigo: 'IDSD713', nombre: 'Procesamiento de Datos Masivos' },
      { codigo: 'IDSD723', nombre: 'Aprendizaje Automático II' },
      { codigo: 'IDSD733', nombre: 'Minería de Datos I' },
      { codigo: 'IDSD742', nombre: 'Seguridad y Privacidad de Datos' },
      { codigo: 'ISID642', nombre: 'User Experience' },
      { codigo: 'IDSD813', nombre: 'Visualización de Datos' },
      { codigo: 'IDSD823', nombre: 'Servicios en la Nube para Big Data' },
      { codigo: 'IDSD833', nombre: 'Minería de Datos II' },
      { codigo: 'IDSD843', nombre: 'Analítica Avanzada' },
      { codigo: 'ICCD842', nombre: 'Profesionalismo en Informática' },
      { codigo: 'IDSD933', nombre: 'Aplicaciones Emergentes de Ciencia de Datos' },
      { codigo: 'IDSD942', nombre: 'Administración de Proyectos de Ciencia de Datos' },
      { codigo: 'TITD104', nombre: 'Diseño de Trabajo de Integración Curricular' },
      { codigo: 'TITD201', nombre: 'Trabajo de Integración Curricular' },
    ],
  },
];
