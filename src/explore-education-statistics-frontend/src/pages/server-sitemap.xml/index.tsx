import { getServerSideSitemapLegacy } from 'next-sitemap';
import getSitemapFields from '@frontend/services/sitemapService';
import { GetServerSideProps } from 'next';

export const getServerSideProps: GetServerSideProps = async ctx => {
  const dynamicFields = await getSitemapFields();

  return getServerSideSitemapLegacy(ctx, dynamicFields);
};

export default function SitemapIndex() {}
