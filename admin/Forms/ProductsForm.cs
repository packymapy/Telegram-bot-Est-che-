using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AdminApp.Controls;
using AdminApp.Database;
using AdminApp.Models;

namespace AdminApp.Forms
{
    public partial class ProductsForm : UserControl
    {
        private DataGridView dgvProducts;
        private TextBox txtSearch;
        private ComboBox cmbCategory;
        private ComboBox cmbBrand;
        private Button btnSearch;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Label lblTotal;

        public ProductsForm()
        {
            InitializeComponent();
            LoadCategories();
            LoadBrands();
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

            // Поиск
            var lblSearch = new Label
            {
                Text = "🔍 Поиск:",
                Location = new Point(10, 25),
                Size = new Size(60, 25),
                ForeColor = ThemeManager.GetTextColor()
            };

            txtSearch = new TextBox
            {
                Location = new Point(75, 22),
                Size = new Size(200, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => LoadData();

            var lblCategory = new Label
            {
                Text = "Категория:",
                Location = new Point(290, 25),
                Size = new Size(70, 25),
                ForeColor = ThemeManager.GetTextColor()
            };

            cmbCategory = new ComboBox
            {
                Location = new Point(365, 22),
                Size = new Size(150, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.SelectedIndexChanged += (s, e) => LoadData();

            var lblBrand = new Label
            {
                Text = "Бренд:",
                Location = new Point(530, 25),
                Size = new Size(60, 25),
                ForeColor = ThemeManager.GetTextColor()
            };

            cmbBrand = new ComboBox
            {
                Location = new Point(595, 22),
                Size = new Size(150, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor(),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbBrand.SelectedIndexChanged += (s, e) => LoadData();

            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(760, 20),
                Size = new Size(100, 30),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => { LoadCategories(); LoadBrands(); LoadData(); };

            panelTop.Controls.AddRange(new Control[] { 
                lblSearch, txtSearch, lblCategory, cmbCategory, 
                lblBrand, cmbBrand, btnRefresh 
            });

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
                Text = "Всего товаров: 0",
                Location = new Point(800, 18),
                Size = new Size(150, 25),
                ForeColor = ThemeManager.GetTextColor(),
                TextAlign = ContentAlignment.MiddleRight
            };

            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, lblTotal });

            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
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
            dgvProducts.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            this.Controls.Add(dgvProducts);
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

        private void LoadBrands()
        {
            cmbBrand.Items.Clear();
            cmbBrand.Items.Add("Все бренды");

            string query = "SELECT name FROM brands ORDER BY name";
            var dt = DatabaseHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                cmbBrand.Items.Add(row["name"].ToString());
            }
            cmbBrand.SelectedIndex = 0;
        }

        private void LoadData()
        {
            string query = @"
                SELECT 
                    p.id,
                    p.name as 'Название',
                    c.name as 'Категория',
                    b.name as 'Бренд',
                    p.price as 'Цена',
                    CASE WHEN p.is_active THEN '✅ Да' ELSE '❌ Нет' END as 'Активен',
                    p.updated_at as 'Обновлен'
                FROM products p
                LEFT JOIN categories c ON c.id = p.category_id
                LEFT JOIN brands b ON b.id = p.brand_id
                WHERE 1=1";

            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                query += $" AND p.name ILIKE '%{txtSearch.Text}%'";
            }

            if (cmbCategory.SelectedIndex > 0)
            {
                query += $" AND c.name = '{cmbCategory.SelectedItem}'";
            }

            if (cmbBrand.SelectedIndex > 0)
            {
                query += $" AND b.name = '{cmbBrand.SelectedItem}'";
            }

            query += " ORDER BY p.name";

            try
            {
                var dt = DatabaseHelper.ExecuteQuery(query);
                dgvProducts.DataSource = dt;
                lblTotal.Text = $"Всего товаров: {dt.Rows.Count}";

                if (dgvProducts.Columns.Contains("id"))
                    dgvProducts.Columns["id"].Visible = false;

                foreach (DataGridViewColumn col in dgvProducts.Columns)
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckPermissions()
        {
            bool canManage = GlobalState.HasPermission("can_manage_products");
            btnAdd.Visible = canManage;
            btnEdit.Visible = canManage;
            btnDelete.Visible = canManage;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Добавление товара будет здесь", "Добавление", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар для редактирования", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["id"].Value);
            MessageBox.Show($"Редактирование товара ID: {productId}", "Редактирование",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар для удаления", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string productName = dgvProducts.SelectedRows[0].Cells["Название"].Value.ToString();
            var result = MessageBox.Show($"Удалить товар '{productName}'?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int productId = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["id"].Value);
                MessageBox.Show($"Товар '{productName}' удален", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }
    }
}
