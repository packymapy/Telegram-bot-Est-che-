using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AdminApp.Utils
{
    public static class Helpers
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
                
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return false;
                
            var digits = Regex.Replace(phone, @"[^\d]", "");
            return digits.Length >= 10 && digits.Length <= 15;
        }

        public static bool IsValidPrice(string price)
        {
            if (string.IsNullOrEmpty(price))
                return false;
                
            return decimal.TryParse(price, out _);
        }

        public static bool IsValidDate(string date)
        {
            if (string.IsNullOrEmpty(date))
                return false;
                
            return DateTime.TryParse(date, out _);
        }
        
        public static string FormatPrice(decimal price)
        {
            return price.ToString("N2") + " ₽";
        }

        public static string FormatDate(DateTime date)
        {
            return date.ToString("dd.MM.yyyy HH:mm");
        }

        public static string FormatShortDate(DateTime date)
        {
            return date.ToString("dd.MM.yyyy");
        }

        public static string FormatTime(DateTime date)
        {
            return date.ToString("HH:mm");
        }

        public static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
                
            return text.Substring(0, maxLength - 3) + "...";
        }

        public static string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }
        
        public static string GetStatusText(bool isActive)
        {
            return isActive ? "🟢 Активен" : "🔴 Неактивен";
        }

        public static string GetStatusText(bool isActive, bool isLocked)
        {
            if (!isActive) return "🔴 Деактивирован";
            if (isLocked) return "🟡 Заблокирован";
            return "🟢 Активен";
        }

        public static Color GetStatusColor(bool isActive)
        {
            return isActive ? Colors.Success : Colors.Danger;
        }

        public static Color GetStatusColor(bool isActive, bool isLocked)
        {
            if (!isActive) return Colors.Danger;
            if (isLocked) return Colors.Warning;
            return Colors.Success;
        }
        
        public static string GenerateRandomString(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(length);
            
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            
            return result.ToString();
        }

        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            var slug = text.ToLower().Trim();
            slug = Regex.Replace(slug, @"[^a-z0-9а-яё\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            
            return slug;
        }
        
        public static string EscapeSqlString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
                
            return value.Replace("'", "''");
        }

        public static string BuildLikeQuery(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return "%";
                
            return "%" + searchText.Trim() + "%";
        }
        
        public static void CenterControlOnParent(Control control)
        {
            if (control.Parent == null)
                return;
                
            control.Left = (control.Parent.Width - control.Width) / 2;
            control.Top = (control.Parent.Height - control.Height) / 2;
        }

        public static void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowSuccess(string message, string title = "Успешно")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowWarning(string message, string title = "Предупреждение")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static DialogResult ShowConfirm(string message, string title = "Подтверждение")
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public static bool CanManageProducts()
        {
            return GlobalState.HasPermission("can_manage_products");
        }

        public static bool CanManageCategories()
        {
            return GlobalState.HasPermission("can_manage_categories");
        }

        public static bool CanManageBrands()
        {
            return GlobalState.HasPermission("can_manage_brands");
        }

        public static bool CanManageUsers()
        {
            return GlobalState.HasPermission("can_manage_users");
        }

        public static bool CanManageAdmins()
        {
            return GlobalState.HasPermission("can_manage_admins");
        }

        public static bool CanViewLogs()
        {
            return GlobalState.HasPermission("can_view_logs");
        }

        public static bool CanManageContacts()
        {
            return GlobalState.HasPermission("can_manage_contacts");
        }
    }
}
