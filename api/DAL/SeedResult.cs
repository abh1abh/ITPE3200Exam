namespace api.DAL
{
    // Class to hold the result of the seeding process
    public class SeedResult
    {
        public Dictionary<string, string> UserIds { get; set; } = new();
    }
}
