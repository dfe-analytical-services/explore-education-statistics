import React from 'react';
import Head from 'next/head';

export interface PageMetaProps {
  title?: string;
  description?: string;
  imgUrl?: string;
}

const defaultPageTitle = 'Explore education statistics – GOV.UK';

const PageMeta = ({
  title = defaultPageTitle,
  description = 'Find, download and explore official Department for Education (DfE) statistics and data in England.',
  imgUrl,
}: PageMetaProps) => {
  return (
    <Head>
      {/* <!-- Primary Meta Tags --> */}
      <title>
        {title && title !== defaultPageTitle
          ? `${title} – ${defaultPageTitle}`
          : title}
      </title>
      <meta name="title" content={title} />
      <meta name="description" content={description} />
      <meta name="theme-color" content="#0b0c0c" />
      <meta
        name="google-site-verification"
        content="jWf4Mg_pzTOgXDWccGcv9stMsdyptYwHeVpODHdesoY"
      />

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
    </Head>
  );
};

export default PageMeta;
