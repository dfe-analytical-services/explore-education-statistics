import PageFooter from '@admin/components/PageFooter';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Helmet } from 'react-helmet';
import ErrorBoundary from '@admin/components/ErrorBoundary';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PrototypePageBanner from './PageBanner';
import PageHeader from './PageHeader';

type Props = {
  children: ReactNode;
  wide?: boolean;
  pageTitle?: string;
} & BreadcrumbsProps;

const Page = ({ breadcrumbs = [], children, wide, pageTitle }: Props) => {
  return (
    <>
      <ErrorBoundary>
        <Helmet>
          <title>
            {pageTitle
              ? `${pageTitle} - Explore education statistics - GOV.UK`
              : 'Explore education statistics - GOV.UK'}
          </title>
        </Helmet>

        <PageHeader wide={wide} />

        <div
          className={classNames('govuk-width-container', {
            'dfe-width-container--wide': wide,
          })}
        >
          <PrototypePageBanner />

          <Breadcrumbs breadcrumbs={breadcrumbs} />

          <main
            className="app-main-class govuk-main-wrapper"
            id="main-content"
            role="main"
          >
            {children}
          </main>
        </div>

        <PageFooter wide={wide} />
      </ErrorBoundary>
    </>
  );
};

export default Page;
