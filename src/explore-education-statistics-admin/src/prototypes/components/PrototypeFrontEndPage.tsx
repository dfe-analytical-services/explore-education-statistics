import classNames from 'classnames';
import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from '@admin/components/Breadcrumbs';
import PageFooter from '@admin/components/PageFooter';
import PageHeader from '@admin/components/PageHeader';
import PageTitle from '@admin/components/PageTitle';
import PhaseBanner from '@common/components/PhaseBanner';

type Props = {
  title: string;
  caption?: string;
  breadcrumbLabel?: string;
  className?: string;
  children?: ReactNode;
  wide?: boolean;
  isHomepage?: boolean;
} & BreadcrumbsProps;

const PrototypeFrontEndPage = ({
  title,
  caption = '',
  breadcrumbLabel = '',
  className,
  children = null,
  wide = false,
  isHomepage = false,
  breadcrumbs = [],
}: Props) => {
  return (
    <>
      <PageHeader wide />

      <div
        className={classNames('govuk-width-container', className, {
          'dfe-width-container--wide': wide,
        })}
      >
        <PhaseBanner url="https://forms.office.com/Pages/ResponsePage.aspx?id=yXfS-grGoU2187O4s0qC-VQ56HAfKLpBrG0LxbfxbVdUQjVJQVdMOFlSMURGQ1kyMzRNWlpKN1NMVy4u" />
        <Breadcrumbs
          breadcrumbs={
            isHomepage
              ? undefined
              : breadcrumbs.concat([{ name: breadcrumbLabel || title }])
          }
        />

        <main
          className="govuk-main-wrapper app-main-class"
          id="main-content"
          role="main"
        >
          <PageTitle title={title} caption={caption} />
          {children}
        </main>
      </div>

      <PageFooter wide={wide} />
    </>
  );
};

export default PrototypeFrontEndPage;
