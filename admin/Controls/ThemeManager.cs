using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdminApp.Controls
{
    public static class ThemeManager
    {
        public enum ThemeType
        {
            Light,
            Dark
        }
      
        private static ThemeType _currentTheme = ThemeType.Light;
        public static event EventHandler ThemeChanged;
        public static ThemeType CurrentTheme
          
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ThemeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static Color GetBgColor()
        {
            return CurrentTheme == ThemeType.Light 
                ? Colors.LightBg 
                : Colors.DarkBg;
        }

        public static Color GetTextColor()
        {
            return CurrentTheme == ThemeType.Light 
                ? Colors.LightText 
                : Colors.DarkText;
        }

        public static Color GetPanelColor()
        {
            return CurrentTheme == ThemeType.Light 
                ? Colors.LightPanel 
                : Colors.DarkPanel;
        }

        public static Color GetBorderColor()
        {
            return CurrentTheme == ThemeType.Light 
                ? Colors.LightBorder 
                : Colors.DarkBorder;
        }

        public static void ApplyTheme(Control control)
        {
            ApplyThemeToControl(control);
        }

        private static void ApplyThemeToControl(Control control)
        {
            if (control is Form form)
            {
                form.BackColor = GetBgColor();
                form.ForeColor = GetTextColor();
            }
            else if (control is Panel panel)
            {
                panel.BackColor = GetPanelColor();
                panel.ForeColor = GetTextColor();
            }
            else if (control is Label label)
            {
                label.ForeColor = GetTextColor();
            }
            else if (control is TextBox textBox)
            {
                textBox.BackColor = GetBgColor();
                textBox.ForeColor = GetTextColor();
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is DataGridView dgv)
            {
                dgv.BackgroundColor = GetBgColor();
                dgv.ForeColor = GetTextColor();
                dgv.GridColor = GetBorderColor();
                dgv.DefaultCellStyle.BackColor = GetBgColor();
                dgv.DefaultCellStyle.ForeColor = GetTextColor();
                dgv.DefaultCellStyle.SelectionBackColor = Colors.Primary;
                dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            else if (control is Button button)
            {
                button.BackColor = Colors.Primary;
                button.ForeColor = Color.White;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
            }

            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child);
            }
        }
    }

    public static class Colors
    {
        public static Color Primary = Color.FromArgb(52, 152, 219);
        public static Color PrimaryDark = Color.FromArgb(41, 128, 185);
        public static Color PrimaryLight = Color.FromArgb(133, 193, 233);
        
        public static Color LightBg = Color.White;
        public static Color LightText = Color.Black;
        public static Color LightPanel = Color.FromArgb(240, 242, 245);
        public static Color LightBorder = Color.FromArgb(200, 200, 200);
        
        public static Color DarkBg = Color.FromArgb(30, 30, 30);
        public static Color DarkText = Color.White;
        public static Color DarkPanel = Color.FromArgb(45, 45, 48);
        public static Color DarkBorder = Color.FromArgb(80, 80, 80);
        
        public static Color Success = Color.FromArgb(46, 204, 113);
        public static Color Warning = Color.FromArgb(241, 196, 15);
        public static Color Danger = Color.FromArgb(231, 76, 60);
        public static Color Info = Color.FromArgb(52, 152, 219);
    }
}
