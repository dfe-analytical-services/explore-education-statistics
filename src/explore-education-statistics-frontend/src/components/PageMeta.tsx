import { useTenant } from '@common/contexts/TenantContext';
import getPublicEnv from '@frontend/utils/publicEnv';
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

const PageMeta = ({
  includeDefaultMetaTitle = true,
  canonicalPathOverride,
  title,
  description,
  imgUrl,
  additionalMeta = [],
}: PageMetaProps) => {
  const {
    service: { nameStartSentence },
    organisation,
  } = useTenant();
  const defaultPageTitle = `${nameStartSentence} - GOV.UK`;
  const pageTitle = title ?? defaultPageTitle;
  const pageDescription =
    description ??
    `Find, download and explore official ${organisation.name}${
      organisation.acronym ? ` (${organisation.acronym})` : ''
    } statistics and data in England.`;

  // Generate canonical link
  const router = useRouter();
  const { asPath } = router; // Full path including query params
  const url = new URL(asPath, getPublicEnv('PUBLIC_URL'));
  const pageParam = url.searchParams.get('page');
  url.hash = '';
  url.search = pageParam ? `page=${pageParam}` : '';
  const canonicalLink = url.toString();

  return (
    <Head>
      {/* <!-- Primary Meta Tags --> */}
      <title>
        {pageTitle && pageTitle !== defaultPageTitle && includeDefaultMetaTitle
          ? `${pageTitle} - ${defaultPageTitle}`
          : pageTitle}
      </title>
      <meta name="title" content={pageTitle} />
      <meta name="description" content={pageDescription} />
      <meta name="theme-color" content="#0b0c0c" />
      <meta
        name="google-site-verification"
        content="jWf4Mg_pzTOgXDWccGcv9stMsdyptYwHeVpODHdesoY"
      />

      <link
        rel="canonical"
        href={
          canonicalPathOverride
            ? `${getPublicEnv('PUBLIC_URL')}${canonicalPathOverride}`
            : canonicalLink
        }
        key="canonical"
      />

      {getPublicEnv('APP_ENV') !== 'Production' && (
        <meta name="robots" content="noindex,nofollow" />
      )}
      {/* <!-- Open Graph / Facebook --> */}
      <meta property="og:type" content="website" />
      <meta property="og:title" content={pageTitle} />
      <meta property="og:description" content={pageDescription} />
      {imgUrl && (
        <meta
          property="og:image"
          content={getPublicEnv('PUBLIC_URL') + imgUrl}
        />
      )}

      {/* <!-- Twitter --> */}
      <meta property="twitter:card" content="summary_large_image" />
      <meta property="twitter:title" content={pageTitle} />
      <meta property="twitter:description" content={pageDescription} />
      {imgUrl && (
        <meta
          property="twitter:image"
          content={getPublicEnv('PUBLIC_URL') + imgUrl}
        />
      )}

      {/* <!-- Additional Meta Tags --> */}
      {additionalMeta.map(PageMetaItem)}
    </Head>
  );
};

export default PageMeta;
