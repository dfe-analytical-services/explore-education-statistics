/** @type {import('next-sitemap').IConfig} */
module.exports = {
  siteUrl: process.env.PUBLIC_URL,
  sitemapSize: 5000,
  exclude: [
    '/subscriptions',
    '/subscriptions/verification-error',
    '/server-sitemap.xml',
  ],
  generateRobotsTxt: true,
  robotsTxtOptions: {
    policies: getRobotsRuleset(process.env.APP_ENV),
    additionalSitemaps: [`${process.env.PUBLIC_URL}server-sitemap.xml`],
  },
};

function getRobotsRuleset(environment) {
  if (environment === 'Production' || environment === 'Local') {
    return [
      {
        userAgent: '*',
        allow: '/',
        disallow: ['/data-tables/fast-track/', '/data-tables/permalink/'],
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
