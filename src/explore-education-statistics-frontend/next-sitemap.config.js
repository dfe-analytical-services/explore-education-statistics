/** @type {import('next-sitemap').IConfig} */
module.exports = {
  siteUrl: process.env.PUBLIC_URL,
  exclude: [
    '/subscriptions',
    '/subscriptions/verification-error',
    '/server-sitemap-index.xml',
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
    additionalSitemaps: [`${process.env.PUBLIC_URL}server-sitemap-index.xml`],
  },
};
