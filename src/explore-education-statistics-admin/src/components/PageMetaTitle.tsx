import React from 'react';
import { Head } from '@unhead/react';

export default function PageMetaTitle({ title }: { title: string }) {
  return (
    <Head>
      <title>{`${title} - Explore education statistics - GOV.UK`}</title>
    </Head>
  );
}
