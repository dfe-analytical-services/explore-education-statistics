import React, { ReactNode } from 'react';
import PageFooter from '../../components/PageFooter';
import PageHeader from '../../components/PageHeader';
import PrototypeBreadcrumbs, { Breadcrumb } from './PrototypeBreadcrumbs';
import PrototypePageBanner from './PrototypePageBanner';

interface Props {
  breadcrumbs?: Breadcrumb[];
  children: ReactNode;
}

const PrototypePage = ({ breadcrumbs = [], children }: Props) => {
  return (
    <>
      <PageHeader />

      <div className="govuk-width-container">
        <PrototypePageBanner />

        <PrototypeBreadcrumbs items={breadcrumbs} />

        <main
          className="app-main-class govuk-main-wrapper"
          id="main-content"
          role="main"
        >
          {children}
        </main>
      </div>

      <PageFooter />
    </>
  );
};

export default PrototypePage;
