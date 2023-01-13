import Page, { PageProps } from '@admin/components/Page';
import PrototypePageBanner from '@admin/prototypes/components/PrototypePageBanner';
import React from 'react';

const PrototypePage = ({ children, ...props }: PageProps) => {
  return (
    <Page
      {...props}
      homePath="./homepage5"
      pageBanner={<PrototypePageBanner />}
    >
      {children}
    </Page>
  );
};

export default PrototypePage;
