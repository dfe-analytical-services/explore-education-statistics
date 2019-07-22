import React from 'react';
import { Helmet } from 'react-helmet';

export interface PageMetaProps {
  title?: string;
  description?: string;
  imgUrl?: string;
}

const PageMeta = ({
  title = 'Explore education statistics',
  description = 'Find, download and explore official Department for Education (DfE) statistics and data in England.',
  imgUrl,
}: PageMetaProps) => {
  return (
    <Helmet>
      {/* <!-- Primary Meta Tags --> */}
      <title>{title}</title>
      <meta name="title" content={title} />
      <meta name="description" content={description} />

      {/* <!-- Open Graph / Facebook --> */}
      <meta property="og:type" content="website" />
      <meta property="og:title" content={title} />
      <meta property="og:description" content={description} />
      {imgUrl && <meta property="og:image" content={imgUrl} />}

      {/* <!-- Twitter --> */}
      <meta property="twitter:card" content="summary_large_image" />
      <meta property="twitter:title" content={title} />
      <meta property="twitter:description" content={description} />
      {imgUrl && <meta property="twitter:image" content={imgUrl} />}
    </Helmet>
  );
};

export default PageMeta;
