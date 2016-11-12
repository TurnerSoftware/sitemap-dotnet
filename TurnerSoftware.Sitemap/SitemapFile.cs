﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnerSoftware.Sitemap
{
    public class SitemapFile
    {
        public Uri Location { get; set; }

        public DateTime? LastModified { get; set; }
        public IEnumerable<SitemapFile> Sitemaps { get; set; }
        
        public IEnumerable<SitemapEntry> Urls { get; set; }
    }
}