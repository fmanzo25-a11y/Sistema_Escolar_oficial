using System;
using System.Drawing;
using System.Windows.Forms;

public class TransparentTextBox : TextBox
{
    public TransparentTextBox()
    {
        // Configuraciones indispensables para evitar parpadeos y habilitar transparencia
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        BackColor = Color.Transparent;
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        BackColor = Color.Transparent;
    }

    // ESTO ES LO QUE FALTA: Obligar al programa a dibujar las letras
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Usamos TextRenderer para pintar el texto con la fuente y el color asignados
        TextRenderer.DrawText(
            e.Graphics,
            this.Text,
            this.Font,
            this.ClientRectangle,
            this.ForeColor,
            TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak
        );
    }

    // Forzar el rediseño cuando el usuario escribe o borra texto
    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        this.Invalidate();
    }
}