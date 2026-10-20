using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Sistema_Escolar
{
    public partial class FormDialogoRegistro : Form
    {
        private string modulo;
        private DataRow filaEditar;
        private DataTable tabla;
        private Dictionary<string, Control> camposControles = new Dictionary<string, Control>();

        public FormDialogoRegistro(string tipoModulo, DataRow fila = null)
        {
            this.modulo = tipoModulo.ToLower();
            this.filaEditar = fila;
            this.tabla = DatosEscolares.ObtenerTabla(this.modulo);

            ConfigurarVentana();
            ConstruirFormulario();
        }

        private void ConfigurarVentana()
        {
            this.Text = (filaEditar == null ? "Nuevo Registro" : "Editar Registro") + " - " + Capitalizar(modulo);
            this.Size = new Size(540, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5F);
        }

        private void ConstruirFormulario()
        {
            // Panel de Encabezado
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = ObtenerColorTema()
            };

            Label lblTitulo = new Label
            {
                Text = filaEditar == null ? "➕ Registrar " + ObtenerNombreSingular() : "✏️ Modificar " + ObtenerNombreSingular(),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 12),
                AutoSize = true
            };

            Label lblSubtitulo = new Label
            {
                Text = "Completa los campos y presiona Guardar para aplicar los cambios.",
                ForeColor = Color.FromArgb(230, 240, 255),
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(22, 40),
                AutoSize = true
            };

            headerPanel.Controls.Add(lblTitulo);
            headerPanel.Controls.Add(lblSubtitulo);
            this.Controls.Add(headerPanel);

            // Panel Contenedor de Campos (con scroll si es necesario)
            Panel pnlCampos = new Panel
            {
                Location = new Point(20, 80),
                Size = new Size(485, 390),
                AutoScroll = true
            };

            int yPos = 10;
            foreach (DataColumn col in tabla.Columns)
            {
                Label lbl = new Label
                {
                    Text = FormatearNombreColumna(col.ColumnName) + ":",
                    Location = new Point(10, yPos),
                    Size = new Size(180, 20),
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 60, 75)
                };

                Control inputCtrl;

                // ComboBox para campos predefinidos
                if (col.ColumnName == "Turno")
                {
                    ComboBox cb = new ComboBox
                    {
                        Location = new Point(195, yPos - 3),
                        Size = new Size(260, 26),
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    cb.Items.AddRange(new object[] { "Matutino", "Vespertino", "Completo", "Mixto" });
                    cb.SelectedIndex = 0;
                    inputCtrl = cb;
                }
                else if (col.ColumnName == "Estado")
                {
                    ComboBox cb = new ComboBox
                    {
                        Location = new Point(195, yPos - 3),
                        Size = new Size(260, 26),
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    cb.Items.AddRange(new object[] { "Activo", "Inactivo", "Baja Temporal", "Graduado" });
                    cb.SelectedIndex = 0;
                    inputCtrl = cb;
                }
                else if (col.ColumnName == "Grado")
                {
                    ComboBox cb = new ComboBox
                    {
                        Location = new Point(195, yPos - 3),
                        Size = new Size(260, 26),
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    cb.Items.AddRange(new object[] { "1ro", "2do", "3ro", "4to", "5to", "6to" });
                    cb.SelectedIndex = 0;
                    inputCtrl = cb;
                }
                else
                {
                    TextBox txt = new TextBox
                    {
                        Location = new Point(195, yPos - 3),
                        Size = new Size(260, 26),
                        BorderStyle = BorderStyle.FixedSingle,
                        Font = new Font("Segoe UI", 9.5F)
                    };
                    inputCtrl = txt;
                }

                // Si estamos editando, cargar el valor existente
                if (filaEditar != null && filaEditar[col.ColumnName] != DBNull.Value)
                {
                    string valor = filaEditar[col.ColumnName].ToString();
                    if (inputCtrl is ComboBox cb)
                    {
                        int idx = cb.FindStringExact(valor);
                        if (idx >= 0) cb.SelectedIndex = idx;
                        else { cb.Items.Add(valor); cb.SelectedItem = valor; }
                    }
                    else if (inputCtrl is TextBox txt)
                    {
                        txt.Text = valor;
                    }
                }

                camposControles[col.ColumnName] = inputCtrl;
                pnlCampos.Controls.Add(lbl);
                pnlCampos.Controls.Add(inputCtrl);

                yPos += 45;
            }

            this.Controls.Add(pnlCampos);

            // Panel Inferior con Botones
            Panel footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.FromArgb(235, 238, 242)
            };

            Button btnCancelar = new Button
            {
                Text = "Cancelar",
                Size = new Size(110, 36),
                Location = new Point(265, 14),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(80, 80, 80),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(180, 190, 200);
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            Button btnGuardar = new Button
            {
                Text = "💾 Guardar",
                Size = new Size(125, 36),
                Location = new Point(385, 14),
                BackColor = ObtenerColorTema(),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            footerPanel.Controls.Add(btnCancelar);
            footerPanel.Controls.Add(btnGuardar);
            this.Controls.Add(footerPanel);

            this.AcceptButton = btnGuardar;
            this.CancelButton = btnCancelar;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // Validar campos obligatorios
            foreach (var kvp in camposControles)
            {
                string valor = kvp.Value.Text.Trim();
                if (string.IsNullOrEmpty(valor) && (kvp.Key.Contains("Matricula") || kvp.Key.Contains("ID") || kvp.Key.Contains("Codigo") || kvp.Key.Contains("Nombre") || kvp.Key.Contains("Materia")))
                {
                    MessageBox.Show($"El campo '{FormatearNombreColumna(kvp.Key)}' es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    kvp.Value.Focus();
                    return;
                }
            }

            try
            {
                if (filaEditar == null)
                {
                    // Nuevo registro
                    DataRow nuevaFila = tabla.NewRow();
                    foreach (var kvp in camposControles)
                    {
                        nuevaFila[kvp.Key] = kvp.Value.Text.Trim();
                    }
                    tabla.Rows.Add(nuevaFila);
                }
                else
                {
                    // Edición
                    foreach (var kvp in camposControles)
                    {
                        filaEditar[kvp.Key] = kvp.Value.Text.Trim();
                    }
                }

                DatosEscolares.GuardarDatos();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Color ObtenerColorTema()
        {
            if (modulo.Contains("alum")) return Color.FromArgb(20, 120, 80);    // Verde Esmeralda
            if (modulo.Contains("docen")) return Color.FromArgb(90, 45, 130);   // Morado Elegante
            if (modulo.Contains("hora")) return Color.FromArgb(25, 95, 160);    // Azul Profesional
            return Color.FromArgb(40, 60, 90);
        }

        private string ObtenerNombreSingular()
        {
            if (modulo.Contains("alum")) return "Alumno";
            if (modulo.Contains("docen")) return "Docente";
            if (modulo.Contains("hora")) return "Horario de Clase";
            return "Registro";
        }

        private string FormatearNombreColumna(string col)
        {
            switch (col)
            {
                case "Matricula": return "Matrícula";
                case "ID_Docente": return "ID de Docente";
                case "Telefono": return "Teléfono";
                case "Dia": return "Días de Clase";
                case "Horario": return "Horario (De - A)";
                case "Codigo": return "Código de Materia";
                default: return col;
            }
        }

        private string Capitalizar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return char.ToUpper(texto[0]) + texto.Substring(1);
        }
    }
}
