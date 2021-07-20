using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrlShortener.AppService.InMemoryService;
using URLShortener.Models;

namespace UrlShortener.AppService.UrlAppService
{
    public class UrlManager : IUrlManager
    {
        private readonly IMemoryCacheService _memoryCacheService;
        private string UrlKey = "UrlKey";
        public UrlManager(IMemoryCacheService memoryCacheService)
        {
            _memoryCacheService = memoryCacheService;
        }

        public async Task<UrlShortenerModel> ShortenUrl(string longUrl, string baseUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(longUrl))
                {
                    return null;
                }

                UrlShortenerModel shortUrl = new UrlShortenerModel();
                List<UrlShortenerModel> retrieved = new List<UrlShortenerModel>();
                // get all existing records from in-memory
                var getAllRecords = await _memoryCacheService.Get<UrlShortenerModel>(UrlKey, new List<UrlShortenerModel>());

                shortUrl.Id = 1;
                var urlChunk = new ShortLink().GetUrlChunk(shortUrl.Id);
                if (getAllRecords !=null)
                {
                    //pretend it's an auto-incremental key for identity..
                    shortUrl.Id = 0;
                    shortUrl.Id = getAllRecords.Last().Id + 1;

                    var find = getAllRecords.Where(x => x.Segment.Contains(urlChunk));
                    if (find.Any())
                    {
                        urlChunk = new ShortLink().GetUrlChunk(shortUrl.Id);
                    }
                    retrieved = getAllRecords;
                }
                else
                {
                    retrieved = new List<UrlShortenerModel>();
                }
              
                var encodedUrl = $"{baseUrl}{urlChunk}";

                var model = new UrlShortenerModel
                {
                    Id = shortUrl.Id,
                    OriginalUrl = longUrl,
                    Added = DateTime.Now,
                    EncodedUrl = encodedUrl,
                    Segment = urlChunk,
                    IsActive = true
                };

                retrieved.Add(model);
                //save to in-memory
                await _memoryCacheService.AddNewItem<UrlShortenerModel>(retrieved, UrlKey);

                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }               
        }

        public async Task<UrlShortenerDto> HandleUrl(string shortenedUrl, string host, string scheme)
        {
            try
            {
                HttpContext context = null;
                var getAllRecords = await _memoryCacheService.Get<UrlShortenerModel>(UrlKey, new List<UrlShortenerModel>()) != null ?
                  await _memoryCacheService.Get<UrlShortenerModel>(UrlKey, new List<UrlShortenerModel>()) :
                  new List<UrlShortenerModel>();

                var findUrlById = new UrlShortenerModel();
                string message = string.Empty;

                Uri urlResult = null;
                var isValid = Uri.TryCreate(shortenedUrl, UriKind.Absolute, out urlResult);

                if (isValid && urlResult.Scheme == scheme && $"{urlResult.Host}:{urlResult.Port}" == host.ToString())
                {
                    var path = urlResult.AbsolutePath;
                    path = path.Replace("/", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        //decode chunk here to get Actual Id
                        var urlChunk = new ShortLink().GetId(path);

                        findUrlById = getAllRecords.FirstOrDefault(url => url.Id == urlChunk);
                        if (findUrlById != null && findUrlById.IsActive)
                        {
                            getAllRecords = RemoveAndUpdateRecord(getAllRecords, findUrlById);
                            findUrlById.NumOfClicks = findUrlById.NumOfClicks + 1;
                            if(findUrlById.NumOfClicks >= 5)
                            {
                                findUrlById.IsActive = false;
                                message = "Url is inactive. It has exceeded it usage count.";
                            }
                            getAllRecords.Add(findUrlById);
                            return new UrlShortenerDto 
                            {
                                UrlShortener = findUrlById,
                                Message = null
                            };
                        }
                        else
                        {
                            message = "No such url exist with this shortlink";
                        }
                    }
                    else
                    {
                        message = "Url path does not exist.";
                    }
                }
                else
                {
                    message = "Url is not valid.";
                }
                return new UrlShortenerDto
                {
                    UrlShortener = null,
                    Message = !string.IsNullOrEmpty(message) ? message : "An error occurred!"
                };
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<UrlShortenerModel> HandleUrlStat(string path)
        {
            try
            {
                var getAllRecords = await _memoryCacheService.Get<UrlShortenerModel>(UrlKey, new List<UrlShortenerModel>()) != null ?
                  await _memoryCacheService.Get<UrlShortenerModel>(UrlKey, new List<UrlShortenerModel>()) :
                  new List<UrlShortenerModel>();

                var findUrlById = new UrlShortenerModel();
                string message = string.Empty;                 
                    
                if (!string.IsNullOrEmpty(path))
                {
                    //decode chunk here to get Actual Id
                    var urlChunk = new ShortLink().GetId(path);

                    findUrlById = getAllRecords.FirstOrDefault(url => url.Id == urlChunk);
                    if (findUrlById != null)
                    {
                        return findUrlById;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<UrlShortenerModel> RemoveAndUpdateRecord(List<UrlShortenerModel> records, UrlShortenerModel recordToBeUpdated)
        {
            records.Remove(recordToBeUpdated);
            return records;
        }



    }

    public class ShortLink
    {
        public string GetUrlChunk(int id)
        {
            return WebEncoders.Base64UrlEncode(BitConverter.GetBytes(id));
        }

        public int GetId(string urlChunk)
        {
            return BitConverter.ToInt32(WebEncoders.Base64UrlDecode(urlChunk));
        }
    }

}
