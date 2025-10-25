namespace DesktopClient
{
    public class JobTransferDto
    {
        public required string Base64Job { get; set; }
        public required string Sha256Hash { get; set; }
    }
}