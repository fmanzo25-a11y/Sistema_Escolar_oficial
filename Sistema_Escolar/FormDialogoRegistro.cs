using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Sistema_Escolar
{
    public partial class FormDialogoRegistro : Form
    {
        private readonly string modulo;
        private readonly DataRow filaEditar;
        private readonly DataTable tabla;
        private readonly Dictionary<string, Control> camposControles = new Dictionary<string, Control>();

        public FormDialogoRegistro(string tipoModulo, DataRow fila = null)
        {
            this.modulo = (tipoModulo ?? string.Empty).ToLowerInvariant();
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

                Control inputCtrl = CrearControlCampo(col.ColumnName, yPos);
                CargarValorEdicion(col.ColumnName, inputCtrl);

                camposControles[col.ColumnName] = inputCtrl;
                pnlCampos.Controls.Add(lbl);
                pnlCampos.Controls.Add(inputCtrl);

                yPos += 45;
            }

            this.Controls.Add(pnlCampos);

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

        private Control CrearControlCampo(string columnName, int yPos)
        {
            ComboBox combo = null;

            if (columnName == "Turno")
            {
                combo = CrearCombo(yPos, new[] { "Matutino", "Vespertino", "Completo", "Mixto" });
            }
            else if (columnName == "Estado")
            {
                combo = CrearCombo(yPos, new[] { "Activo", "Inactivo", "Baja Temporal", "Graduado" });
            }
            else if (columnName == "Grado")
            {
                combo = CrearCombo(yPos, new[] { "1ro", "2do", "3ro", "4to", "5to", "6to" });
            }
            else if (columnName == "Grupo" && modulo.Contains("alum"))
            {
                combo = CrearCombo(yPos, new[] { "A", "B", "C", "D", "E", "F" });
            }
            else if (columnName == "Dia" && modulo.Contains("hora"))
            {
                combo = CrearCombo(yPos, new[] { "Lunes y Miércoles", "Martes y Jueves", "Lunes y Viernes", "Miércoles y Viernes", "Sábado" });
            }
            else if (columnName == "Docente" && modulo.Contains("hora"))
            {
                combo = CrearCombo(yPos, DatosEscolares.ObtenerDocentesDisponibles().ToArray());
            }
            else if (columnName == "Grupo" && modulo.Contains("hora"))
            {
                combo = CrearCombo(yPos, DatosEscolares.ObtenerGruposDisponibles().ToArray());
            }

            if (combo != null) return combo;

            return new TextBox
            {
                Location = new Point(195, yPos - 3),
                Size = new Size(260, 26),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F)
            };
        }

        private ComboBox CrearCombo(int yPos, string[] items)
        {
            ComboBox cb = new ComboBox
            {
                Location = new Point(195, yPos - 3),
                Size = new Size(260, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cb.Items.AddRange(items);
            if (cb.Items.Count > 0) cb.SelectedIndex = 0;
            return cb;
        }

        private void CargarValorEdicion(string columnName, Control control)
        {
            if (filaEditar == null) return;
            if (filaEditar[columnName] == DBNull.Value) return;

            string valor = filaEditar[columnName].ToString();
            if (control is ComboBox cb)
            {
                int idx = cb.FindStringExact(valor);
                if (idx >= 0)
                {
                    cb.SelectedIndex = idx;
                }
                else
                {
                    cb.Items.Add(valor);
                    cb.SelectedItem = valor;
                }
            }
            else if (control is TextBox txt)
            {
                txt.Text = valor;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos(out string mensaje, out Control controlError))
            {
                MessageBox.Show(mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                controlError?.Focus();
                return;
            }

            try
            {
                if (filaEditar == null)
                {
                    DataRow nuevaFila = tabla.NewRow();
                    foreach (var kvp in camposControles)
                    {
                        nuevaFila[kvp.Key] = kvp.Value.Text.Trim();
                    }
                    tabla.Rows.Add(nuevaFila);
                    DatosEscolares.RegistrarAuditoria("AGREGAR", modulo, "Nuevo registro creado.");
                }
                else
                {
                    foreach (var kvp in camposControles)
                    {
                        filaEditar[kvp.Key] = kvp.Value.Text.Trim();
                    }
                    DatosEscolares.RegistrarAuditoria("EDITAR", modulo, "Registro actualizado.");
                }

                if (!DatosEscolares.GuardarDatos(out string error))
                {
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarCampos(out string mensaje, out Control controlError)
        {
            mensaje = string.Empty;
            controlError = null;

            foreach (var kvp in camposControles)
            {
                string key = kvp.Key;
                string valor = kvp.Value.Text.Trim();
                bool obligatorio = key.Contains("Matricula") || key.Contains("ID") || key.Contains("Codigo") || key.Contains("Nombre") || key.Contains("Materia");

                if (obligatorio && string.IsNullOrWhiteSpace(valor))
                {
                    mensaje = $"El campo '{FormatearNombreColumna(key)}' es obligatorio.";
                    controlError = kvp.Value;
                    return false;
                }

                if (key == "Email" && !string.IsNullOrWhiteSpace(valor))
                {
                    if (!Regex.IsMatch(valor, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        mensaje = "El correo electrónico no tiene un formato válido.";
                        controlError = kvp.Value;
                        return false;
                    }
                }

                if (key == "Telefono" && !string.IsNullOrWhiteSpace(valor))
                {
                    if (!Regex.IsMatch(valor, @"^\d{2}-\d{4}-\d{4}$"))
                    {
                        mensaje = "El teléfono debe seguir el formato 55-1234-5678.";
                        controlError = kvp.Value;
                        return false;
                    }
                }

                if (key == "Promedio" && !string.IsNullOrWhiteSpace(valor))
                {
                    if (!decimal.TryParse(valor.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal promedio) || promedio < 0 || promedio > 10)
                    {
                        mensaje = "El promedio debe ser un número entre 0 y 10.";
                        controlError = kvp.Value;
                        return false;
                    }
                }

                if (key == "Horario" && !string.IsNullOrWhiteSpace(valor))
                {
                    if (!Regex.IsMatch(valor, @"^\d{2}:\d{2}\s*-\s*\d{2}:\d{2}$"))
                    {
                        mensaje = "El horario debe tener el formato HH:mm - HH:mm.";
                        controlError = kvp.Value;
                        return false;
                    }
                }
            }

            string claveUnica = ObtenerCampoClaveUnica();
            if (!string.IsNullOrWhiteSpace(claveUnica))
            {
                string valorClave = camposControles[claveUnica].Text.Trim();
                bool duplicado = tabla.AsEnumerable()
                    .Where(r => filaEditar == null || r != filaEditar)
                    .Any(r => string.Equals((r[claveUnica]?.ToString() ?? string.Empty).Trim(), valorClave, StringComparison.OrdinalIgnoreCase));

                if (duplicado)
                {
                    mensaje = $"Ya existe un registro con el mismo valor en '{FormatearNombreColumna(claveUnica)}'.";
                    controlError = camposControles[claveUnica];
                    return false;
                }
            }

            if (modulo.Contains("hora"))
            {
                string docente = camposControles.ContainsKey("Docente") ? camposControles["Docente"].Text.Trim() : string.Empty;
                string grupo = camposControles.ContainsKey("Grupo") ? camposControles["Grupo"].Text.Trim().ToUpperInvariant() : string.Empty;

                string valorOriginalDocente = filaEditar == null ? string.Empty : (filaEditar["Docente"]?.ToString() ?? string.Empty).Trim();
                string valorOriginalGrupo = filaEditar == null ? string.Empty : (filaEditar["Grupo"]?.ToString() ?? string.Empty).Trim().ToUpperInvariant();

                HashSet<string> docentes = new HashSet<string>(DatosEscolares.ObtenerDocentesDisponibles(), StringComparer.OrdinalIgnoreCase);
                HashSet<string> grupos = new HashSet<string>(DatosEscolares.ObtenerGruposDisponibles(), StringComparer.OrdinalIgnoreCase);

                if (!string.Equals(docente, valorOriginalDocente, StringComparison.OrdinalIgnoreCase) && !docentes.Contains(docente))
                {
                    mensaje = "El docente seleccionado no existe en el catálogo de docentes.";
                    controlError = camposControles["Docente"];
                    return false;
                }

                if (!string.Equals(grupo, valorOriginalGrupo, StringComparison.OrdinalIgnoreCase) && grupos.Count > 0 && !grupos.Contains(grupo))
                {
                    mensaje = "El grupo seleccionado no existe en el catálogo de alumnos.";
                    controlError = camposControles["Grupo"];
                    return false;
                }
            }

            return true;
        }

        private string ObtenerCampoClaveUnica()
        {
            if (camposControles.ContainsKey("Matricula")) return "Matricula";
            if (camposControles.ContainsKey("ID_Docente")) return "ID_Docente";
            if (camposControles.ContainsKey("Codigo")) return "Codigo";
            return string.Empty;
        }

        private Color ObtenerColorTema()
        {
            if (modulo.Contains("alum")) return Color.FromArgb(20, 120, 80);
            if (modulo.Contains("docen")) return Color.FromArgb(90, 45, 130);
            if (modulo.Contains("hora")) return Color.FromArgb(25, 95, 160);
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
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            return char.ToUpper(texto[0]) + texto.Substring(1);
        }
    }
}
