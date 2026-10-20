using System;
using System.Globalization;
using System.Windows.Forms;

namespace Sistema_Escolar
{
    public partial class Menu_Princ : Form
    {
        public Menu_Princ()
        {
            InitializeComponent();
        }

        private void Menu_Princ_Load(object sender, EventArgs e)
        {
            lblUsuarioSesion.Text = "👤 Usuario: " + DatosEscolares.UsuarioActual;
            
            // Mostrar fecha actual en formato amigable en español
            try
            {
                CultureInfo culturaEs = new CultureInfo("es-ES");
                lblFecha.Text = "📅 " + DateTime.Now.ToString("dddd, d 'de' MMMM 'de' yyyy", culturaEs);
            }
            catch
            {
                lblFecha.Text = "📅 " + DateTime.Now.ToShortDateString();
            }
        }

        private void btnAlumnos_Click(object sender, EventArgs e)
        {
            AbrirModulo("alumnos");
        }

        private void btnHorarios_Click(object sender, EventArgs e)
        {
            AbrirModulo("horarios");
        }

        private void btnDocentes_Click(object sender, EventArgs e)
        {
            AbrirModulo("docentes");
        }

        private void AbrirModulo(string tipoModulo)
        {
            Sistema_Alum formDestino = new Sistema_Alum(tipoModulo);
            formDestino.FormClosed += (s, args) =>
            {
                // Cuando se cierra la ventana del módulo, volver a mostrar el Menú Principal
                this.Show();
            };

            this.Hide();
            formDestino.Show();
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                "¿Deseas cerrar tu sesión actual y volver a la pantalla de acceso?",
                "Cerrar Sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (dr == DialogResult.Yes)
            {
                this.Close(); // Al cerrarse, el evento FormClosed en Menu_Login lo reabre
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                "¿Estás seguro de que deseas salir del sistema?",
                "Confirmar salida",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (dr == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
