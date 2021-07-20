using System;

namespace URLShortener.Models
{
    public class UrlShortenerModel
    {
        public string OriginalUrl { get; set; }
        public string EncodedUrl { get; set; }
        public int Id { get; set; }
        public int  NumOfClicks { get; set; }
        public DateTime Added { get; set; }
        public string Segment { get; set; }
        public bool IsActive { get; set; }
    }
}
