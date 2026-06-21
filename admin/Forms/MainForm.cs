using System;
using System.Drawing;
using System.Windows.Forms;
using AdminApp.Controls;

namespace AdminApp.Forms
{
    public partial class MainForm : Form
    {
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem themeMenu;
        private ToolStripMenuItem lightTheme;
        private ToolStripMenuItem darkTheme;
        private ToolStripMenuItem exitMenu;

        private TabControl tabControl;
        private TabPage tabDashboard;
        private TabPage tabProducts;
        private TabPage tabCategories;
        private TabPage tabBrands;
        private TabPage tabUsers;
        private TabPage tabAdmins;
        private TabPage tabLogs;

        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel themeStatus;

        public MainForm()
        {
            InitializeComponent();
            ThemeManager.ApplyTheme(this);
            UpdateStatus();
            CheckPermissions();
        }

        private void InitializeComponent()
        {
            this.Text = "Администрирование магазина";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = ThemeManager.GetBgColor();
            menuStrip = new MenuStrip
            {
                BackColor = ThemeManager.GetPanelColor(),
                ForeColor = ThemeManager.GetTextColor()
            };

            fileMenu = new ToolStripMenuItem("Файл");
            
            themeMenu = new ToolStripMenuItem("Тема");
            lightTheme = new ToolStripMenuItem("Светлая");
            darkTheme = new ToolStripMenuItem("Темная");
            lightTheme.Click += (s, e) => ChangeTheme(ThemeManager.ThemeType.Light);
            darkTheme.Click += (s, e) => ChangeTheme(ThemeManager.ThemeType.Dark);
            themeMenu.DropDownItems.AddRange(new ToolStripItem[] { lightTheme, darkTheme });

            exitMenu = new ToolStripMenuItem("Выйти", null, (s, e) => Application.Exit());

            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { themeMenu, new ToolStripSeparator(), exitMenu });
            menuStrip.Items.Add(fileMenu);

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.GetBgColor()
            };

            tabDashboard = new TabPage("📊 Дашборд");
            tabDashboard.BackColor = ThemeManager.GetBgColor();

            tabProducts = new TabPage("📦 Товары");
            tabProducts.BackColor = ThemeManager.GetBgColor();

            tabCategories = new TabPage("📂 Категории");
            tabCategories.BackColor = ThemeManager.GetBgColor();

            tabBrands = new TabPage("🏷️ Бренды");
            tabBrands.BackColor = ThemeManager.GetBgColor();

            tabUsers = new TabPage("👤 Пользователи");
            tabUsers.BackColor = ThemeManager.GetBgColor();

            tabAdmins = new TabPage("👑 Администраторы");
            tabAdmins.BackColor = ThemeManager.GetBgColor();

            tabLogs = new TabPage("📋 Логи");
            tabLogs.BackColor = ThemeManager.GetBgColor();

            tabControl.TabPages.AddRange(new TabPage[] {
                tabDashboard, tabProducts, tabCategories, tabBrands,
                tabUsers, tabAdmins, tabLogs
            });

            statusStrip = new StatusStrip
            {
                BackColor = ThemeManager.GetPanelColor(),
                ForeColor = ThemeManager.GetTextColor()
            };

            statusLabel = new ToolStripStatusLabel
            {
                Text = "Готов к работе",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            themeStatus = new ToolStripStatusLabel
            {
                Text = "Тема: Светлая",
                TextAlign = ContentAlignment.MiddleRight
            };

            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, themeStatus });

            this.Controls.Add(tabControl);
            this.Controls.Add(menuStrip);
            this.Controls.Add(statusStrip);

            this.MainMenuStrip = menuStrip;
        }

        private void ChangeTheme(ThemeManager.ThemeType theme)
        {
            ThemeManager.CurrentTheme = theme;
            ThemeManager.ApplyTheme(this);
            UpdateStatus();
            tabControl.BackColor = ThemeManager.GetBgColor();
            
            foreach (TabPage page in tabControl.TabPages)
                page.BackColor = ThemeManager.GetBgColor();
        }

        private void UpdateStatus()
        {
            if (GlobalState.CurrentAdmin != null)
            {
                statusLabel.Text = $"👤 {GlobalState.CurrentAdmin.FullName} | " +
                                  $"Роль: {(GlobalState.CurrentAdmin.IsAdmin() ? "Администратор" : "Продавец")}";
            }
            themeStatus.Text = $"Тема: {(ThemeManager.CurrentTheme == ThemeManager.ThemeType.Light ? "Светлая" : "Темная")}";
        }

        private void CheckPermissions()
        {
            var admin = GlobalState.CurrentAdmin;
            if (admin == null) return;

            tabAdmins.Visible = admin.HasPermission("can_manage_admins") || admin.HasPermission("can_view_admins");
            tabUsers.Visible = admin.HasPermission("can_manage_users") || admin.HasPermission("can_view_users");
            tabLogs.Visible = admin.HasPermission("can_view_logs");
        }
    }
}
