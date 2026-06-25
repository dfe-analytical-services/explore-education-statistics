import Link from '@admin/components/Link';
import PageFooter from '@admin/components/PageFooter';
import PageTitle from '@admin/components/PageTitle';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageHeader from './PageHeader';

export type PageProps = {
  backLink?: string;
  backLinkText?: string;
  caption?: string;
  children: ReactNode;
  wide?: boolean;
  title?: string;
} & BreadcrumbsProps;

const Page = ({
  backLink,
  backLinkText = 'Back',
  caption = '',
  children,
  wide = true,
  title,
  ...breadcrumbProps
}: PageProps) => {
  return (
    <>
      {/* eslint-disable-next-line jsx-a11y/anchor-has-content */}
      <a id="top" />
      <PageHeader wide={wide} />
      <div
        className={classNames('govuk-width-container', {
          'dfe-width-container--wide': wide,
        })}
      >
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
