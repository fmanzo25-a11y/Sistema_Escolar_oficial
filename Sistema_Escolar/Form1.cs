using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sistema_Escolar
{
    public partial class Menu_Login : Form
    {
        private static bool advertenciasMostradas;

        public Menu_Login()
        {
            InitializeComponent();
            this.AcceptButton = btnIniciarSesion;
            this.CancelButton = btnSalir;
            this.Load += Menu_Login_Load;
        }

        private void Menu_Login_Load(object sender, EventArgs e)
        {
            if (advertenciasMostradas) return;

            advertenciasMostradas = true;
            string advertencias = (DatosEscolares.UltimoErrorInicializacion + Environment.NewLine + SeguridadServicio.UltimoErrorUsuarios).Trim();
            if (!string.IsNullOrWhiteSpace(advertencias))
            {
                MessageBox.Show(
                    advertencias,
                    "Advertencias de inicialización",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
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

            bool valido = SeguridadServicio.ValidarCredenciales(usuario, contrasena, out string rol, out string mensajeError);

            if (valido)
            {
                DatosEscolares.UsuarioActual = usuario;
                DatosEscolares.RolActual = rol;
                MostrarMensaje("¡Acceso correcto! Iniciando sesión...", false);
                DatosEscolares.RegistrarAuditoria("INICIO_SESION", "login", "Inicio de sesión exitoso.");

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
                DatosEscolares.RegistrarAuditoria("INICIO_SESION_FALLIDO", "login", "Intento fallido para usuario '" + usuario + "'.");
                MostrarMensaje(mensajeError, true);
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
