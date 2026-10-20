using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Sistema_Escolar
{
    public static class DatosEscolares
    {
        private static DataTable tablaAlumnos;
        private static DataTable tablaDocentes;
        private static DataTable tablaHorarios;

        private static readonly string rutaDatos = Path.Combine(Application.StartupPath, "Datos");

        static DatosEscolares()
        {
            InicializarTablas();
            CargarDatos();
        }

        private static void InicializarTablas()
        {
            // --- TABLA ALUMNOS ---
            tablaAlumnos = new DataTable("Alumnos");
            tablaAlumnos.Columns.Add("Matricula", typeof(string));
            tablaAlumnos.Columns.Add("Nombre", typeof(string));
            tablaAlumnos.Columns.Add("Apellidos", typeof(string));
            tablaAlumnos.Columns.Add("Grado", typeof(string));
            tablaAlumnos.Columns.Add("Grupo", typeof(string));
            tablaAlumnos.Columns.Add("Turno", typeof(string));
            tablaAlumnos.Columns.Add("Promedio", typeof(string));
            tablaAlumnos.Columns.Add("Estado", typeof(string));

            // --- TABLA DOCENTES ---
            tablaDocentes = new DataTable("Docentes");
            tablaDocentes.Columns.Add("ID_Docente", typeof(string));
            tablaDocentes.Columns.Add("Nombre", typeof(string));
            tablaDocentes.Columns.Add("Apellidos", typeof(string));
            tablaDocentes.Columns.Add("Materia", typeof(string));
            tablaDocentes.Columns.Add("Telefono", typeof(string));
            tablaDocentes.Columns.Add("Email", typeof(string));
            tablaDocentes.Columns.Add("Turno", typeof(string));

            // --- TABLA HORARIOS ---
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
            try
            {
                if (!Directory.Exists(rutaDatos))
                {
                    Directory.CreateDirectory(rutaDatos);
                }

                string archivoAlumnos = Path.Combine(rutaDatos, "alumnos.xml");
                string archivoDocentes = Path.Combine(rutaDatos, "docentes.xml");
                string archivoHorarios = Path.Combine(rutaDatos, "horarios.xml");

                if (File.Exists(archivoAlumnos))
                    tablaAlumnos.ReadXml(archivoAlumnos);
                else
                    CargarDatosInicialesAlumnos();

                if (File.Exists(archivoDocentes))
                    tablaDocentes.ReadXml(archivoDocentes);
                else
                    CargarDatosInicialesDocentes();

                if (File.Exists(archivoHorarios))
                    tablaHorarios.ReadXml(archivoHorarios);
                else
                    CargarDatosInicialesHorarios();
            }
            catch
            {
                CargarDatosInicialesAlumnos();
                CargarDatosInicialesDocentes();
                CargarDatosInicialesHorarios();
            }
        }

        public static void GuardarDatos()
        {
            try
            {
                if (!Directory.Exists(rutaDatos))
                {
                    Directory.CreateDirectory(rutaDatos);
                }

                tablaAlumnos.WriteXml(Path.Combine(rutaDatos, "alumnos.xml"), XmlWriteMode.WriteSchema);
                tablaDocentes.WriteXml(Path.Combine(rutaDatos, "docentes.xml"), XmlWriteMode.WriteSchema);
                tablaHorarios.WriteXml(Path.Combine(rutaDatos, "horarios.xml"), XmlWriteMode.WriteSchema);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al guardar datos: " + ex.Message);
            }
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
            tablaHorarios.Rows.Add("HOR-001", "Programación C#", "Ing. Roberto Peña", "Lunes y Miércoles", "08:00 - 10:00", "Laboratorio 2", "3-A");
            tablaHorarios.Rows.Add("HOR-002", "Álgebra Lineal", "Dra. Mariana Hernández", "Martes y Jueves", "10:00 - 12:00", "Aula 104", "3-A");
            tablaHorarios.Rows.Add("HOR-003", "Física II", "Mtra. Beatriz Lara", "Lunes y Viernes", "12:00 - 14:00", "Aula 201", "2-B");
            tablaHorarios.Rows.Add("HOR-004", "Estructura de Datos", "Ing. Fernando Navarro", "Miércoles y Viernes", "16:00 - 18:00", "Laboratorio 1", "1-C");
            tablaHorarios.Rows.Add("HOR-005", "Inglés Avanzado", "Lic. Gabriela Montes", "Martes y Jueves", "07:00 - 08:30", "Aula Magna", "1-B");
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

        public static string UsuarioActual { get; set; } = "Administrador";
    }
}
