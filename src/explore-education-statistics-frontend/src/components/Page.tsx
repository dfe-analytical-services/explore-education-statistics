import CookieBanner from '@frontend/components/CookieBanner';
import TemporaryNotice from '@frontend/components/TemporaryNotice';
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
  return (
    <>
      <CookieBanner wide={wide} />
      <PageMeta
        title={`${title}${caption && `, ${caption}`}`}
        description={description}
        {...pageMeta}
      />
      <PageHeader />

      <TemporaryNotice expires={new Date('2020-06-257T00:00:00Z')} wide={wide}>
        <p>
          An issue with the connection to an internal DfE Server in the early
          hours of this morning (25 June 2020) has meant that, whilst all data
          was published for the <strong>'School workforce in England’</strong>{' '}
          and <strong>‘Schools, pupils and their characteristics’</strong> the
          aesthetics of the platform were not to the standard that we strive
          for. This meant that charts and tables were not being presented for
          these releases.
        </p>

        <p>
          Explore Education Statistics is a new platform and is continually
          being developed. We are using feedback from our users to improve the
          service. Please do send us any feedback that you have.
        </p>

        <p>
          We have used the lessons learned from today to improve our contingency
          planning so that our users will not be impacted by events such as
          those seen today. We apologise for any inconvenience the above
          described issue has caused to our users and we thank you for your
          patience.
        </p>
      </TemporaryNotice>

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
