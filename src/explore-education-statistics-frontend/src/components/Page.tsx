import PhaseBanner from '@common/components/PhaseBanner';
import NotificationBanner from '@common/components/NotificationBanner';
import CookieBanner from '@frontend/components/CookieBanner';
import UserTestingBanner from '@frontend/components/UserTestingBanner';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageFooter from './PageFooter';
import PageHeader from './PageHeader';
import PageMeta, { PageMetaProps } from './PageMeta';
import PageTitle from './PageTitle';
import Feedback from './Feedback';
import Link from './Link';

type Props = {
  includeDefaultMetaTitle?: boolean;
  title: string;
  metaTitle?: string;
  caption?: string;
  description?: string;
  breadcrumbLabel?: string;
  pageMeta?: PageMetaProps;
  className?: string;
  children?: ReactNode;
  wide?: boolean;
  isHomepage?: boolean;
  showApiBanner?: boolean;
} & BreadcrumbsProps;

const Page = ({
  includeDefaultMetaTitle,
  title,
  metaTitle,
  caption = '',
  description,
  breadcrumbLabel = '',
  pageMeta,
  className,
  children = null,
  wide = false,
  isHomepage = false,
  breadcrumbs = [],
  showApiBanner = true,
}: Props) => {
  return (
    <>
      <CookieBanner wide={wide} />
      <PageMeta
        includeDefaultMetaTitle={includeDefaultMetaTitle}
        title={metaTitle ?? `${title}${caption && `, ${caption}`}`}
        description={description}
        {...pageMeta}
      />
      <PageHeader />

      <UserTestingBanner />

      <div
        className={classNames('govuk-width-container', className, {
          'dfe-width-container--wide': wide,
        })}
      >
        <PhaseBanner url="https://forms.office.com/Pages/ResponsePage.aspx?id=yXfS-grGoU2187O4s0qC-XMiKzsnr8xJoWM_DeGwIu9UNDJHOEJDRklTNVA1SDdLOFJITEwyWU1OQS4u" />

        {showApiBanner && (
          <NotificationBanner
            className="govuk-!-margin-top-6"
            fullWidthContent
            title="Important update"
          >
            <p>
              The first data sets are now available in our new API,{' '}
              <Link to="/data-catalogue?dataSetType=api">
                view them in the data catalogue
              </Link>{' '}
              or read our{' '}
              <Link to="https://api.education.gov.uk/statistics/docs/ ">
                API documentation
              </Link>
              . If you have any questions or feedback, contact us at{' '}
              <a href="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </a>
            </p>
          </NotificationBanner>
        )}

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

      <Feedback />
      <PageFooter wide={wide} />
    </>
  );
};

export default Page;
