import classNames from 'classnames';
import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageBanner from './PageBanner';
import PageFooter from './PageFooter';
import PageHeader from './PageHeader';
import PageMeta, { PageMetaProps } from './PageMeta';
import PageTitle from './PageTitle';

type Props = {
  title: string;
  caption?: string;
  breadcrumbLabel?: string;
  pageMeta?: PageMetaProps;
  children?: ReactNode;
  hideTitle?: boolean;
  wide?: boolean;
  isHomepage?: boolean;
} & BreadcrumbsProps;

const Page = ({
  title,
  caption = '',
  breadcrumbLabel = '',
  pageMeta,
  children = null,
  hideTitle = false,
  wide = false,
  isHomepage = false,
  breadcrumbs = [],
}: Props) => {
  return (
    <>
      <PageMeta title={title} description={caption} {...pageMeta} />
      <PageHeader wide={wide} />

      <div
        className={classNames('govuk-width-container', {
          'dfe-width-container--wide': wide,
        })}
      >
        <PageBanner />
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
          {!hideTitle && <PageTitle title={title} caption={caption} />}
          {children}
        </main>
      </div>

      <PageFooter wide={wide} />
    </>
  );
};

export default Page;
