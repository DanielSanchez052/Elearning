**DOCUMENTO DE ALCANCE**

**Plataforma E-Learning Multipaís**

Versión 1.0 \| 2025

**1. Resumen Ejecutivo**

Este documento define el alcance completo para el desarrollo de una
plataforma de e-learning multipaís. La plataforma permitirá a usuarios
de distintos países acceder a contenido segmentado geográficamente,
completar cursos con videos, documentos y evaluaciones, y participar en
un sistema de gamificación basado en medallas y niveles de progreso.

El desarrollo se llevará a cabo en fases iterativas (MVPs), utilizando
el siguiente stack tecnológico: ASP.NET Core 10 como backend principal,
Next.js para el frontend, PostgreSQL como base de datos relacional, y
Redis para caché y operaciones en tiempo real.

**2. Stack Tecnológico**

  -----------------------------------------------------------------------
  **Capa**              **Tecnología**           **Propósito**
  --------------------- ------------------------ ------------------------
  Backend API           ASP.NET Core 10 (C#)     API REST principal

  Motor de Gamificación Microservicio C#         Medallas, niveles,
                        separado                 reglas

  Frontend              Next.js (React)          Interfaz web responsive

  Base de datos         PostgreSQL               Datos relacionales
                                                 principales

  Caché / Tiempo real   Redis                    Sesiones, progreso en
                                                 vivo

  Almacenamiento de     S3 / Azure Blob + CDN    Videos, PDFs, imágenes
  medios                                         

  Autenticación         ASP.NET Identity + JWT   Registro, login, roles
  -----------------------------------------------------------------------

**3. Alcance Funcional**

**3.1 Módulo de Países y Segmentación de Contenido**

La plataforma soportará entre 1 y 3 países en su lanzamiento inicial,
con capacidad de escalar. El contenido (cursos, materiales) estará
etiquetado con un campo country_scope que puede ser \'global\' o
asociado a países específicos.

-   Gestión de países activos desde el panel de Super Admin.

-   Detección automática del país del usuario al momento del registro.

-   Filtrado de catálogo de cursos según el país del usuario.

-   Contenido global visible para todos los países simultáneamente.

-   Contenido exclusivo por país, no visible para usuarios de otras
    regiones.

**3.2 Módulo de Usuarios y Autenticación**

Los usuarios se registran de forma autónoma en la plataforma. El sistema
de roles es jerárquico y tiene cuatro niveles bien definidos.

-   Registro propio del usuario mediante formulario (email, contraseña,
    país de residencia).

-   Verificación de email al momento del registro.

-   Login con JWT, manejo de sesiones mediante Redis.

-   Recuperación de contraseña vía email.

**Los cuatro roles del sistema son:**

-   Super Admin: acceso total a todos los países, configuración global,
    gestión de todos los módulos.

-   Admin por País: gestión de usuarios, cursos y reportes dentro de su
    país asignado.

-   Instructor / Creador de Contenido: creación y edición de cursos,
    carga de materiales.

-   Usuario Final / Estudiante: acceso al catálogo, inscripción a
    cursos, progreso y gamificación.

**3.3 Módulo de Cursos y Contenido**

Los cursos serán la unidad central de aprendizaje. Cada curso puede
contener múltiples lecciones organizadas en módulos o secciones.

**Tipos de contenido soportados:**

-   Videos: cargados en almacenamiento en la nube (S3 / Azure Blob) con
    reproducción vía CDN.

-   PDFs y documentos: descargables o visualizables dentro de la
    plataforma.

-   Evaluaciones y quizzes: preguntas de opción múltiple con puntaje y
    umbral de aprobación configurable.

**Funcionalidades del módulo:**

-   Creación y edición de cursos por Instructores y Admins.

-   Asignación de cursos a países (global o específico).

-   El usuario elige libremente a qué cursos inscribirse del catálogo
    disponible para su país.

-   Seguimiento de progreso por lección y por curso (porcentaje
    completado).

-   Registro de fecha y hora de inicio y finalización de cada curso.

-   Marcación automática de curso completado al superar todas sus
    lecciones y evaluaciones.

-   Generación de certificado PDF descargable al completar un curso
    (incluye nombre del usuario, nombre del curso y fecha de
    completación).

**3.4 Módulo de Gamificación**

El sistema de gamificación opera mediante dos mecanismos distintos:
medallas (logros puntuales) y el nivel \'Móvil\' (estado continuo y
dinámico).

**3.4.1 Sistema de Medallas**

Las medallas se otorgan automáticamente cuando se cumple una condición
específica. No se pueden revocar una vez obtenidas.

  -----------------------------------------------------------------------
  **Medalla**         **Condición de Obtención**   **Observaciones**
  ------------------- ---------------------------- ----------------------
  Inicio de Sesión    El usuario inicia sesión en  Puede haber variantes:
                      la plataforma                primer login, racha de
                                                   7 días, etc.

  Curso Completado    El usuario finaliza un curso Una medalla por cada
                      al 100%                      curso completado

  Velocista           El usuario completa un curso El tiempo límite se
                      en menos de X tiempo         define por curso en su
                      configurado                  configuración
  -----------------------------------------------------------------------

*Nota: las variantes exactas de cada medalla (ej. rachas de login,
número de medallas de completación) se definirán en detalle durante la
fase de diseño.*

**3.4.2 Nivel \'Móvil\'**

