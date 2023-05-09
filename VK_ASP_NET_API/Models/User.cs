using System;

namespace VK_ASP_NET_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserGroupId { get; set; }
        public UserGroup UserGroup { get; set; }
        public int UserStateId { get; set; }
        public UserState UserState { get; set; }
    }
}
