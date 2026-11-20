using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MangaProjectApi.Controllers
{
    [ApiController]
    [AllowAnonymous]
    public class SitemapController : ControllerBase
    {
        private readonly IMangasService _mangasService;
        private readonly IMangaChaptersService _chaptersService;
        private readonly INoticesService _noticesService;
        private readonly IForumService _forumService;
        private readonly IConfiguration _config;

        public SitemapController(
            IMangasService mangasService,
            IMangaChaptersService chaptersService,
            INoticesService noticesService,
            IForumService forumService,
            IConfiguration config)
        {
            _mangasService = mangasService;
            _chaptersService = chaptersService;
            _noticesService = noticesService;
            _forumService = forumService;
            _config = config;
        }

        /// <summary>
        /// Genera un sitemap.xml dinámico para motores de búsqueda (Google, Bing)
        /// incluyendo mangas, capítulos, noticias y discusiones del foro.
        /// </summary>
        [HttpGet("/sitemap.xml")]
        [Produces("application/xml")]
        public IActionResult GetSitemap()
        {
            string baseUrl = (_config["Seo:SiteUrl"]
                ?? $"{Request.Scheme}://{Request.Host}").TrimEnd('/');

            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urlset = new XElement(ns + "urlset");

            void AddUrl(string path, string changeFreq, string priority, DateTime? lastMod = null)
            {
                var urlEl = new XElement(ns + "url",
                    new XElement(ns + "loc", $"{baseUrl}{path}"),
                    new XElement(ns + "changefreq", changeFreq),
                    new XElement(ns + "priority", priority)
                );

                if (lastMod.HasValue)
                {
                    urlEl.Add(new XElement(ns + "lastmod", lastMod.Value.ToString("yyyy-MM-dd")));
                }

                urlset.Add(urlEl);
            }

            // 1. Rutas principales públicas
            AddUrl("/", "daily", "1.0", DateTime.UtcNow);
            AddUrl("/manga-section", "daily", "0.9", DateTime.UtcNow);
            AddUrl("/forum", "daily", "0.8", DateTime.UtcNow);
            AddUrl("/news", "daily", "0.8", DateTime.UtcNow);

            // 2. Mangas visibles
            try
            {
                var mangasRes = _mangasService.GetMangas();
                if (mangasRes?.Items != null)
                {
                    foreach (var m in mangasRes.Items.Where(x => !x.Hidden))
                    {
                        AddUrl($"/manga/{m.MangaId}", "weekly", "0.85", m.CreatedAt);
                    }
                }
            }
            catch { }

            // 3. Capítulos activos
            try
            {
                var chaptersRes = _chaptersService.GetMangaChapters();
                if (chaptersRes?.Items != null)
                {
                    foreach (var c in chaptersRes.Items.Where(x => x.ChapterStatus))
                    {
                        AddUrl($"/manga/{c.MangaId}/chapter/{c.ChapterId}", "monthly", "0.7", c.UploadedAt);
                    }
                }
            }
            catch { }

            // 4. Noticias activas
            try
            {
                var noticesRes = _noticesService.GetNotices();
                if (noticesRes?.Items != null)
                {
                    foreach (var n in noticesRes.Items.Where(x => x.IsActive))
                    {
                        AddUrl($"/news/{n.NoticeId}", "monthly", "0.75", n.CreatedAt);
                    }
                }
            }
            catch { }

            // 5. Publicaciones del foro
            try
            {
                var postsRes = _forumService.GetPosts();
                if (postsRes?.Items != null)
                {
                    foreach (var p in postsRes.Items)
                    {
                        AddUrl($"/forum/post/{p.PostId}", "weekly", "0.65", p.UpdatedAt ?? p.CreatedAt);
                    }
                }
            }
            catch { }

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
            return Content(doc.ToString(), "application/xml", Encoding.UTF8);
        }
    }
}
