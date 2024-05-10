// pages/server-sitemap-index.xml/index.tsx
import { getServerSideSitemapLegacy, ISitemapField } from 'next-sitemap';
import getPublicationSlugs from '@frontend/services/sitemapService';
import { GetServerSideProps } from 'next';

export const getServerSideProps: GetServerSideProps = async ctx => {
  const publicationPages = await getPublicationSlugs();

  // Method to source urls from cms
  // const urls = await fetch('https//example.com/api')

  const fields: ISitemapField[] = [
    {
      loc: 'https://example.com', // Absolute url
      lastmod: new Date().toISOString(),
      // changefreq
      // priority
    },
    {
      loc: 'https://example.com/dynamic-path-2', // Absolute url
      lastmod: new Date().toISOString(),
      // changefreq
      // priority
    },
    ...publicationPages,
  ];

  return getServerSideSitemapLegacy(ctx, fields);
};

// Default export to prevent next.js errors
export default function SitemapIndex() {}