El nivel \'Móvil\' es un indicador dinámico que refleja el estado actual
del usuario respecto al total de cursos activos en la plataforma para su
país. A diferencia de las medallas, puede subir o bajar en función del
catálogo activo.

Fórmula de cálculo:

**Nivel Móvil (%) = Cursos completados por el usuario / Total de cursos
activos en la plataforma × 100**

-   El denominador se actualiza automáticamente cuando se publican o
    desactivan cursos.

-   El porcentaje se recalcula en tiempo real (con apoyo de Redis)
    cuando hay cambios en el catálogo.

-   El nivel se muestra visualmente en el perfil del usuario como un
    indicador de progreso.

-   Los rangos de nivel (ej. Bronce / Plata / Oro según porcentaje)
    quedan pendientes de definición en la fase de diseño.

**3.5 Módulo de Notificaciones**

El sistema enviará notificaciones a los usuarios por dos canales: dentro
de la plataforma (in-app) y por correo electrónico.

  ------------------------------------------------------------------------
  **Evento**                     **In-App**           **Email**
  ------------------------------ -------------------- --------------------
  Obtención de una medalla       Sí                   Sí

  Publicación de curso nuevo en  Sí                   Sí
  su país                                             

  Recordatorio de cursos         Sí                   Sí
  pendientes                                          
  ------------------------------------------------------------------------

*La frecuencia de los recordatorios (diaria, semanal, etc.) será
configurable por el Admin.*

**3.6 Módulo de Reportes y Estadísticas**

El panel de reportes estará disponible para Admins y Super Admin, con
visibilidad limitada al país asignado en el caso de los Admins por país.

-   Progreso individual por usuario: cursos inscritos, completados,
    tiempo promedio, medallas obtenidas.

-   Reportes por país: tasa de completación, usuarios activos, cursos
    más populares.

-   Rankings y leaderboards: top usuarios por medallas, por cursos
    completados, por nivel móvil.

-   Exportación de reportes a formato Excel (.xlsx) y PDF.

**4. Fuera del Alcance (Versión Inicial)**

Los siguientes elementos quedan explícitamente excluidos del alcance del
proyecto en su fase inicial, pudiendo incorporarse en versiones futuras:

-   Soporte para contenido SCORM (paquetes de e-learning externos).

-   Aplicación móvil nativa (iOS / Android). La plataforma será
    responsive web.

-   Integración con sistemas de recursos humanos (HR), Active Directory
    o SSO corporativo.

-   Interfaz en múltiples idiomas: la plataforma tendrá un único idioma
    de interfaz.

-   Asignación obligatoria de cursos por rol o cargo.

-   Videoconferencias o clases en vivo.

-   Foros o funcionalidades de comunidad entre usuarios.

**5. Fases del Proyecto (MVP)**

El proyecto se desarrollará en fases iterativas, entregando valor
funcional en cada etapa.

  -----------------------------------------------------------------------
  **Fase**   **Nombre**          **Contenido Principal**
  ---------- ------------------- ----------------------------------------
  MVP 1      Base de la          Registro y login de usuarios, roles,
             plataforma          gestión de países, catálogo de cursos,
                                 visualización de videos y PDFs.

  MVP 2      Aprendizaje         Evaluaciones y quizzes, seguimiento de
             completo            progreso, marcación de cursos
                                 completados, generación de certificados
                                 PDF.

  MVP 3      Gamificación        Sistema de medallas, cálculo y
                                 visualización del nivel Móvil,
                                 notificaciones in-app y por email.

  MVP 4      Reportes y          Panel de reportes,
             administración      rankings/leaderboards, exportación
                                 Excel/PDF, panel Admin por país.
  -----------------------------------------------------------------------

**6. Supuestos y Restricciones**

**6.1 Supuestos**

-   El cliente proveerá el contenido (videos, PDFs, preguntas de
    evaluación) para cargar en la plataforma.

-   Los tiempos límite para la medalla \'Velocista\' serán definidos por
    el cliente por cada curso antes del MVP 2.

-   Los rangos del nivel Móvil (umbrales de porcentaje por categoría)
    serán definidos antes del MVP 3.

-   Se dispondrá de una cuenta activa en AWS o Azure para el
    almacenamiento de medios.

-   El idioma de la interfaz será español para la versión inicial.

**6.2 Restricciones**

-   No se integrarán sistemas externos de autenticación (SSO, LDAP,
    Active Directory) en esta versión.

-   El número máximo de países en el lanzamiento inicial es 3.

-   La plataforma no tendrá versión nativa para dispositivos móviles; se
    priorizará el diseño responsive.

**7. Criterios de Aceptación General**

-   Un usuario puede registrarse, verificar su email e iniciar sesión
    correctamente.

-   El catálogo de cursos muestra solo el contenido correspondiente al
    país del usuario.

-   El sistema otorga medallas automáticamente cuando se cumple la
    condición definida.

-   El nivel Móvil se actualiza correctamente cuando se publican o
    desactivan cursos.

-   Al completar un curso, el usuario puede descargar su certificado en
    PDF.

-   Los reportes reflejan datos reales y la exportación a Excel/PDF
    funciona correctamente.

-   La plataforma funciona correctamente en navegadores modernos
    (Chrome, Firefox, Safari, Edge) en desktop y mobile.

*Este documento está sujeto a revisión y aprobación por todas las partes
involucradas.*
