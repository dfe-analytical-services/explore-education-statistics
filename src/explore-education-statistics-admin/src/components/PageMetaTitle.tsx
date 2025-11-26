import { Helmet } from 'react-helmet';
import React from 'react';

export default function PageMetaTitle({ title }: { title: string }) {
  return (
    <Helmet>
      <title>{`${title} - Explore education statistics - GOV.UK`}</title>
    </Helmet>
  );
}
