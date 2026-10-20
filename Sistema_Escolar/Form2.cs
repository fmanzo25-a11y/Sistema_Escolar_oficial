using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Sistema_Escolar
{
    public partial class Sistema_Alum : Form
    {
        private string modulo;
        private DataTable tablaDatos;
        private DataView vistaDatos;

        public Sistema_Alum(string tipoModulo)
        {
            InitializeComponent();
            this.modulo = string.IsNullOrEmpty(tipoModulo) ? "alumnos" : tipoModulo.ToLower().Trim();
        }

        private void Sistema_Alum_Load(object sender, EventArgs e)
        {
            ConfigurarTemaModulo();
            CargarDatos();
        }

        private void ConfigurarTemaModulo()
        {
            Color colorTema;
            string icono;
            string titulo;
            string subtitulo;

            if (modulo.Contains("alum"))
            {
                colorTema = Color.FromArgb(20, 120, 80); // Verde Esmeralda
                icono = "🎓";
                titulo = "Gestión de Alumnos";
                subtitulo = "Expedientes estudiantiles, matrículas, promedios y estatus académico.";
                this.Text = "Portal Escolar - Control de Alumnos";
            }
            else if (modulo.Contains("docen"))
            {
                colorTema = Color.FromArgb(90, 45, 130); // Morado
                icono = "👨‍🏫";
                titulo = "Plantilla de Docentes";
                subtitulo = "Registro de profesores, materias asignadas, teléfonos y correos institucionales.";
                this.Text = "Portal Escolar - Directorio de Docentes";
            }
            else
            {
                colorTema = Color.FromArgb(25, 95, 160); // Azul
                icono = "📅";
                titulo = "Cronograma de Horarios";
                subtitulo = "Distribución de clases, asignación de aulas, docentes y grupos escolares.";
                this.Text = "Portal Escolar - Horarios de Clases";
            }

            pnlEncabezado.BackColor = colorTema;
            lblTitulo.Text = $"{icono} {titulo}";
            lblSubtitulo.Text = subtitulo;
            btnAgregar.BackColor = colorTema;

            // Personalizar encabezado del DataGridView con el color del tema
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = colorTema;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = colorTema;
        }

        private void CargarDatos()
        {
            tablaDatos = DatosEscolares.ObtenerTabla(modulo);
            vistaDatos = new DataView(tablaDatos);
            dataGridView1.DataSource = vistaDatos;

            PersonalizarColumnas();
            ActualizarContador();
        }

        private void PersonalizarColumnas()
        {
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                switch (col.Name)
                {
                    case "Matricula": col.HeaderText = "Matrícula"; break;
                    case "Nombre": col.HeaderText = "Nombre(s)"; break;
                    case "Apellidos": col.HeaderText = "Apellidos"; break;
                    case "Grado": col.HeaderText = "Grado"; break;
                    case "Grupo": col.HeaderText = "Grupo"; break;
                    case "Turno": col.HeaderText = "Turno"; break;
                    case "Promedio": col.HeaderText = "Promedio"; break;
                    case "Estado": col.HeaderText = "Estatus"; break;
                    case "ID_Docente": col.HeaderText = "ID Docente"; break;
                    case "Materia": col.HeaderText = "Materia"; break;
                    case "Telefono": col.HeaderText = "Teléfono"; break;
                    case "Email": col.HeaderText = "Correo Electrónico"; break;
                    case "Codigo": col.HeaderText = "Código"; break;
                    case "Docente": col.HeaderText = "Docente Asignado"; break;
                    case "Dia": col.HeaderText = "Días"; break;
                    case "Horario": col.HeaderText = "Horario"; break;
                    case "Aula": col.HeaderText = "Aula"; break;
                }
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            if (vistaDatos == null || tablaDatos == null) return;

            string filtro = txtBuscar.Text.Trim().Replace("'", "''");

            if (string.IsNullOrEmpty(filtro))
            {
                vistaDatos.RowFilter = string.Empty;
            }
            else
            {
                // Construir filtro dinámico por texto en todas las columnas relevantes
                string rowFilter = "";
                foreach (DataColumn col in tablaDatos.Columns)
                {
                    if (rowFilter.Length > 0) rowFilter += " OR ";
                    rowFilter += $"[{col.ColumnName}] LIKE '%{filtro}%'";
                }
                vistaDatos.RowFilter = rowFilter;
            }

            ActualizarContador();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            using (FormDialogoRegistro formReg = new FormDialogoRegistro(modulo, null))
            {
                if (formReg.ShowDialog(this) == DialogResult.OK)
                {
                    ActualizarContador();
                    lblEstado.Text = "✅ Nuevo registro guardado con éxito.";
                }
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor selecciona una fila de la tabla para editar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRowView filaVista = (DataRowView)dataGridView1.SelectedRows[0].DataBoundItem;
            DataRow filaReal = filaVista.Row;

            using (FormDialogoRegistro formReg = new FormDialogoRegistro(modulo, filaReal))
            {
                if (formReg.ShowDialog(this) == DialogResult.OK)
                {
                    ActualizarContador();
                    lblEstado.Text = "✏️ Registro actualizado con éxito.";
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor selecciona una fila para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRowView filaVista = (DataRowView)dataGridView1.SelectedRows[0].DataBoundItem;
            DataRow filaReal = filaVista.Row;

            string identificador = filaReal[0].ToString() + " - " + filaReal[1].ToString();

            DialogResult dr = MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar permanentemente el registro:\n\n'{identificador}'?",
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (dr == DialogResult.Yes)
            {
                filaReal.Delete();
                DatosEscolares.GuardarDatos();
                ActualizarContador();
                lblEstado.Text = "🗑️ Registro eliminado correctamente.";
            }
        }

        private void btnRecargar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            vistaDatos.RowFilter = string.Empty;
            ActualizarContador();
            lblEstado.Text = "🔄 Lista de datos actualizada.";
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnEditar_Click(sender, e);
            }
        }

        private void btnRegresar_Click(object sender, EventArgs e)
        {
            this.Close(); // El FormClosed en Menu_Princ vuelve a mostrar el menú principal
        }

        private void ActualizarContador()
        {
            int total = vistaDatos != null ? vistaDatos.Count : 0;
            lblTotalRegistros.Text = $"Total: {total} registro(s)";
        }
    }
}
