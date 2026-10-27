# Sistema Escolar (WinForms)

Sistema de gestión escolar para **alumnos**, **docentes** y **horarios** en Windows Forms (.NET Framework 4.7.2).

## Requisitos

- Windows con .NET Framework 4.7.2
- Visual Studio 2019+ (o MSBuild compatible con proyectos .NET Framework)

## Cómo ejecutar

1. Abrir `Sistema_Escolar.sln`.
2. Compilar en modo `Debug` o `Release`.
3. Ejecutar el proyecto `Sistema_Escolar`.

## Acceso inicial

La primera ejecución genera usuarios por defecto con contraseñas hasheadas y almacenadas en XML:

- `admin` (rol Administrador)
- `docente` (rol Docente)
- `alumno` (rol Estudiante)

> Si el archivo de usuarios se corrompe o no existe, el sistema restaura automáticamente estos usuarios base.

## Funcionalidades principales

- Login con validación estricta de credenciales.
- Control de permisos por rol:
  - **Administrador:** altas, ediciones y bajas.
  - **Docente:** edición limitada, sin eliminación.
  - **Estudiante:** solo consulta.
- Validaciones de negocio:
  - Formato de correo.
  - Formato de teléfono `55-1234-5678`.
  - Promedio entre `0` y `10`.
  - Horario con formato `HH:mm - HH:mm`.
  - Claves únicas (matrícula, ID docente y código).
- Integridad básica en horarios:
  - Docente debe existir en catálogo de docentes.
  - Grupo debe existir en catálogo de alumnos.
- Persistencia en XML con mejor manejo de errores.
- Exportación a CSV por módulo.
- Respaldo y restauración de datos.
- Bitácora de auditoría (`Datos/auditoria.log`).

## Ubicación de datos

Los archivos de datos se guardan en la carpeta `Datos` dentro del directorio de ejecución:

- `alumnos.xml`
- `docentes.xml`
- `horarios.xml`
- `usuarios.xml`
- `auditoria.log`
