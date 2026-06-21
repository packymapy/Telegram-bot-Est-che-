using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AdminApp.Controls;
using AdminApp.Database;

namespace AdminApp.Forms
{
    public partial class DashboardForm : UserControl
    {
        private Label lblTotalProducts;
        private Label lblActiveProducts;
        private Label lblTotalCategories;
        private Label lblTotalUsers;
        private Label lblTotalOrders;
        private Label lblTotalRevenue;
        private Label lblPendingOrders;
        private Label lblTotalViews;
        private Timer refreshTimer;
        private Label lblLastUpdate;

        public DashboardForm()
        {
            InitializeComponent();
            LoadStatistics();
            SetupAutoRefresh();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ThemeManager.GetBgColor();
            this.Padding = new Padding(20);

            var lblTitle = new Label
            {
                Text = "📊 Дашборд",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(0, 0),
                Size = new Size(200, 40),
                ForeColor = ThemeManager.GetTextColor()
            };

            lblLastUpdate = new Label
            {
                Text = "Обновлено: " + DateTime.Now.ToString("HH:mm:ss"),
                Location = new Point(0, 45),
                Size = new Size(200, 25),
                ForeColor = ThemeManager.GetTextColor(),
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            int cardWidth = 180;
            int cardHeight = 100;
            int spacing = 20;
            int startX = 0;
            int startY = 80;

            var stats = new (string label, string icon, Color color, ref Label control)[]
            {
                ("Товаров", "📦", Colors.Primary, ref lblTotalProducts),
                ("Активных", "✅", Colors.Success, ref lblActiveProducts),
                ("Категорий", "📂", Colors.Info, ref lblTotalCategories),
                ("Пользователей", "👤", Colors.Primary, ref lblTotalUsers),
                ("Заказов", "📋", Colors.Warning, ref lblTotalOrders),
                ("Выручка", "💰", Colors.Success, ref lblTotalRevenue),
                ("В обработке", "⏳", Colors.Danger, ref lblPendingOrders),
                ("Просмотров", "👁️", Colors.Info, ref lblTotalViews)
            };

            for (int i = 0; i < stats.Length; i++)
            {
                int row = i / 4;
                int col = i % 4;
                int x = startX + col * (cardWidth + spacing);
                int y = startY + row * (cardHeight + spacing);

                var panel = CreateStatCard(
                    stats[i].label,
                    stats[i].icon,
                    stats[i].color,
                    x, y, cardWidth, cardHeight,
                    ref stats[i].control
                );
                this.Controls.Add(panel);
            }

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblLastUpdate);

            refreshTimer = new Timer();
            refreshTimer.Interval = 60000;
            refreshTimer.Tick += (s, e) => LoadStatistics();
            refreshTimer.Start();
        }

        private Panel CreateStatCard(string label, string icon, Color color, int x, int y, int width, int height, ref Label valueLabel)
        {
            var panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = ThemeManager.GetPanelColor(),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 24),
                Location = new Point(10, 10),
                Size = new Size(50, 40),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleCenter
            };

            valueLabel = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(60, 10),
                Size = new Size(100, 35),
                ForeColor = ThemeManager.GetTextColor(),
                TextAlign = ContentAlignment.MiddleRight
            };

            var lblName = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10),
                Location = new Point(60, 50),
                Size = new Size(100, 25),
                ForeColor = ThemeManager.GetTextColor(),
                TextAlign = ContentAlignment.MiddleRight
            };

            panel.Controls.AddRange(new Control[] { lblIcon, valueLabel, lblName });
            return panel;
        }

        private void LoadStatistics()
        {
            try
            {
                string query = @"
                    SELECT 
                        (SELECT COUNT(*) FROM products) as total_products,
                        (SELECT COUNT(*) FROM products WHERE is_active = true) as active_products,
                        (SELECT COUNT(*) FROM categories) as total_categories,
                        (SELECT COUNT(*) FROM users) as total_users,
                        (SELECT COUNT(*) FROM orders) as total_orders,
                        COALESCE((SELECT SUM(total_price) FROM orders WHERE status != 'cancelled'), 0) as total_revenue,
                        (SELECT COUNT(*) FROM orders WHERE status = 'pending') as pending_orders,
                        (SELECT COUNT(*) FROM product_views) as total_views";

                var dt = DatabaseHelper.ExecuteQuery(query);
                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    lblTotalProducts.Text = row["total_products"]?.ToString() ?? "0";
                    lblActiveProducts.Text = row["active_products"]?.ToString() ?? "0";
                    lblTotalCategories.Text = row["total_categories"]?.ToString() ?? "0";
                    lblTotalUsers.Text = row["total_users"]?.ToString() ?? "0";
                    lblTotalOrders.Text = row["total_orders"]?.ToString() ?? "0";
                    
                    decimal revenue = Convert.ToDecimal(row["total_revenue"] ?? 0);
                    lblTotalRevenue.Text = revenue.ToString("N0") + " ₽";
                    
                    lblPendingOrders.Text = row["pending_orders"]?.ToString() ?? "0";
                    lblTotalViews.Text = row["total_views"]?.ToString() ?? "0";
                }

                lblLastUpdate.Text = "Обновлено: " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
            }
        }

        private void SetupAutoRefresh()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 60000;
            refreshTimer.Tick += (s, e) => LoadStatistics();
            refreshTimer.Start();
        }
    }
}
