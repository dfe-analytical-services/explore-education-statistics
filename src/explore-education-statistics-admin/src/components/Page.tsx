import Link from '@admin/components/Link';
import PageFooter from '@admin/components/PageFooter';
import PageTitle from '@admin/components/PageTitle';
import PhaseBanner from '@common/components/PhaseBanner';
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
      {/* eslint-disable-next-line jsx-a11y/anchor-has-content */}
      <a id="top" />
      <PageHeader wide={wide} />
      <div
        className={classNames('govuk-width-container', {
          'dfe-width-container--wide': wide,
        })}
      >
        {pageBanner ?? (
          <PhaseBanner url="https://forms.office.com/Pages/ResponsePage.aspx?id=yXfS-grGoU2187O4s0qC-VQ56HAfKLpBrG0LxbfxbVdUQjVJQVdMOFlSMURGQ1kyMzRNWlpKN1NMVy4u" />
        )}

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
