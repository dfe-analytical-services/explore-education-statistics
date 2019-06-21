import PageFooter from '@admin/components/PageFooter';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PrototypePageBanner from './PageBanner';
import PageHeader from './PageHeader';

type Props = {
  children: ReactNode;
  wide?: boolean;
} & BreadcrumbsProps;

const Page = ({ breadcrumbs = [], children, wide }: Props) => {
  return (
    <>
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
    </>
  );
};

export default Page;
