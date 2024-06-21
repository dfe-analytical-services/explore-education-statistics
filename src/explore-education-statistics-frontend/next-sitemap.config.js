/** @type {import('next-sitemap').IConfig} */
module.exports = {
  siteUrl: process.env.PROD_PUBLIC_URL,
  sitemapSize: 5000,
  exclude: ['/server-sitemap.xml'],
  generateRobotsTxt: true,
  robotsTxtOptions: {
    policies: [
      {
        userAgent: '*',
        allow: '/',
        disallow: [
          '/data-tables/fast-track/',
          '/data-tables/permalink/',
          '/subscriptions/',
        ],
      },
    ],
    additionalSitemaps: [`${process.env.PROD_PUBLIC_URL}/server-sitemap.xml`],
  },
  transform: async (config, path) => {
    if (path === '/') {
      return {
        ...generateDefaultPathProperties(config, path),
        priority: 1,
      };
    }

    if (path === '/find-statistics') {
      return {
        ...generateDefaultPathProperties(config, path),
        priority: 0.9,
      };
    }

    const lowerPriorityPages = [
      '/accessibility-statement',
      '/contact-us',
      '/cookies',
      '/cookies/details',
      '/privacy-notice',
      '/help-support',
    ];
    if (lowerPriorityPages.includes(path)) {
      return {
        ...generateDefaultPathProperties(config, path),
        priority: 0.2,
      };
    }

    return generateDefaultPathProperties(config, path);
  },
};

function generateDefaultPathProperties(config, path) {
  return {
    loc: path,
    changefreq: config.changefreq,
    priority: config.priority,
    lastmod: config.autoLastmod ? new Date().toISOString() : undefined,
    alternateRefs: config.alternateRefs ?? [],
  };
}
