using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sistema_Escolar
{
    public static class DatosEscolares
    {
        private static DataTable tablaAlumnos;
        private static DataTable tablaDocentes;
        private static DataTable tablaHorarios;

        private static readonly string rutaDatos = Path.Combine(Application.StartupPath, "Datos");
        private static readonly string archivoAlumnos = "alumnos.xml";
        private static readonly string archivoDocentes = "docentes.xml";
        private static readonly string archivoHorarios = "horarios.xml";
        private static readonly string archivoAuditoria = "auditoria.log";

        public static string RutaDatos => rutaDatos;
        public static string UsuarioActual { get; set; } = "Invitado";
        public static string RolActual { get; set; } = "Invitado";
        public static string UltimoErrorInicializacion { get; private set; } = string.Empty;
        public static string UltimoErrorGuardado { get; private set; } = string.Empty;

        static DatosEscolares()
        {
            InicializarTablas();
            CargarDatos();
        }

        private static void InicializarTablas()
        {
            tablaAlumnos = new DataTable("Alumnos");
            tablaAlumnos.Columns.Add("Matricula", typeof(string));
            tablaAlumnos.Columns.Add("Nombre", typeof(string));
            tablaAlumnos.Columns.Add("Apellidos", typeof(string));
            tablaAlumnos.Columns.Add("Grado", typeof(string));
            tablaAlumnos.Columns.Add("Grupo", typeof(string));
            tablaAlumnos.Columns.Add("Turno", typeof(string));
            tablaAlumnos.Columns.Add("Promedio", typeof(string));
            tablaAlumnos.Columns.Add("Estado", typeof(string));

            tablaDocentes = new DataTable("Docentes");
            tablaDocentes.Columns.Add("ID_Docente", typeof(string));
            tablaDocentes.Columns.Add("Nombre", typeof(string));
            tablaDocentes.Columns.Add("Apellidos", typeof(string));
            tablaDocentes.Columns.Add("Materia", typeof(string));
            tablaDocentes.Columns.Add("Telefono", typeof(string));
            tablaDocentes.Columns.Add("Email", typeof(string));
            tablaDocentes.Columns.Add("Turno", typeof(string));

            tablaHorarios = new DataTable("Horarios");
            tablaHorarios.Columns.Add("Codigo", typeof(string));
            tablaHorarios.Columns.Add("Materia", typeof(string));
            tablaHorarios.Columns.Add("Docente", typeof(string));
            tablaHorarios.Columns.Add("Dia", typeof(string));
            tablaHorarios.Columns.Add("Horario", typeof(string));
            tablaHorarios.Columns.Add("Aula", typeof(string));
            tablaHorarios.Columns.Add("Grupo", typeof(string));
        }

        private static void CargarDatos()
        {
            List<string> advertencias = new List<string>();

            try
            {
                if (!Directory.Exists(rutaDatos))
                {
                    Directory.CreateDirectory(rutaDatos);
                }

                CargarTablaDesdeArchivo(tablaAlumnos, Path.Combine(rutaDatos, archivoAlumnos), CargarDatosInicialesAlumnos, "Alumnos", advertencias);
                CargarTablaDesdeArchivo(tablaDocentes, Path.Combine(rutaDatos, archivoDocentes), CargarDatosInicialesDocentes, "Docentes", advertencias);
                CargarTablaDesdeArchivo(tablaHorarios, Path.Combine(rutaDatos, archivoHorarios), CargarDatosInicialesHorarios, "Horarios", advertencias);
            }
            catch (Exception ex)
            {
                if (tablaAlumnos.Rows.Count == 0) CargarDatosInicialesAlumnos();
                if (tablaDocentes.Rows.Count == 0) CargarDatosInicialesDocentes();
                if (tablaHorarios.Rows.Count == 0) CargarDatosInicialesHorarios();
                advertencias.Add("Error general al inicializar datos: " + ex.Message);
            }

            UltimoErrorInicializacion = advertencias.Count == 0
                ? string.Empty
                : string.Join(Environment.NewLine, advertencias.Distinct());
        }

        private static void CargarTablaDesdeArchivo(
            DataTable tabla,
            string rutaArchivo,
            Action cargarInicial,
            string nombreTabla,
            List<string> advertencias)
        {
            if (File.Exists(rutaArchivo))
            {
                try
                {
                    tabla.ReadXml(rutaArchivo);
                    return;
                }
                catch (Exception ex)
                {
                    tabla.Clear();
                    advertencias.Add($"No se pudo leer '{nombreTabla}' desde {Path.GetFileName(rutaArchivo)}. Se cargaron datos iniciales. Detalle: {ex.Message}");
                }
            }

            if (tabla.Rows.Count == 0)
            {
                cargarInicial();
            }
        }

        public static bool GuardarDatos(out string error)
        {
            try
            {
                if (!Directory.Exists(rutaDatos))
                {
                    Directory.CreateDirectory(rutaDatos);
                }

                tablaAlumnos.WriteXml(Path.Combine(rutaDatos, archivoAlumnos), XmlWriteMode.WriteSchema);
                tablaDocentes.WriteXml(Path.Combine(rutaDatos, archivoDocentes), XmlWriteMode.WriteSchema);
                tablaHorarios.WriteXml(Path.Combine(rutaDatos, archivoHorarios), XmlWriteMode.WriteSchema);

                error = string.Empty;
                UltimoErrorGuardado = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                error = "Error al guardar datos: " + ex.Message;
                UltimoErrorGuardado = error;
                return false;
            }
        }

        public static void GuardarDatos()
        {
            GuardarDatos(out _);
        }

        public static bool ExportarTablaCsv(string modulo, string rutaArchivo, out string error)
        {
            try
            {
                DataTable tabla = ObtenerTabla(modulo);
                StringBuilder sb = new StringBuilder();

                string[] headers = tabla.Columns.Cast<DataColumn>().Select(c => EscapeCsv(c.ColumnName)).ToArray();
                sb.AppendLine(string.Join(",", headers));

                foreach (DataRow row in tabla.Rows)
                {
                    if (row.RowState == DataRowState.Deleted) continue;
                    string[] values = tabla.Columns.Cast<DataColumn>()
                        .Select(c => EscapeCsv(row[c]?.ToString() ?? string.Empty))
                        .ToArray();
                    sb.AppendLine(string.Join(",", values));
                }

                File.WriteAllText(rutaArchivo, sb.ToString(), Encoding.UTF8);
                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                error = "No se pudo exportar CSV: " + ex.Message;
                return false;
            }
        }

        public static bool CrearRespaldo(string carpetaDestino, out string error)
        {
            try
            {
                if (!GuardarDatos(out error))
                {
                    return false;
                }

                string nombreCarpeta = "respaldo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                string rutaRespaldo = Path.Combine(carpetaDestino, nombreCarpeta);
                Directory.CreateDirectory(rutaRespaldo);

                CopiarSiExiste(Path.Combine(rutaDatos, archivoAlumnos), Path.Combine(rutaRespaldo, archivoAlumnos));
                CopiarSiExiste(Path.Combine(rutaDatos, archivoDocentes), Path.Combine(rutaRespaldo, archivoDocentes));
                CopiarSiExiste(Path.Combine(rutaDatos, archivoHorarios), Path.Combine(rutaRespaldo, archivoHorarios));
                CopiarSiExiste(Path.Combine(rutaDatos, "usuarios.xml"), Path.Combine(rutaRespaldo, "usuarios.xml"));
                CopiarSiExiste(Path.Combine(rutaDatos, archivoAuditoria), Path.Combine(rutaRespaldo, archivoAuditoria));

                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                error = "No se pudo generar el respaldo: " + ex.Message;
                return false;
            }
        }

        public static bool RestaurarDesdeRespaldo(string carpetaRespaldo, out string error)
        {
            try
            {
                if (!Directory.Exists(carpetaRespaldo))
                {
                    error = "La carpeta de respaldo no existe.";
                    return false;
                }

                if (!Directory.Exists(rutaDatos))
                {
                    Directory.CreateDirectory(rutaDatos);
                }

                CopiarSiExiste(Path.Combine(carpetaRespaldo, archivoAlumnos), Path.Combine(rutaDatos, archivoAlumnos));
                CopiarSiExiste(Path.Combine(carpetaRespaldo, archivoDocentes), Path.Combine(rutaDatos, archivoDocentes));
                CopiarSiExiste(Path.Combine(carpetaRespaldo, archivoHorarios), Path.Combine(rutaDatos, archivoHorarios));
                CopiarSiExiste(Path.Combine(carpetaRespaldo, "usuarios.xml"), Path.Combine(rutaDatos, "usuarios.xml"));
                CopiarSiExiste(Path.Combine(carpetaRespaldo, archivoAuditoria), Path.Combine(rutaDatos, archivoAuditoria));

                InicializarTablas();
                CargarDatos();
                SeguridadServicio.RecargarUsuarios();

                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                error = "No se pudo restaurar el respaldo: " + ex.Message;
                return false;
            }
        }

        private static void CopiarSiExiste(string origen, string destino)
        {
            if (File.Exists(origen))
            {
                File.Copy(origen, destino, true);
            }
        }

        public static IEnumerable<string> ObtenerDocentesDisponibles()
        {
            return tablaDocentes.AsEnumerable()
                .Select(r => string.Format(
                    "{0} {1}",
                    (r["Nombre"]?.ToString() ?? string.Empty).Trim(),
                    (r["Apellidos"]?.ToString() ?? string.Empty).Trim()).Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IEnumerable<string> ObtenerGruposDisponibles()
        {
            return tablaAlumnos.AsEnumerable()
                .Select(ConstruirGrupoAlumno)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string ConstruirGrupoAlumno(DataRow row)
        {
            string grado = (row["Grado"]?.ToString() ?? string.Empty).Trim();
            string grupo = (row["Grupo"]?.ToString() ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(grado) || string.IsNullOrWhiteSpace(grupo))
            {
                return string.Empty;
            }

            string digitos = new string(grado.Where(char.IsDigit).ToArray());
            string prefijo = string.IsNullOrWhiteSpace(digitos) ? grado : digitos;
            return (prefijo + "-" + grupo).ToUpperInvariant();
        }

        private static string EscapeCsv(string valor)
        {
            string limpio = valor.Replace("\"", "\"\"");
            return $"\"{limpio}\"";
        }

        private static void CargarDatosInicialesAlumnos()
        {
            if (tablaAlumnos.Rows.Count > 0) return;
            tablaAlumnos.Rows.Add("2024001", "Carlos", "García Méndez", "3ro", "A", "Matutino", "9.5", "Activo");
            tablaAlumnos.Rows.Add("2024002", "Valeria", "Ríos Navarrete", "2do", "B", "Matutino", "9.8", "Activo");
            tablaAlumnos.Rows.Add("2024003", "Mateo", "López Estrada", "1ro", "C", "Vespertino", "8.7", "Activo");
            tablaAlumnos.Rows.Add("2024004", "Sofía", "Castillo Morales", "3ro", "A", "Matutino", "9.2", "Activo");
            tablaAlumnos.Rows.Add("2024005", "Diego", "Herrera Gómez", "2do", "A", "Vespertino", "7.9", "Baja Temporal");
            tablaAlumnos.Rows.Add("2024006", "Camila", "Flores Ramírez", "1ro", "B", "Matutino", "9.6", "Activo");
            tablaAlumnos.Rows.Add("2024007", "Alejandro", "Vargas Silva", "3ro", "B", "Vespertino", "8.9", "Activo");
        }

        private static void CargarDatosInicialesDocentes()
        {
            if (tablaDocentes.Rows.Count > 0) return;
            tablaDocentes.Rows.Add("DOC-101", "Mariana", "Hernández Soto", "Matemáticas y Álgebra", "55-1234-5678", "m.hernandez@escuela.edu", "Completo");
            tablaDocentes.Rows.Add("DOC-102", "Roberto", "Peña Santana", "Programación Orientada a Objetos", "55-8765-4321", "r.pena@escuela.edu", "Completo");
            tablaDocentes.Rows.Add("DOC-103", "Beatriz", "Lara Domínguez", "Física Elemental", "55-4567-8901", "b.lara@escuela.edu", "Matutino");
            tablaDocentes.Rows.Add("DOC-104", "Fernando", "Navarro Cruz", "Estructura de Datos", "55-2345-6789", "f.navarro@escuela.edu", "Vespertino");
            tablaDocentes.Rows.Add("DOC-105", "Gabriela", "Montes Reyes", "Inglés Técnico", "55-9876-5432", "g.montes@escuela.edu", "Matutino");
        }

        private static void CargarDatosInicialesHorarios()
        {
            if (tablaHorarios.Rows.Count > 0) return;
            tablaHorarios.Rows.Add("HOR-001", "Programación C#", "Roberto Peña Santana", "Lunes y Miércoles", "08:00 - 10:00", "Laboratorio 2", "3-A");
            tablaHorarios.Rows.Add("HOR-002", "Álgebra Lineal", "Mariana Hernández Soto", "Martes y Jueves", "10:00 - 12:00", "Aula 104", "3-A");
            tablaHorarios.Rows.Add("HOR-003", "Física II", "Beatriz Lara Domínguez", "Lunes y Viernes", "12:00 - 14:00", "Aula 201", "2-B");
            tablaHorarios.Rows.Add("HOR-004", "Estructura de Datos", "Fernando Navarro Cruz", "Miércoles y Viernes", "16:00 - 18:00", "Laboratorio 1", "1-C");
            tablaHorarios.Rows.Add("HOR-005", "Inglés Avanzado", "Gabriela Montes Reyes", "Martes y Jueves", "07:00 - 08:30", "Aula Magna", "1-B");
        }

        public static DataTable ObtenerTabla(string modulo)
        {
            if (string.IsNullOrEmpty(modulo)) return tablaAlumnos;

            string m = modulo.ToLower().Trim();
            if (m.Contains("alum")) return tablaAlumnos;
            if (m.Contains("docen") || m.Contains("prof")) return tablaDocentes;
            if (m.Contains("hora")) return tablaHorarios;

            return tablaAlumnos;
        }

        public static void RegistrarAuditoria(string accion, string modulo, string detalle)
        {
            try
            {
                if (!Directory.Exists(rutaDatos))
                {
                    Directory.CreateDirectory(rutaDatos);
                }

                string linea = string.Format(
                    "[{0}] Usuario={1} Rol={2} Modulo={3} Accion={4} Detalle={5}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    UsuarioActual,
                    RolActual,
                    modulo,
                    accion,
                    detalle);

                File.AppendAllText(Path.Combine(rutaDatos, archivoAuditoria), linea + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // Evitar bloquear flujo principal por errores de auditoría.
            }
        }
    }
}
