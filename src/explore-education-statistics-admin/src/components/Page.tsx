import Link from '@admin/components/Link';
import PageFooter from '@admin/components/PageFooter';
import PageTitle from '@admin/components/PageTitle';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Helmet } from 'react-helmet';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageBanner from './PageBanner';
import PageHeader from './PageHeader';

export type PageProps = {
  backLink?: string;
  backLinkText?: string;
  caption?: string;
  children: ReactNode;
  wide?: boolean;
  title?: string;
  pageBanner?: ReactNode;
} & BreadcrumbsProps;

const Page = ({
  backLink,
  backLinkText = 'Back',
  caption = '',
  children,
  wide = true,
  title,
  pageBanner,
  ...breadcrumbProps
}: PageProps) => {
  return (
    <>
      <Helmet>
        <title>
          {title
            ? `${title} - Explore education statistics - GOV.UK`
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

        {backLink && (
          <Link back to={backLink}>
            {backLinkText}
          </Link>
        )}

        <main
          className="app-main-class govuk-main-wrapper"
          id="main-content"
          role="main"
        >
          {title && <PageTitle title={title} caption={caption} />}

          {children}
        </main>
      </div>

      <PageFooter wide={wide} />
    </>
  );
};

export default Page;
