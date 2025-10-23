import PhaseBanner from '@common/components/PhaseBanner';
import CookieBanner from '@frontend/components/CookieBanner';
import UserTestingBanner from '@frontend/components/UserTestingBanner';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageFooter from './PageFooter';
import PageHeader from './PageHeader';
import PageMeta, { PageMetaProps } from './PageMeta';
import PageTitle from './PageTitle';
import PageFeedback from './PageFeedback';

export type PageWidth = 'wide' | 'full';

type Props = {
  includeDefaultMetaTitle?: boolean;
  title: string;
  metaTitle?: string;
  caption?: string;
  captionInsideTitle?: boolean; // Use when caption should be part of `h1` too
  description?: string;
  breadcrumbLabel?: string;
  pageMeta?: PageMetaProps;
  pageTitleComponent?: ReactNode;
  className?: string;
  children?: ReactNode;
  customBannerContent?: ReactNode;
  width?: PageWidth;
  isHomepage?: boolean;
} & BreadcrumbsProps;

const Page = ({
  includeDefaultMetaTitle,
  title,
  metaTitle,
  caption = '',
  captionInsideTitle = false,
  description,
  breadcrumbLabel = '',
  pageMeta,
  pageTitleComponent = null,
  className,
  children = null,
  customBannerContent = null,
  width,
  isHomepage = false,
  breadcrumbs = [],
}: Props) => {
  return (
    <>
      {/* eslint-disable-next-line jsx-a11y/anchor-has-content */}
      <a id="top" />
      <CookieBanner width={width} />
      <PageMeta
        includeDefaultMetaTitle={includeDefaultMetaTitle}
        title={metaTitle ?? `${title}${caption && `, ${caption}`}`}
        description={description}
        {...pageMeta}
      />
      <PageHeader width={width} />

      <UserTestingBanner width={width} />

      <div
        className={classNames('govuk-width-container', className, {
          'dfe-width-container--wide': width === 'wide',
          'dfe-width-container--full': width === 'full',
        })}
      >
        <PhaseBanner url="https://forms.office.com/Pages/ResponsePage.aspx?id=yXfS-grGoU2187O4s0qC-XMiKzsnr8xJoWM_DeGwIu9UNDJHOEJDRklTNVA1SDdLOFJITEwyWU1OQS4u" />

        {customBannerContent}

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
          {pageTitleComponent || (
            <PageTitle
              title={title}
              caption={caption}
              captionInsideTitle={captionInsideTitle}
            />
          )}
          {children}
        </main>
      </div>

      <PageFeedback width={width} />
      <PageFooter width={width} />
    </>
  );
};

export default Page;
