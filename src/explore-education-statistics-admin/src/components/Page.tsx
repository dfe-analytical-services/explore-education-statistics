import Link from '@admin/components/Link';
import PageFooter from '@admin/components/PageFooter';
import PageTitle from '@admin/components/PageTitle';
import { useCookies } from '@admin/hooks/useCookies';
import Banner from '@common/components/Banner';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Helmet } from 'react-helmet';
import Breadcrumbs, { BreadcrumbsProps } from './Breadcrumbs';
import PageBanner from './PageBanner';
import PageHeader from './PageHeader';

export type PageProps = {
  backLink?: string;
  backLinkText?: string;
  caption?: string;
  children: ReactNode;
  wide?: boolean;
  title?: string;
  pageBanner?: ReactNode;
  showBanner?: boolean;
} & BreadcrumbsProps;

const Page = ({
  backLink,
  backLinkText = 'Back',
  caption = '',
  children,
  wide = true,
  title,
  pageBanner,
  showBanner = true,
  ...breadcrumbProps
}: PageProps) => {
  const { getCookie, setAdminBannerSeenCookie } = useCookies();
  const isAdminBannerSeen = getCookie('adminBannerSeen') === 'true';

  return (
    <>
      <Helmet>
        <title>
          {title
            ? `${title} - Explore education statistics - GOV.UK`
            : 'Explore education statistics - GOV.UK'}
        </title>
      </Helmet>

      <PageHeader wide={wide} />

      {showBanner && !isAdminBannerSeen && (
        <Banner
          wide={wide}
          onClose={() => setAdminBannerSeenCookie(true)}
          testId="admin-survey-banner"
        >
          <p className="govuk-!-font-weight-bold">
            Pl-EES-e let us know what you think of EES admin!
          </p>
          <p>
            EES has been around for a little while now and we'd really
            appreciate it if you would take the time to complete our survey to
            let us know what you think about the service!
          </p>
          <p>
            <Link
              to="https://forms.gle/ZMBcQjBpr9ZZvRwu5"
              target="_blank"
              rel="noopener noreferrer"
            >
              Click here to complete our survey.
            </Link>
          </p>
          <p>
            We'll use the results to better understand the impact EES has had so
            far as well as informing where we focus our development next. This
            could include anything from fixing current pain points to
            introducing brand new functionality we may have not have even
            thought of yet!
          </p>
          <p className="govuk-!-margin-bottom-2">
            The survey is designed to gather feedback from anyone who has used
            the EES admin website to publish a statistics release at any point
            since its launch in March 2020. Please fill in yourself as an
            individual and your own experience of the service, it doesn't matter
            if others from your team or publications also fill it out too. The
            more feedback the merrier!
          </p>
        </Banner>
      )}

      <div
        className={classNames('govuk-width-container', {
          'dfe-width-container--wide': wide,
        })}
      >
        {pageBanner ?? <PageBanner />}

        <Breadcrumbs {...breadcrumbProps} />

        {backLink && (
          <Link back to={backLink}>
            {backLinkText}
          </Link>
        )}

        <main
          className="app-main-class govuk-main-wrapper"
          id="main-content"
          role="main"
        >
          {title && <PageTitle title={title} caption={caption} />}

          {children}
        </main>
      </div>

      <PageFooter wide={wide} />
    </>
  );
};

export default Page;
