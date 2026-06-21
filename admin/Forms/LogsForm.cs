using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AdminApp.Controls;
using AdminApp.Database;

namespace AdminApp.Forms
{
    public partial class LogsForm : UserControl
    {
        private DataGridView dgvLogs;
        private ComboBox cmbAction;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Button btnSearch;
        private Button btnRefresh;
        private Label lblTotal;

        public LogsForm()
        {
            InitializeComponent();
            LoadActions();
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
                Height = 70,
                Dock = DockStyle.Top,
                BackColor = ThemeManager.GetPanelColor(),
                Padding = new Padding(10)
            };

            var lblAction = new Label
            {
                Text = "Действие:",
                Location = new Point(10, 25),
                Size = new Size(70, 25),
                ForeColor = ThemeManager.GetTextColor()
            };

            cmbAction = new ComboBox
            {
                Location = new Point(85, 22),
                Size = new Size(120, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblFrom = new Label
            {
                Text = "С:",
                Location = new Point(220, 25),
                Size = new Size(25, 25),
                ForeColor = ThemeManager.GetTextColor()
            };

            dtpFrom = new DateTimePicker
            {
                Location = new Point(250, 22),
                Size = new Size(150, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm"
            };
            dtpFrom.Value = DateTime.Now.AddDays(-7);

            var lblTo = new Label
            {
                Text = "По:",
                Location = new Point(410, 25),
                Size = new Size(25, 25),
                ForeColor = ThemeManager.GetTextColor()
            };

            dtpTo = new DateTimePicker
            {
                Location = new Point(440, 22),
                Size = new Size(150, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm"
            };
            dtpTo.Value = DateTime.Now;

            btnSearch = new Button
            {
                Text = "🔍 Поиск",
                Location = new Point(600, 20),
                Size = new Size(90, 30),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.Click += (s, e) => LoadData();

            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(700, 20),
                Size = new Size(100, 30),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => { LoadActions(); LoadData(); };

            panelTop.Controls.AddRange(new Control[] { 
                lblAction, cmbAction, lblFrom, dtpFrom, lblTo, dtpTo, btnSearch, btnRefresh 
            });

            var panelBottom = new Panel
            {
                Height = 40,
                Dock = DockStyle.Bottom,
                BackColor = ThemeManager.GetPanelColor(),
                Padding = new Padding(10)
            };

            lblTotal = new Label
            {
                Text = "Всего записей: 0",
                Location = new Point(10, 8),
                Size = new Size(200, 25),
                ForeColor = ThemeManager.GetTextColor(),
                TextAlign = ContentAlignment.MiddleLeft
            };

            panelBottom.Controls.Add(lblTotal);

            dgvLogs = new DataGridView
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

            this.Controls.Add(dgvLogs);
            this.Controls.Add(panelBottom);
            this.Controls.Add(panelTop);
        }

        private void LoadActions()
        {
            cmbAction.Items.Clear();
            cmbAction.Items.Add("Все действия");
            cmbAction.Items.Add("insert");
            cmbAction.Items.Add("update");
            cmbAction.Items.Add("delete");
            cmbAction.SelectedIndex = 0;
        }

        private void LoadData()
        {
            string query = @"
                SELECT 
                    l.id,
                    p.name as 'Товар',
                    l.action as 'Действие',
                    l.old_data as 'Старые данные',
                    l.new_data as 'Новые данные',
                    l.changed_at as 'Время'
                FROM products_log l
                JOIN products p ON p.id = l.product_id
                WHERE l.changed_at BETWEEN @from AND @to";

            if (cmbAction.SelectedIndex > 0)
            {
                query += " AND l.action = @action";
            }

            query += " ORDER BY l.changed_at DESC LIMIT 500";

            try
            {
                var parameters = new Npgsql.NpgsqlParameter[]
                {
                    new Npgsql.NpgsqlParameter("@from", dtpFrom.Value),
                    new Npgsql.NpgsqlParameter("@to", dtpTo.Value)
                };

                var dt = DatabaseHelper.ExecuteQuery(query, parameters);
                dgvLogs.DataSource = dt;
                lblTotal.Text = $"Всего записей: {dt.Rows.Count}";

                if (dgvLogs.Columns.Contains("id"))
                    dgvLogs.Columns["id"].Visible = false;

                if (dgvLogs.Columns.Contains("old_data"))
                    dgvLogs.Columns["old_data"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (dgvLogs.Columns.Contains("new_data"))
                    dgvLogs.Columns["new_data"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckPermissions()
        {
            if (!GlobalState.HasPermission("can_view_logs"))
            {
                this.Visible = false;
            }
        }
    }
}
