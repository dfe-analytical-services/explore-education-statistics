import React from 'react';
import Head from 'next/head';
import { useRouter } from 'next/router';
import PageMetaItem, { PageMetaItemType } from './PageMetaItem';

export interface PageMetaProps {
  includeDefaultMetaTitle?: boolean;
  canonicalPathOverride?: string;
  title?: string;
  description?: string;
  imgUrl?: string;
  additionalMeta?: PageMetaItemType[];
}

const defaultPageTitle = 'Explore education statistics - GOV.UK';

const PageMeta = ({
  includeDefaultMetaTitle = true,
  canonicalPathOverride,
  title = defaultPageTitle,
  description = 'Find, download and explore official Department for Education (DfE) statistics and data in England.',
  imgUrl,
  additionalMeta = [],
}: PageMetaProps) => {
  // Generate canonical link
  const router = useRouter();
  const { asPath } = router; // Full path including query params
  const url = new URL(asPath, process.env.PUBLIC_URL);
  const pageParam = url.searchParams.get('page');
  url.hash = '';
  url.search = pageParam ? `page=${pageParam}` : '';
  const canonicalLink = url.toString();

  return (
    <Head>
      {/* <!-- Primary Meta Tags --> */}
      <title>
        {title && title !== defaultPageTitle && includeDefaultMetaTitle
          ? `${title} - ${defaultPageTitle}`
          : title}
      </title>
      <meta name="title" content={title} />
      <meta name="description" content={description} />
      <meta name="theme-color" content="#0b0c0c" />
      <meta
        name="google-site-verification"
        content="jWf4Mg_pzTOgXDWccGcv9stMsdyptYwHeVpODHdesoY"
      />

      <link
        rel="canonical"
        href={
          canonicalPathOverride
            ? `${process.env.PUBLIC_URL}${canonicalPathOverride}`
            : canonicalLink
        }
        key="canonical"
      />

      {process.env.APP_ENV !== 'Production' && (
        <meta name="robots" content="noindex,nofollow" />
      )}
      {/* <!-- Open Graph / Facebook --> */}
      <meta property="og:type" content="website" />
      <meta property="og:title" content={title} />
      <meta property="og:description" content={description} />
      {imgUrl && (
        <meta property="og:image" content={process.env.PUBLIC_URL + imgUrl} />
      )}

      {/* <!-- Twitter --> */}
      <meta property="twitter:card" content="summary_large_image" />
      <meta property="twitter:title" content={title} />
      <meta property="twitter:description" content={description} />
      {imgUrl && (
        <meta
          property="twitter:image"
          content={process.env.PUBLIC_URL + imgUrl}
        />
      )}

      {/* <!-- Additional Meta Tags --> */}
      {additionalMeta.map(PageMetaItem)}
    </Head>
  );
};

export default PageMeta;
