using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Sistema_Escolar
{
    public static class SeguridadServicio
    {
        private static readonly DataTable tablaUsuarios = new DataTable("Usuarios");
        private static readonly string rutaUsuarios = Path.Combine(DatosEscolares.RutaDatos, "usuarios.xml");

        public static string UltimoErrorUsuarios { get; private set; } = string.Empty;

        static SeguridadServicio()
        {
            InicializarTablaUsuarios();
            CargarUsuarios();
        }

        private static void InicializarTablaUsuarios()
        {
            tablaUsuarios.Columns.Add("Usuario", typeof(string));
            tablaUsuarios.Columns.Add("Salt", typeof(string));
            tablaUsuarios.Columns.Add("PasswordHash", typeof(string));
            tablaUsuarios.Columns.Add("Rol", typeof(string));
            tablaUsuarios.Columns.Add("Activo", typeof(bool));
        }

        public static void RecargarUsuarios()
        {
            tablaUsuarios.Rows.Clear();
            CargarUsuarios();
        }

        private static void CargarUsuarios()
        {
            try
            {
                if (File.Exists(rutaUsuarios))
                {
                    tablaUsuarios.ReadXml(rutaUsuarios);
                }

                if (tablaUsuarios.Rows.Count == 0)
                {
                    CrearUsuariosPorDefecto();
                    GuardarUsuarios();
                }

                UltimoErrorUsuarios = string.Empty;
            }
            catch (Exception ex)
            {
                tablaUsuarios.Rows.Clear();
                CrearUsuariosPorDefecto();
                GuardarUsuarios();
                UltimoErrorUsuarios = "No se pudo leer el archivo de usuarios. Se restauraron usuarios por defecto. Detalle: " + ex.Message;
            }
        }

        private static void CrearUsuariosPorDefecto()
        {
            AgregarUsuario("admin", "Admin#2026", "Administrador");
            AgregarUsuario("docente", "Docente#2026", "Docente");
            AgregarUsuario("alumno", "Alumno#2026", "Estudiante");
        }

        private static void AgregarUsuario(string usuario, string password, string rol)
        {
            string salt = GenerarSalt();
            string hash = CalcularHash(password, salt);
            tablaUsuarios.Rows.Add(usuario, salt, hash, rol, true);
        }

        private static void GuardarUsuarios()
        {
            tablaUsuarios.WriteXml(rutaUsuarios, XmlWriteMode.WriteSchema);
        }

        public static bool ValidarCredenciales(string usuario, string password, out string rol, out string mensaje)
        {
            rol = string.Empty;
            mensaje = "Usuario o contraseña incorrectos.";

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                mensaje = "Ingresa usuario y contraseña.";
                return false;
            }

            DataRow fila = tablaUsuarios.AsEnumerable()
                .FirstOrDefault(r => string.Equals(
                    r.Field<string>("Usuario")?.Trim(),
                    usuario.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (fila == null)
            {
                return false;
            }

            bool activo = fila.Field<bool?>("Activo") ?? false;
            if (!activo)
            {
                mensaje = "La cuenta se encuentra desactivada.";
                return false;
            }

            string salt = fila.Field<string>("Salt") ?? string.Empty;
            string hashGuardado = fila.Field<string>("PasswordHash") ?? string.Empty;
            string hashIngresado = CalcularHash(password, salt);

            if (!HashesCoinciden(hashGuardado, hashIngresado))
            {
                return false;
            }

            rol = fila.Field<string>("Rol") ?? "Usuario";
            mensaje = string.Empty;
            return true;
        }

        private static bool HashesCoinciden(string hashA, string hashB)
        {
            try
            {
                byte[] bytesA = Convert.FromBase64String(hashA);
                byte[] bytesB = Convert.FromBase64String(hashB);
                return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
            }
            catch
            {
                return false;
            }
        }

        private static string GenerarSalt()
        {
            byte[] bytes = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        private static string CalcularHash(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
