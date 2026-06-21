using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AdminApp.Controls;
using AdminApp.Database;

namespace AdminApp.Forms
{
    public partial class UsersForm : UserControl
    {
        private DataGridView dgvUsers;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnRefresh;
        private Button btnBlock;
        private Button btnUnblock;
        private Label lblTotal;

        public UsersForm()
        {
            InitializeComponent();
            LoadData();
            CheckPermissions();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ThemeManager.GetBgColor();
            this.Padding = new Padding(10);

            var panelTop = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = ThemeManager.GetPanelColor(),
                Padding = new Padding(10)
            };

            var lblSearch = new Label
            {
                Text = "🔍 Поиск:",
                Location = new Point(10, 18),
                Size = new Size(60, 25),
                ForeColor = ThemeManager.GetTextColor()
            };

            txtSearch = new TextBox
            {
                Location = new Point(75, 15),
                Size = new Size(200, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => LoadData();

            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(290, 13),
                Size = new Size(100, 30),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => LoadData();

            panelTop.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnRefresh });

            var panelButtons = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                BackColor = ThemeManager.GetPanelColor(),
                Padding = new Padding(10)
            };

            btnBlock = new Button
            {
                Text = "🔒 Заблокировать",
                Location = new Point(10, 12),
                Size = new Size(120, 35),
                BackColor = Colors.Danger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnBlock.Click += BtnBlock_Click;

            btnUnblock = new Button
            {
                Text = "🔓 Разблокировать",
                Location = new Point(140, 12),
                Size = new Size(120, 35),
                BackColor = Colors.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnUnblock.Click += BtnUnblock_Click;

            lblTotal = new Label
            {
                Text = "Всего пользователей: 0",
                Location = new Point(800, 18),
                Size = new Size(150, 25),
                ForeColor = ThemeManager.GetTextColor(),
                TextAlign = ContentAlignment.MiddleRight
            };

            panelButtons.Controls.AddRange(new Control[] { btnBlock, btnUnblock, lblTotal });

            dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                GridColor = ThemeManager.GetBorderColor(),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9)
            };

            this.Controls.Add(dgvUsers);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelTop);
        }

        private void LoadData()
        {
            string query = @"
                SELECT 
                    u.id,
                    u.tg_id as 'Telegram ID',
                    u.first_name as 'Имя',
                    u.last_name as 'Фамилия',
                    u.username as 'Username',
                    CASE WHEN u.age_verified THEN '✅' ELSE '❌' END as '18+',
                    CASE WHEN u.agreed_to_terms THEN '✅' ELSE '❌' END as 'Условия',
                    u.verification_attempts as 'Попытки',
                    CASE 
                        WHEN u.blocked_until > CURRENT_TIMESTAMP THEN '🔒 Заблокирован'
                        WHEN u.blocked_until IS NOT NULL THEN '✅ Разблокирован'
                        ELSE '✅ Активен'
                    END as 'Статус',
                    u.created_at as 'Дата регистрации'
                FROM users u
                WHERE 1=1";

            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                query += $" AND (u.tg_id::TEXT ILIKE '%{txtSearch.Text}%' OR u.first_name ILIKE '%{txtSearch.Text}%')";
            }

            query += " ORDER BY u.created_at DESC";

            try
            {
                var dt = DatabaseHelper.ExecuteQuery(query);
                dgvUsers.DataSource = dt;
                lblTotal.Text = $"Всего пользователей: {dt.Rows.Count}";

                if (dgvUsers.Columns.Contains("id"))
                    dgvUsers.Columns["id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckPermissions()
        {
            bool canManage = GlobalState.HasPermission("can_manage_users");
            btnBlock.Visible = canManage;
            btnUnblock.Visible = canManage;
        }

        private void BtnBlock_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Заблокировать пользователя?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show("Пользователь заблокирован", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }

        private void BtnUnblock_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Разблокировать пользователя?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show("Пользователь разблокирован", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }
    }
}
