import PageFooter from '@frontend/components/PageFooter';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import PrototypeBreadcrumbs, { Breadcrumb } from './PrototypeBreadcrumbs';
import PrototypePageBanner from './PrototypePageBanner';
import PrototypePageHeader from './PrototypePageHeader';

interface Props {
  breadcrumbs?: Breadcrumb[];
  children: ReactNode;
  wide?: boolean;
}

const PrototypePage = ({ breadcrumbs = [], children, wide }: Props) => {
  return (
    <>
      <PrototypePageHeader wide={wide} />

      <div
        className={classNames('govuk-width-container', {
          'dfe-width-container--wide': wide,
        })}
      >
        <PrototypePageBanner />

        <PrototypeBreadcrumbs items={breadcrumbs} />

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

export default PrototypePage;
