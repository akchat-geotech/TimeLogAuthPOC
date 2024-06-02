namespace TimeLogAuthPOC.Entity
{
    public class Project
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public IList<UserProjectRole> UserProjectRoles { get; set; }
    }
}
