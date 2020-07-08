import PageFooter from '@admin/components/PageFooter';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Helmet } from 'react-helmet';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageBanner from './PageBanner';
import PageHeader from './PageHeader';

export type PageProps = {
  children: ReactNode;
  wide?: boolean;
  pageTitle?: string;
  pageBanner?: ReactNode;
} & BreadcrumbsProps;

const Page = ({
  children,
  wide,
  pageTitle,
  pageBanner,
  ...breadcrumbProps
}: PageProps) => {
  return (
    <>
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
        {pageBanner ?? <PageBanner />}

        <Breadcrumbs {...breadcrumbProps} />

        <main
          className="app-main-class govuk-main-wrapper"
          id="main-content"
          role="main"
        >
          {pageTitle && <h1 className="govuk-heading-xl">{pageTitle}</h1>}
          {children}
        </main>
      </div>

      <PageFooter wide={wide} />
    </>
  );
};

export default Page;
