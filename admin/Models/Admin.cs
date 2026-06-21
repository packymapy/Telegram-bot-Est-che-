using System;
using Newtonsoft.Json;

namespace AdminApp.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public dynamic Permissions { get; set; }

        public bool HasPermission(string permission)
        {
            if (Permissions == null) return false;
            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(
                    Permissions.ToString()
                );
                return dict.ContainsKey(permission) && dict[permission];
            }
            catch
            {
                return false;
            }
        }

        public string StatusText
        {
            get
            {
                if (!IsActive) return "🔴 Деактивирован";
                if (IsLocked) return "🟡 Заблокирован";
                return "🟢 Активен";
            }
        }
    }
}
