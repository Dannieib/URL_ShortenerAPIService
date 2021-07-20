using System;
using System.Collections.Generic;
using System.Text;

namespace URLShortener.Models
{
    public class UrlShortenerDto
    {
        public string Message { get; set; }
        public UrlShortenerModel UrlShortener { get; set; }
    }
}
