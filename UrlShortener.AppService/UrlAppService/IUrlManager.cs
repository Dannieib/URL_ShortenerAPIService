using System.Threading.Tasks;
using URLShortener.Models;

namespace UrlShortener.AppService.UrlAppService
{
    public interface IUrlManager
    {
        Task<UrlShortenerModel> ShortenUrl(string longUrl, string baseUrl);
        Task<UrlShortenerDto> HandleUrl(string shortenedUrl, string host, string scheme);
        Task<UrlShortenerModel> HandleUrlStat(string path);
    }
}