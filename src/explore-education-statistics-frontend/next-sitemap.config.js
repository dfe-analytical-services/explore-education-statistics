/** @type {import('next-sitemap').IConfig} */
module.exports = {
  siteUrl: process.env.PUBLIC_URL,
  sitemapSize: 5000,
  exclude: ['/server-sitemap.xml'],
  generateRobotsTxt: true,
  robotsTxtOptions: {
    policies: getRobotsRuleset(process.env.APP_ENV),
    additionalSitemaps: [`${process.env.PUBLIC_URL}server-sitemap.xml`],
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

function getRobotsRuleset(environment) {
  if (environment === 'Production' || environment === 'Local') {
    return [
      {
        userAgent: '*',
        allow: '/',
        disallow: [
          '/data-tables/fast-track/',
          '/data-tables/permalink/',
          '/subscriptions/',
        ],
      },
    ];
  }

  // Disallow any and all trawling of staging, dev, or pre-prod sites
  return [
    {
      userAgent: '*',
      disallow: '/',
    },
  ];
}
