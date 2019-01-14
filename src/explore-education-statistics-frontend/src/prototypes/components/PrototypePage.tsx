import React, { ReactNode } from 'react';
import PrototypeBreadcrumbs, { Breadcrumb } from './PrototypeBreadcrumbs';
import PrototypePageBanner from './PrototypePageBanner';

interface Props {
  breadcrumbs?: Breadcrumb[];
  children: ReactNode;
}

const PrototypePage = ({ breadcrumbs = [], children }: Props) => {
  return (
    <>
      <PrototypePageBanner />

      <PrototypeBreadcrumbs items={breadcrumbs} />

      <main
        className="app-main-class govuk-main-wrapper"
        id="main-content"
        role="main"
      >
        {children}
      </main>
    </>
  );
};

export default PrototypePage;
