namespace TimeLogAuthPOC.Entity
{
    public class UserProjectRole
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public string RoleId { get; set; }
        public Role Role { get; set; }

        public string ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
