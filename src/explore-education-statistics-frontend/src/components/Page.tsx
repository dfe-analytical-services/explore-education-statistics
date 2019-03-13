import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageBanner from './PageBanner';
import PageFooter from './PageFooter';
import PageHeader from './PageHeader';

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
