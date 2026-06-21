using System;

namespace AdminApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public long TgId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool AgeVerified { get; set; }
        public bool AgreedToTerms { get; set; }
        public int VerificationAttempts { get; set; }
        public DateTime? BlockedUntil { get; set; }
        public string CityName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Status
        {
            get
            {
                if (BlockedUntil.HasValue && BlockedUntil.Value > DateTime.Now)
                    return "🔒 Заблокирован";
                if (!AgeVerified)
                    return "🔴 Не подтвержден";
                return "🟢 Активен";
            }
        }
    }
}
