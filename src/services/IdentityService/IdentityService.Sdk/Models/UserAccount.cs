namespace IdentityService.Sdk.Models
{
    public class UserAccount
    {
        public UserAccount(Guid userId, string name, string email, 
                           bool? enabled = true, 
                           long? createdTimestamp = null, 
                           List<string>? realmRoles = null, 
                           List<string>? groups = null)
        {
            this.userId = userId;
            this.name = name;
            this.email = email;
            this.enabled = enabled ?? true;
            this.createdTimestamp = createdTimestamp;
            this.realmRoles = realmRoles ?? new List<string>();
            this.groups = groups ?? new List<string>();
        }

        public Guid userId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public bool enabled { get; set; }
        public long? createdTimestamp { get; set; }
        public List<string> realmRoles { get; set; }
        public List<string> groups { get; set; }

        // Helper property til at konvertere timestamp
        public DateTime CreatedAt => createdTimestamp.HasValue 
            ? DateTimeOffset.FromUnixTimeMilliseconds(createdTimestamp.Value).DateTime 
            : DateTime.Now;
    }
}
