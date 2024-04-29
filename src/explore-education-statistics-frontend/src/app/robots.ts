import { MetadataRoute } from 'next';

export default function robots(): MetadataRoute.Robots {
  return {
    rules: [
      {
        userAgent: '*',
        allow: '/',
        disallow: ['/data-tables/fast-track/', '/data-tables/permalink/'],
      },
    ],
    sitemap: `${process.env.PUBLIC_URL}/sitemap.xml`,
  };
}
