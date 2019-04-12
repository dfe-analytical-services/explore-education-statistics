import React, { ReactNode } from 'react';
import PageBanner from '../../../explore-education-statistics-common/src/components/PageBanner';
import PageFooter from '../../../explore-education-statistics-common/src/components/PageFooter';
import PageHeader from '../../../explore-education-statistics-common/src/components/PageHeader';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';

type Props = {
  children: ReactNode;
} & BreadcrumbsProps;

const Page = ({ children, breadcrumbs = [] }: Props) => {
  return (
    <>
      <PageHeader />

      <div className="govuk-width-container">
        <PageBanner />
        <Breadcrumbs breadcrumbs={breadcrumbs} />

        <main
          className="govuk-main-wrapper app-main-class"
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

export default Page;
