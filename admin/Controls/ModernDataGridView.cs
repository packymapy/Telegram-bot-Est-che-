using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdminApp.Controls
{
    public class ModernDataGridView : DataGridView
    {
        private Color _headerBackColor = Colors.Primary;
        private Color _headerForeColor = Color.White;
        private Color _cellBackColor = Color.White;
        private Color _cellForeColor = Color.Black;
        private Color _alternatingRowColor = Color.FromArgb(248, 249, 250);
        private Color _selectionColor = Colors.PrimaryLight;
        private Color _gridColor = Color.FromArgb(220, 220, 220);

        public ModernDataGridView()
        {
            this.BackgroundColor = ThemeManager.GetBgColor();
            this.ForeColor = ThemeManager.GetTextColor();
            this.GridColor = ThemeManager.GetBorderColor();
            this.BorderStyle = BorderStyle.None;
            this.EnableHeadersVisualStyles = false;
            this.RowHeadersVisible = false;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.ReadOnly = true;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.MultiSelect = false;
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.RowTemplate.Height = 35;
            this.Font = new Font("Segoe UI", 9);
            this.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            this.ColumnHeadersDefaultCellStyle.BackColor = _headerBackColor;
            this.ColumnHeadersDefaultCellStyle.ForeColor = _headerForeColor;
            this.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.ColumnHeadersHeight = 40;
            this.DefaultCellStyle.BackColor = _cellBackColor;
            this.DefaultCellStyle.ForeColor = _cellForeColor;
            this.DefaultCellStyle.SelectionBackColor = _selectionColor;
            this.DefaultCellStyle.SelectionForeColor = _cellForeColor;
            this.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);
            this.AlternatingRowsDefaultCellStyle.BackColor = _alternatingRowColor;
            this.AlternatingRowsDefaultCellStyle.ForeColor = _cellForeColor;
            this.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            this.GridColor = _gridColor;
            this.CellFormatting += ModernDataGridView_CellFormatting;
        }

        private void ModernDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is DateTime dateTime)
            {
                e.Value = dateTime.ToString("dd.MM.yyyy HH:mm");
                e.FormattingApplied = true;
            }
            
            if (e.Value is decimal price)
            {
                e.Value = price.ToString("N2") + " ₽";
                e.FormattingApplied = true;
            }
        }

        public void ApplyTheme()
        {
            this.BackgroundColor = ThemeManager.GetBgColor();
            this.ForeColor = ThemeManager.GetTextColor();
            this.GridColor = ThemeManager.GetBorderColor();
            this.Refresh();
        }
    }
}
