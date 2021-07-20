using System.Collections.Generic;
using System.Threading.Tasks;

namespace UrlShortener.AppService.InMemoryService
{
    public interface IMemoryCacheService
    {
        Task<List<T>> AddNewItem<T>(List<T> model, string key);
        Task<List<T>> Get<T>(string key, List<T> model);
    }
}