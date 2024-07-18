namespace internship.Core.Models
{
    internal class DbConnectionParams
    {
        public string ServerName { get; set; } 
        public string DatabaseName { get; set; }
        public string UserName { get; set; } 
        public string Password { get; set; } 

        public DbConnectionParams()
        {
            this.ServerName = string.Empty;
            this.DatabaseName = string.Empty;
            this.UserName = string.Empty;
            this.Password = string.Empty;
        }
    }
}
