﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TurnerSoftware.Sitemap.Request
{
    public class SitemapRequestService : ISitemapRequestService
    {
        public IEnumerable<Uri> GetAvailableSitemapsForDomain(string domainName)
        {
            //Load Robots.txt to see if we are told where the sitemaps live
            var robot = new Robots.Robots();
            var robotsUri = new UriBuilder("http", domainName);
            robot.Load(robotsUri.Uri);

            var sitemapFilePaths = robot.GetSitemapUrls();

            var httpDefaultSitemap = new UriBuilder("http", domainName)
            {
                Path = "sitemap.xml"
            }.Uri.ToString();
            var httpsDefaultSitemap = new UriBuilder("https", domainName)
            {
                Path = "sitemap.xml"
            }.Uri.ToString();

            //Check if the "default" sitemap path is in the list, if not add it
            //If we can't find a sitemap listed in the robots.txt file, add a "default" to search
            if (!sitemapFilePaths.Any(url => url == httpDefaultSitemap || url == httpsDefaultSitemap))
            {
                //Some sites (eg. stackoverflow) specify a relative path for their site maps
                if (sitemapFilePaths.Contains("/sitemap.xml"))
                {
                    sitemapFilePaths.Remove("/sitemap.xml");
                }
                
                sitemapFilePaths.Add(httpDefaultSitemap);
            }

            //Parse each of the paths and check that the file exists
            Uri tmpUri;
            var result = new List<Uri>();
            using (var httpClient = new HttpClient())
            {
                foreach (var sitemapPath in sitemapFilePaths)
                {
                    if (Uri.TryCreate(sitemapPath, UriKind.Absolute, out tmpUri))
                    {
                        //We perform a head request because we don't care about the content here
                        var requestMessage = new HttpRequestMessage(HttpMethod.Head, tmpUri);
                        var response = httpClient.SendAsync(requestMessage).Result;

                        //If it is successful, add to our results list
                        if (response.IsSuccessStatusCode)
                        {
                            result.Add(tmpUri);
                        }
                    }
                }
            }
            return result;
        }

        public string RetrieveRawSitemap(Uri sitemapLocation)
        {
            var request = WebRequest.Create(sitemapLocation);
            
            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                var stream = responseStream;
                if (sitemapLocation.AbsolutePath.EndsWith(".gz"))
                {
                    stream = new GZipStream(stream, CompressionMode.Decompress);
                }

                using (var streamReader = new StreamReader(stream))
                {
                    var result = streamReader.ReadToEnd();
                    return result;
                }
            }
        }
    }
}