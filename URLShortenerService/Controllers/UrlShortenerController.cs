using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using UrlShortener.AppService.InMemoryService;
using UrlShortener.AppService.UrlAppService;
using URLShortener.Models;

namespace URLShortener.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlShortenerController : ControllerBase
    {
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IUrlManager _urlManager;
        private string UrlKey = "UrlKey";
        public UrlShortenerController(IMemoryCacheService memoryCacheService, IUrlManager urlManager)
        {
            _memoryCacheService = memoryCacheService;
            _urlManager = urlManager;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> EncodeUrl(string url)
        { 
            try
            {
                if (ModelState.IsValid)
                {

                    var encodedUrl = $"{Request.Scheme}://{Request.Host}/";
                    var response = await _urlManager.ShortenUrl(url, encodedUrl);

                    if (response != null)
                        return Ok(response);
                    return BadRequest();
                }
                return BadRequest("Url state not good!");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("[action]")]
        [Produces("application/json")]
        public async Task<IActionResult> DecodeUrl(string url)
        {
            try
            {
                var scheme =HttpContext.Request.Scheme;
                var host = HttpContext.Request.Host.ToString();
                if (string.IsNullOrEmpty(url) && string.IsNullOrEmpty(scheme) && string.IsNullOrEmpty(host))
                {
                    throw new ArgumentNullException(nameof(url));
                }
                var getDecodedUrl = await _urlManager.HandleUrl(url,host, scheme);
                if (getDecodedUrl.UrlShortener != null)
                    return Ok(getDecodedUrl.UrlShortener.OriginalUrl);
                return BadRequest(getDecodedUrl.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Statistic(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentNullException(nameof(path));
                }

                var getDecodedUrl = await _urlManager.HandleUrlStat(path);
                if (getDecodedUrl != null)
                    return Ok(getDecodedUrl);
                return BadRequest("No such identifier found for your query");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
