using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AdminApp.Controls;
using AdminApp.Database;

namespace AdminApp.Forms
{
    public partial class BrandsForm : UserControl
    {
        private DataGridView dgvBrands;
        private ComboBox cmbCategory;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Label lblTotal;

        public BrandsForm()
        {
            InitializeComponent();
            LoadCategories();
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

            var lblCategory = new Label
            {
                Text = "Категория:",
                Location = new Point(10, 18),
                Size = new Size(70, 25),
                ForeColor = ThemeManager.GetTextColor()
            };

            cmbCategory = new ComboBox
            {
                Location = new Point(85, 15),
                Size = new Size(150, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.SelectedIndexChanged += (s, e) => LoadData();

            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(250, 13),
                Size = new Size(100, 30),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => { LoadCategories(); LoadData(); };

            panelTop.Controls.AddRange(new Control[] { lblCategory, cmbCategory, btnRefresh });

            var panelButtons = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
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

            lblTotal = new Label
            {
                Text = "Всего брендов: 0",
                Location = new Point(800, 18),
                Size = new Size(150, 25),
                ForeColor = ThemeManager.GetTextColor(),
                TextAlign = ContentAlignment.MiddleRight
            };

            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, lblTotal });

            dgvBrands = new DataGridView
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
            dgvBrands.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            this.Controls.Add(dgvBrands);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelTop);
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("Все категории");

            string query = "SELECT name FROM categories ORDER BY name";
            var dt = DatabaseHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                cmbCategory.Items.Add(row["name"].ToString());
            }
            cmbCategory.SelectedIndex = 0;
        }

        private void LoadData()
        {
            string query = @"
                SELECT 
                    b.id,
                    b.name as 'Название',
                    c.name as 'Категория',
                    COUNT(p.id) as 'Товаров'
                FROM brands b
                LEFT JOIN categories c ON c.id = b.category_id
                LEFT JOIN products p ON p.brand_id = b.id AND p.is_active = true
                WHERE 1=1";

            if (cmbCategory.SelectedIndex > 0)
            {
                query += $" AND c.name = '{cmbCategory.SelectedItem}'";
            }

            query += " GROUP BY b.id, b.name, c.name ORDER BY b.name";

            try
            {
                var dt = DatabaseHelper.ExecuteQuery(query);
                dgvBrands.DataSource = dt;
                lblTotal.Text = $"Всего брендов: {dt.Rows.Count}";

                if (dgvBrands.Columns.Contains("id"))
                    dgvBrands.Columns["id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckPermissions()
        {
            bool canManage = GlobalState.HasPermission("can_manage_brands");
            btnAdd.Visible = canManage;
            btnEdit.Visible = canManage;
            btnDelete.Visible = canManage;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Добавление бренда будет здесь", "Добавление",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvBrands.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите бренд", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show("Редактирование бренда будет здесь", "Редактирование",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBrands.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите бренд", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Удалить бренд?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show("Бренд удален", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }
    }
}
