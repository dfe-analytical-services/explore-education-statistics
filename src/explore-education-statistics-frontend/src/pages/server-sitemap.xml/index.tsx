import { getServerSideSitemapLegacy, ISitemapField } from 'next-sitemap';
import getPublicationSlugs, {
  getMethodologySitemapFields,
} from '@frontend/services/sitemapService';
import { GetServerSideProps } from 'next';

export const getServerSideProps: GetServerSideProps = async ctx => {
  const publicationPages = await getPublicationSlugs();
  const methodologyFields = await getMethodologySitemapFields();

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
    ...methodologyFields,
  ];

  return getServerSideSitemapLegacy(ctx, fields);
};

// Default export to prevent next.js errors
export default function SitemapIndex() {}
