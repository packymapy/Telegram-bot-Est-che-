using System;
using System.Drawing;
using System.Windows.Forms;
using AdminApp.Controls;
using AdminApp.Database;

namespace AdminApp.Forms
{
    public partial class LoginForm : Form
    {
        private TextBox txtLogin;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCancel;
        private Label lblError;
        private Label lblTitle;
        private CheckBox chkShowPassword;
        private Label lblVersion;

        public LoginForm()
        {
            InitializeComponent();
            ThemeManager.ApplyTheme(this);
        }

        private void InitializeComponent()
        {
            this.Text = "Вход в систему";
            this.Size = new Size(400, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeManager.GetBgColor();
            lblTitle = new Label
            {
                Text = "Администрирование",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(30, 30),
                Size = new Size(340, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = ThemeManager.GetTextColor()
            };
            
            var lblLogin = new Label
            {
                Text = "Логин:",
                Location = new Point(40, 100),
                Size = new Size(80, 25),
                ForeColor = ThemeManager.GetTextColor()
            };

            txtLogin = new TextBox
            {
                Location = new Point(130, 97),
                Size = new Size(200, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(40, 140),
                Size = new Size(80, 25),
                ForeColor = ThemeManager.GetTextColor()
            };

            txtPassword = new TextBox
            {
                Location = new Point(130, 137),
                Size = new Size(200, 25),
                UseSystemPasswordChar = true,
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtPassword.KeyPress += (s, e) => {
                if (e.KeyChar == (char)Keys.Enter) btnLogin.PerformClick();
            };

            chkShowPassword = new CheckBox
            {
                Text = "Показать пароль",
                Location = new Point(130, 168),
                Size = new Size(130, 25),
                ForeColor = ThemeManager.GetTextColor()
            };
            chkShowPassword.CheckedChanged += (s, e) => {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };

            lblError = new Label
            {
                ForeColor = Color.Red,
                Location = new Point(40, 200),
                Size = new Size(320, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(100, 240),
                Size = new Size(90, 35),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLogin.Click += BtnLogin_Click;

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(210, 240),
                Size = new Size(90, 35),
                BackColor = Colors.Danger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            lblVersion = new Label
            {
                Text = "v1.0.0",
                Location = new Point(10, 290),
                Size = new Size(60, 20),
                ForeColor = ThemeManager.GetTextColor(),
                Font = new Font("Segoe UI", 8, FontStyle.Italic)
            };

            this.Controls.AddRange(new Control[] {
                lblTitle, lblLogin, txtLogin, lblPassword, txtPassword,
                chkShowPassword, lblError, btnLogin, btnCancel, lblVersion
            });
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLogin.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                lblError.Text = "Введите логин и пароль!";
                lblError.Visible = true;
                return;
            }

            var admin = DatabaseHelper.AuthenticateAdmin(txtLogin.Text, txtPassword.Text);

            if (admin != null)
            {
                GlobalState.CurrentAdmin = admin;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblError.Text = "Неверный логин или пароль!";
                lblError.Visible = true;
                txtPassword.Text = "";
                txtPassword.Focus();
            }
        }
    }

    public static class GlobalState
    {
        public static AdminInfo CurrentAdmin { get; set; }
        public static bool IsLoggedIn => CurrentAdmin != null;
        public static bool HasPermission(string permission) => CurrentAdmin?.HasPermission(permission) ?? false;
    }
}
