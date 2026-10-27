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
        private Button btnExportarCsv;
        private Button btnRespaldo;
        private Button btnRestaurar;

        public Sistema_Alum(string tipoModulo)
        {
            InitializeComponent();
            this.modulo = string.IsNullOrEmpty(tipoModulo) ? "alumnos" : tipoModulo.ToLower().Trim();
        }

        private void Sistema_Alum_Load(object sender, EventArgs e)
        {
            ConfigurarTemaModulo();
            CargarDatos();
            ConfigurarFuncionesAdicionales();
            AplicarPermisosPorRol();
        }

        private void ConfigurarTemaModulo()
        {
            Color colorTema;
            string icono;
            string titulo;
            string subtitulo;

            if (modulo.Contains("alum"))
            {
                colorTema = Color.FromArgb(20, 120, 80);
                icono = "🎓";
                titulo = "Gestión de Alumnos";
                subtitulo = "Expedientes estudiantiles, matrículas, promedios y estatus académico.";
                this.Text = "Portal Escolar - Control de Alumnos";
            }
            else if (modulo.Contains("docen"))
            {
                colorTema = Color.FromArgb(90, 45, 130);
                icono = "👨‍🏫";
                titulo = "Plantilla de Docentes";
                subtitulo = "Registro de profesores, materias asignadas, teléfonos y correos institucionales.";
                this.Text = "Portal Escolar - Directorio de Docentes";
            }
            else
            {
                colorTema = Color.FromArgb(25, 95, 160);
                icono = "📅";
                titulo = "Cronograma de Horarios";
                subtitulo = "Distribución de clases, asignación de aulas, docentes y grupos escolares.";
                this.Text = "Portal Escolar - Horarios de Clases";
            }

            pnlEncabezado.BackColor = colorTema;
            lblTitulo.Text = $"{icono} {titulo}";
            lblSubtitulo.Text = subtitulo;
            btnAgregar.BackColor = colorTema;
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

        private void ConfigurarFuncionesAdicionales()
        {
            if (btnExportarCsv != null) return;

            btnExportarCsv = CrearBotonAccion("📤 CSV", 365, BtnExportarCsv_Click);
            btnRespaldo = CrearBotonAccion("💾 Respaldo", 455, BtnRespaldo_Click);
            btnRestaurar = CrearBotonAccion("♻ Restaurar", 560, BtnRestaurar_Click);

            pnlBarraHerramientas.Controls.Add(btnExportarCsv);
            pnlBarraHerramientas.Controls.Add(btnRespaldo);
            pnlBarraHerramientas.Controls.Add(btnRestaurar);
        }

        private Button CrearBotonAccion(string texto, int x, EventHandler click)
        {
            Button btn = new Button
            {
                Text = texto,
                Location = new Point(x, 12),
                Size = new Size(90, 36),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 80, 95),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(190, 200, 215);
            btn.Click += click;
            return btn;
        }

        private void AplicarPermisosPorRol()
        {
            string rol = (DatosEscolares.RolActual ?? string.Empty).ToLowerInvariant();

            bool esAdmin = rol.Contains("admin");
            bool esDocente = rol.Contains("docente");

            bool puedeAgregarEditar = esAdmin || esDocente;
            bool puedeEliminar = esAdmin;

            if (esDocente && modulo.Contains("docen"))
            {
                puedeAgregarEditar = false;
            }

            if (!esAdmin && !esDocente)
            {
                puedeAgregarEditar = false;
                puedeEliminar = false;
            }

            btnAgregar.Enabled = puedeAgregarEditar;
            btnEditar.Enabled = puedeAgregarEditar;
            btnEliminar.Enabled = puedeEliminar;

            if (!puedeAgregarEditar && !puedeEliminar)
            {
                lblEstado.Text = "🔒 Modo consulta: no cuentas con permisos para modificar registros.";
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
                if (DatosEscolares.GuardarDatos(out string error))
                {
                    DatosEscolares.RegistrarAuditoria("ELIMINAR", modulo, "Se eliminó " + identificador);
                    ActualizarContador();
                    lblEstado.Text = "🗑️ Registro eliminado correctamente.";
                }
                else
                {
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRecargar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            vistaDatos.RowFilter = string.Empty;
            ActualizarContador();
            lblEstado.Text = "🔄 Lista de datos actualizada.";
        }

        private void BtnExportarCsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Archivo CSV (*.csv)|*.csv";
                saveDialog.FileName = $"reporte_{modulo}_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                if (saveDialog.ShowDialog(this) != DialogResult.OK) return;

                if (DatosEscolares.ExportarTablaCsv(modulo, saveDialog.FileName, out string error))
                {
                    DatosEscolares.RegistrarAuditoria("EXPORTAR_CSV", modulo, "Archivo: " + saveDialog.FileName);
                    lblEstado.Text = "📤 Exportación CSV completada.";
                }
                else
                {
                    MessageBox.Show(error, "Error de exportación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRespaldo_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Selecciona la carpeta destino del respaldo";
                if (folderDialog.ShowDialog(this) != DialogResult.OK) return;

                if (DatosEscolares.CrearRespaldo(folderDialog.SelectedPath, out string error))
                {
                    DatosEscolares.RegistrarAuditoria("RESPALDO", modulo, "Destino: " + folderDialog.SelectedPath);
                    lblEstado.Text = "💾 Respaldo generado correctamente.";
                }
                else
                {
                    MessageBox.Show(error, "Error de respaldo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRestaurar_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Selecciona la carpeta del respaldo a restaurar";
                if (folderDialog.ShowDialog(this) != DialogResult.OK) return;

                DialogResult confirmar = MessageBox.Show(
                    "Restaurar reemplazará los datos actuales. ¿Deseas continuar?",
                    "Confirmar restauración",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmar != DialogResult.Yes) return;

                if (DatosEscolares.RestaurarDesdeRespaldo(folderDialog.SelectedPath, out string error))
                {
                    SeguridadServicio.RecargarUsuarios();
                    CargarDatos();
                    DatosEscolares.RegistrarAuditoria("RESTAURAR", modulo, "Origen: " + folderDialog.SelectedPath);
                    lblEstado.Text = "♻ Restauración completada.";
                }
                else
                {
                    MessageBox.Show(error, "Error de restauración", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && btnEditar.Enabled)
            {
                btnEditar_Click(sender, e);
            }
        }

        private void btnRegresar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ActualizarContador()
        {
            int total = vistaDatos != null ? vistaDatos.Count : 0;
            lblTotalRegistros.Text = $"Total: {total} registro(s)";
        }
    }
}
