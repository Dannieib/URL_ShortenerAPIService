using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.AppService.InMemoryService
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private IMemoryCache _cache;
        public static readonly string UrlShortenerKey = "UrlShortenerKey";
        public static readonly string UrlShortenerStat = "UrlShortenerStat";

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public async Task<List<T>> AddNewItem<T>(List<T> model, string key)
        {
            try
            {
                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(15),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromMinutes(10),
                    Size = 1024,
                };

                var resp = _cache.Set(key, model, cacheExpiryOptions);
                return await Task.FromResult(resp);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<T>> Get<T>(string key, List<T> model)
        {
            try
            {
                var get = _cache.TryGetValue(key, out model);
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
