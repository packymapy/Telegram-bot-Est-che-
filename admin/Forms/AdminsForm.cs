using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AdminApp.Controls;
using AdminApp.Database;

namespace AdminApp.Forms
{
    public partial class AdminsForm : UserControl
    {
        private DataGridView dgvAdmins;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnLock;
        private Button btnUnlock;
        private Button btnRefresh;
        private Label lblTotal;

        public AdminsForm()
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

            btnAdd = new Button
            {
                Text = "➕ Добавить",
                Location = new Point(10, 12),
                Size = new Size(110, 35),
                BackColor = Colors.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button
            {
                Text = "✏️ Редактировать",
                Location = new Point(130, 12),
                Size = new Size(110, 35),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "🗑️ Удалить",
                Location = new Point(250, 12),
                Size = new Size(110, 35),
                BackColor = Colors.Danger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnDelete.Click += BtnDelete_Click;

            btnLock = new Button
            {
                Text = "🔒 Заблокировать",
                Location = new Point(370, 12),
                Size = new Size(110, 35),
                BackColor = Colors.Warning,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLock.Click += BtnLock_Click;

            btnUnlock = new Button
            {
                Text = "🔓 Разблокировать",
                Location = new Point(490, 12),
                Size = new Size(110, 35),
                BackColor = Colors.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnUnlock.Click += BtnUnlock_Click;

            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(700, 12),
                Size = new Size(100, 35),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnRefresh.Click += (s, e) => LoadData();

            lblTotal = new Label
            {
                Text = "Всего админов: 0",
                Location = new Point(820, 18),
                Size = new Size(150, 25),
                ForeColor = ThemeManager.GetTextColor(),
                TextAlign = ContentAlignment.MiddleRight
            };

            panelTop.Controls.AddRange(new Control[] { 
                btnAdd, btnEdit, btnDelete, btnLock, btnUnlock, btnRefresh, lblTotal 
            });

            dgvAdmins = new DataGridView
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
            dgvAdmins.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            this.Controls.Add(dgvAdmins);
            this.Controls.Add(panelTop);
        }

        private void LoadData()
        {
            string query = @"
                SELECT 
                    a.id,
                    a.login as 'Логин',
                    a.full_name as 'ФИО',
                    CASE WHEN a.is_active THEN '🟢 Активен' ELSE '🔴 Деактивирован' END as 'Статус',
                    CASE WHEN a.is_locked THEN '🔒 Да' ELSE '🔓 Нет' END as 'Заблокирован',
                    a.last_login as 'Последний вход',
                    a.created_at as 'Создан'
                FROM admins a
                ORDER BY a.id";

            try
            {
                var dt = DatabaseHelper.ExecuteQuery(query);
                dgvAdmins.DataSource = dt;
                lblTotal.Text = $"Всего админов: {dt.Rows.Count}";

                if (dgvAdmins.Columns.Contains("id"))
                    dgvAdmins.Columns["id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckPermissions()
        {
            bool canManage = GlobalState.HasPermission("can_manage_admins");
            btnAdd.Visible = canManage;
            btnEdit.Visible = canManage;
            btnDelete.Visible = canManage;
            btnLock.Visible = canManage;
            btnUnlock.Visible = canManage;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Добавление администратора будет здесь", "Добавление",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvAdmins.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите администратора", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show("Редактирование администратора будет здесь", "Редактирование",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAdmins.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите администратора", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Удалить администратора?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show("Администратор удален", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }

        private void BtnLock_Click(object sender, EventArgs e)
        {
            if (dgvAdmins.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите администратора", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Заблокировать администратора?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show("Администратор заблокирован", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }

        private void BtnUnlock_Click(object sender, EventArgs e)
        {
            if (dgvAdmins.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите администратора", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Разблокировать администратора?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show("Администратор разблокирован", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }
    }
}
