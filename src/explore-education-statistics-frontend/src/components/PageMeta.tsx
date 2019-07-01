import React from 'react';
import { Helmet } from 'react-helmet';

interface Props {
  title?: string;
  description?: string;
  imgUrl?: string;
}

const PageMeta = ({
  title = 'GOV.UK',
  description = 'Explore education statistics',
  imgUrl = '/assets/images/govuk-opengraph-image.png',
}: Props) => {
  return (
    <Helmet>
      {/* <!-- Primary Meta Tags --> */}
      <title>{title}</title>
      <meta name="title" content={title} />
      <meta name="description" content={description} />

      {/* <!-- Open Graph / Facebook --> */}
      <meta property="og:type" content="website" />
      <meta property="og:url" content="" />
      <meta property="og:title" content={title} />
      <meta property="og:description" content={description} />
      <meta property="og:image" content={imgUrl} />

      {/* <!-- Twitter --> */}
      <meta property="twitter:card" content="summary_large_image" />
      <meta property="twitter:url" content="" />
      <meta property="twitter:title" content={title} />
      <meta property="twitter:description" content={description} />
      <meta property="twitter:image" content={imgUrl} />
    </Helmet>
  );
};

export default PageMeta;
