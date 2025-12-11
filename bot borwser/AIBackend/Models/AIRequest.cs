namespace AIBackend.Models
{
    public class AiRequest
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public string Message { get; set; } = "";
        public string SnapshotString { get; set; } = "";

    }
}
