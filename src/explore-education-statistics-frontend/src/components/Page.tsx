import classNames from 'classnames';
import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageBanner from './PageBanner';
import PageFooter from './PageFooter';
import PageHeader from './PageHeader';
import PageMeta, { PageMetaProps } from './PageMeta';
import PageTitle from './PageTitle';

type Props = {
  children: ReactNode;
  wide?: boolean;
  pageMeta?: PageMetaProps;
  title: string;
  hideTitle?: boolean;
  caption?: string;
  hideCaption?: boolean;
  isHomepage?: boolean;
  breadcrumbLabel?: string;
} & BreadcrumbsProps;

const Page = ({
  children,
  breadcrumbs = [],
  breadcrumbLabel = '',
  wide = false,
  pageMeta,
  title,
  hideTitle = false,
  caption = '',
  hideCaption = false,
  isHomepage = false,
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
          {!hideTitle && (
            <PageTitle title={title} caption={hideCaption ? '' : caption} />
          )}
          {children}
        </main>
      </div>

      <PageFooter wide={wide} />
    </>
  );
};

export default Page;
