using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdminApp.Controls
{
    public class ModernTextBox : TextBox
    {
        private Color _borderColor = Colors.Primary;
        private Color _hoverColor = Colors.PrimaryLight;
        private Color _focusColor = Colors.Primary;
        private int _borderSize = 1;
        private bool _isHovered = false;
        private bool _isFocused = false;

        public ModernTextBox()
        {
            this.BorderStyle = BorderStyle.None;
            this.BackColor = ThemeManager.GetBgColor();
            this.ForeColor = ThemeManager.GetTextColor();
            this.Font = new Font("Segoe UI", 10);
            this.Padding = new Padding(5, 5, 5, 5);
            this.Height = 30;
            
            _borderColor = Colors.Primary;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            using (var pen = new Pen(_borderColor, _borderSize))
            {
                var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            _isFocused = true;
            _borderColor = _focusColor;
            _borderSize = 2;
            this.Invalidate();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            _isFocused = false;
            _borderColor = _isHovered ? _hoverColor : Colors.Primary;
            _borderSize = 1;
            this.Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            if (!_isFocused)
            {
                _borderColor = _hoverColor;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            if (!_isFocused)
            {
                _borderColor = Colors.Primary;
                this.Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Height = 30;
        }

        public void ApplyTheme()
        {
            this.BackColor = ThemeManager.GetBgColor();
            this.ForeColor = ThemeManager.GetTextColor();
            this.Invalidate();
        }

        public void SetErrorState()
        {
            _borderColor = Colors.Danger;
            _borderSize = 2;
            this.Invalidate();
        }

        public void SetSuccessState()
        {
            _borderColor = Colors.Success;
            _borderSize = 2;
            this.Invalidate();
        }

        public void ResetState()
        {
            _borderColor = Colors.Primary;
            _borderSize = 1;
            this.Invalidate();
        }
    }
}
