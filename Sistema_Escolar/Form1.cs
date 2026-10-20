using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sistema_Escolar
{
    public partial class Menu_Login : Form
    {
        public Menu_Login()
        {
            InitializeComponent();
            this.AcceptButton = btnIniciarSesion;
            this.CancelButton = btnSalir;
        }

        private void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string contrasena = txtContrasena.Text.Trim();

            // Validación de campos vacíos
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasena))
            {
                MostrarMensaje("Por favor ingresa usuario y contraseña.", true);
                txtUsuario.Focus();
                return;
            }

            // Validación de credenciales
            // Se aceptan las cuentas estándar o cualquier usuario con clave no vacía
            bool valido = false;
            string rol = "Administrador";

            if (usuario.ToLower() == "admin" && (contrasena == "1234" || contrasena == "admin"))
            {
                valido = true;
                rol = "Administrador";
            }
            else if (usuario.ToLower() == "docente" && contrasena == "1234")
            {
                valido = true;
                rol = "Docente";
            }
            else if (usuario.ToLower() == "alumno" && contrasena == "1234")
            {
                valido = true;
                rol = "Estudiante";
            }
            else if (contrasena.Length >= 3)
            {
                // Permitir acceso flexible a cuentas personalizadas
                valido = true;
                rol = usuario;
            }

            if (valido)
            {
                DatosEscolares.UsuarioActual = usuario;
                MostrarMensaje("¡Acceso correcto! Iniciando sesión...", false);

                // Abrir el Menú Principal
                Menu_Princ menu = new Menu_Princ();
                menu.FormClosed += (s, args) =>
                {
                    // Cuando se cierra el menú principal, volver a mostrar el login
                    this.txtContrasena.Clear();
                    this.lblMensaje.Visible = false;
                    this.Show();
                };

                this.Hide();
                menu.Show();
            }
            else
            {
                MostrarMensaje("Usuario o contraseña incorrectos.", true);
                txtContrasena.SelectAll();
                txtContrasena.Focus();
            }
        }

        private void chkMostrarContra_CheckedChanged(object sender, EventArgs e)
        {
            txtContrasena.UseSystemPasswordChar = !chkMostrarContra.Checked;
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                "¿Estás seguro de que deseas salir del sistema escolar?",
                "Confirmar salida",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (dr == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void MostrarMensaje(string texto, bool esError)
        {
            lblMensaje.Text = texto;
            lblMensaje.ForeColor = esError ? Color.FromArgb(200, 35, 35) : Color.FromArgb(20, 130, 70);
            lblMensaje.Visible = true;
        }
    }
}
