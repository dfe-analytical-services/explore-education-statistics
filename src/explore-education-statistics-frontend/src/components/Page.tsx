import Banner from '@common/components/Banner';
import useMounted from '@common/hooks/useMounted';
import CookieBanner from '@frontend/components/CookieBanner';
import Link from '@frontend/components/Link';
import { useCookies } from '@frontend/hooks/useCookies';
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
  const { isMounted } = useMounted();
  const { getCookie, setUserTestingBannerSeenCookie } = useCookies();
  const isUserTestingBannerSeen = getCookie('userTestingBannerSeen') === 'true';

  return (
    <>
      <CookieBanner wide={wide} />
      <PageMeta
        title={metaTitle ?? `${title}${caption && `, ${caption}`}`}
        description={description}
        {...pageMeta}
      />
      <PageHeader />

      {isMounted && !isUserTestingBannerSeen && (
        <Banner onClose={() => setUserTestingBannerSeenCookie(true)}>
          <p className="govuk-!-font-weight-bold govuk-!-margin-bottom-0">
            Help develop Explore education statistics
          </p>
          <p className="govuk-!-margin-bottom-2">
            <Link
              to="https://forms.office.com/Pages/ResponsePage.aspx?id=yXfS-grGoU2187O4s0qC-XMiKzsnr8xJoWM_DeGwIu9UQVNYVkxZSEJVVjhPOURXSjJVMjhZRTdYMi4u"
              target="_blank"
              rel="noopener noreferrer"
            >
              Get involved in making this service better
            </Link>
          </p>
        </Banner>
      )}

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
