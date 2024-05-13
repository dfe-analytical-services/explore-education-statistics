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
    policies: [
      {
        userAgent: '*',
        allow: '/',
        disallow: ['/data-tables/fast-track/', '/data-tables/permalink/'],
      },
    ],
    additionalSitemaps: [`${process.env.PUBLIC_URL}server-sitemap.xml`],
  },
};
