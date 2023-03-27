import CookieBanner from '@frontend/components/CookieBanner';
import classNames from 'classnames';
import { useRouter } from 'next/router';
import React, { ReactNode } from 'react';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageBanner from './PageBanner';
import PageFooter from './PageFooter';
import PageHeader from './PageHeader';
import PageMeta, { PageMetaProps } from './PageMeta';
import PageTitle from './PageTitle';
import TemporaryNotice from './TemporaryNotice';
import UserTestingBanner from './UserTestingBanner';

type Props = {
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
} & BreadcrumbsProps;

const Page = ({
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
}: Props) => {
  const router = useRouter();
  return (
    <>
      <CookieBanner wide={wide} />
      <PageMeta
        title={metaTitle ?? `${title}${caption && `, ${caption}`}`}
        description={description}
        {...pageMeta}
      />
      <PageHeader />
      <UserTestingBanner />
      {router.asPath.includes(
        'find-statistics/attendance-in-education-and-early-years-settings-during-the-coronavirus-covid-19-outbreak',
      ) && (
        <TemporaryNotice
          start={new Date('2020-12-01T09:30:00Z')}
          end={new Date('2020-12-15T09:30:00Z')}
          wide={wide}
        >
          <p>
            1 December 2020: Further breakdowns to be provided within the
            publication “Attendance in education and early years settings during
            the coronavirus (COVID-19) outbreak” scheduled for 15 December and
            subsequent releases
          </p>

          <p>
            From 15 December we will include local authority breakdowns
            including by pupil attendance and an update will be provided
            half-termly. Additionally school workforce data will be published in
            the new year.
          </p>
        </TemporaryNotice>
      )}

      <div
        className={classNames('govuk-width-container', className, {
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
          <PageTitle title={title} caption={caption} />
          {children}
        </main>
      </div>

      <PageFooter wide={wide} />
    </>
  );
};

export default Page;
