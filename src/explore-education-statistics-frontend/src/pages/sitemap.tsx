import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';

function SitemapPage() {
  return (
    <Page breadcrumbs={[{ name: 'Sitemap' }]} pageMeta={{ title: 'Sitemap' }}>
      <PageTitle title="Sitemap" />
    </Page>
  );
}

export default SitemapPage;
