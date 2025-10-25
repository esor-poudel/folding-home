using System;

namespace DesktopClient
{
    public class JobItem
    {
        public required string Base64Job { get; set; }
        public required string Sha256Hash { get; set; }
    }
}