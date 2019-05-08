import classNames from 'classnames';
import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageBanner from './PageBanner';
import PageFooter from './PageFooter';
import PageHeader from './PageHeader';

type Props = {
  children: ReactNode;
  wide?: boolean;
} & BreadcrumbsProps;

const Page = ({ children, breadcrumbs = [], wide = false }: Props) => {
  return (
    <>
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
