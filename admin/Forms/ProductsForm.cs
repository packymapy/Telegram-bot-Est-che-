using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AdminApp.Controls;
using AdminApp.Database;

namespace AdminApp.Forms
{
    public partial class ProductsForm : UserControl
    {
        private DataGridView dgvProducts;
        private TextBox txtSearch;
        private ComboBox cmbCategory;
        private Button btnSearch;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;

        public ProductsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ThemeManager.GetBgColor();

            var panelTop = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = ThemeManager.GetPanelColor(),
                Padding = new Padding(10)
            };

            txtSearch = new TextBox
            {
                Location = new Point(10, 15),
                Size = new Size(250, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor()
            };

            cmbCategory = new ComboBox
            {
                Location = new Point(270, 15),
                Size = new Size(150, 25),
                BackColor = ThemeManager.GetBgColor(),
                ForeColor = ThemeManager.GetTextColor()
            };
            cmbCategory.Items.Add("Все категории");
            cmbCategory.SelectedIndex = 0;

            btnSearch = new Button
            {
                Text = "🔍 Поиск",
                Location = new Point(430, 14),
                Size = new Size(90, 30),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSearch.Click += (s, e) => LoadData();

            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(530, 14),
                Size = new Size(90, 30),
                BackColor = Colors.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadData();

            panelTop.Controls.AddRange(new Control[] { txtSearch, cmbCategory, btnSearch, btnRefresh });

            var panelButtons = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                BackColor = ThemeManager.GetPanelColor(),
                Padding = new Padding(10)
            };

            btnAdd = new Button
            {
                Text = "➕ Добавить",
                Location = new Point(10, 8),
                Size = new Size(100, 35),
                BackColor = Colors.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnEdit = new Button
            {
                Text = "✏️ Редактировать",
                Location = new Point(120, 8),
                Size = new Size(100, 35),
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnDelete = new Button
            {
                Text = "🗑️ Удалить",
                Location = new Point(230, 8),
                Size = new Size(100, 35),
                BackColor = Colors.Danger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            dgvProducts = new DataGridView
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
                MultiSelect = false
            };

            this.Controls.Add(dgvProducts);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelTop);
        }

        private void LoadData()
        {
            string query = @"
                SELECT 
                    p.id,
                    p.name,
                    c.name as category,
                    b.name as brand,
                    p.price,
                    p.is_active
                FROM products p
                LEFT JOIN categories c ON c.id = p.category_id
                LEFT JOIN brands b ON b.id = p.brand_id
                WHERE p.is_active = true
                ORDER BY p.name";

            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                query = query.Replace("WHERE p.is_active = true", 
                    "WHERE p.is_active = true AND p.name ILIKE '%" + txtSearch.Text + "%'");
            }

            if (cmbCategory.SelectedIndex > 0)
            {
                query = query.Replace("WHERE p.is_active = true",
                    "WHERE p.is_active = true AND c.name = '" + cmbCategory.SelectedItem + "'");
            }

            var dt = DatabaseHelper.ExecuteQuery(query);
            dgvProducts.DataSource = dt;
            dgvProducts.Columns["id"].Visible = false;
        }
    }
}
