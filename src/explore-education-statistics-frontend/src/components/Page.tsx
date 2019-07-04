import classNames from 'classnames';
import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageBanner from './PageBanner';
import PageFooter from './PageFooter';
import PageHeader from './PageHeader';
import PageMeta, { PageMetaProps } from './PageMeta';

type Props = {
  children: ReactNode;
  wide?: boolean;
  pageMeta?: PageMetaProps;
} & BreadcrumbsProps;

const Page = ({
  children,
  breadcrumbs = [],
  wide = false,
  pageMeta,
}: Props) => {
  return (
    <>
      <PageMeta {...pageMeta} />
      <PageHeader wide={wide} />

      <div
        className={classNames('govuk-width-container', {
          'dfe-width-container--wide': wide,
        })}
      >
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

      <PageFooter wide={wide} />
    </>
  );
};

export default Page;
