import NotificationBanner from '@common/components/NotificationBanner';
import { NavItem } from '@common/components/PageNavExpandable';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import {
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import ReleasePageIntro from '@frontend/modules/find-statistics/components/ReleasePageIntro';
import ReleasePageTabNav, {
  TabRouteItem,
} from '@frontend/modules/find-statistics/components/ReleasePageTabNav';
import ReleasePageTitle from '@frontend/modules/find-statistics/components/ReleasePageTitle';
import {
  releasePageTabRouteItems,
  ReleasePageTabRouteKey,
} from '@frontend/modules/find-statistics/PublicationReleasePage';
import { NextPage } from 'next';
import React, { ReactNode } from 'react';

interface Props {
  activePage: ReleasePageTabRouteKey;
  inPageNavItems: NavItem[];
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
  tabNavItems: TabRouteItem;
  children: ReactNode;
}

const ReleasePageShell: NextPage<Props> = ({
  activePage,
  inPageNavItems,
  publicationSummary,
  releaseVersionSummary,
  tabNavItems,
  children,
}) => {
  return (
    <Page
      title={publicationSummary.title}
      metaTitle={`${releasePageTabRouteItems[activePage].title} - ${publicationSummary.title}`}
      description={publicationSummary.summary}
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
      ]}
      pageTitleComponent={
        <ReleasePageTitle
          publicationSummary={publicationSummary}
          releaseTitle={releaseVersionSummary.title}
        />
      }
      customBannerContent={
        <NotificationBanner
          className="govuk-!-margin-top-6"
          fullWidthContent
          title="New release page"
        >
          <p>
            You are viewing the new design of the Release page - if you would
            like to provide feedback, please complete{' '}
            <Link
              to="https://forms.office.com/e/sBRKZgs6zB"
              target="_blank"
              rel="noopener noreferrer nofollow"
            >
              our feedback form (opens in new window)
            </Link>
          </p>
        </NotificationBanner>
      }
      width="wide"
    >
      <ReleasePageIntro
        publicationSummary={publicationSummary}
        releaseVersionSummary={releaseVersionSummary}
      />
      <ReleasePageTabNav
        activePage={activePage}
        releaseUrlBase={`/find-statistics/${publicationSummary.slug}/${releaseVersionSummary.slug}`}
        tabNavItems={tabNavItems}
      />
      <ReleasePageLayout navItems={inPageNavItems}>
        {children}
      </ReleasePageLayout>
    </Page>
  );
};

export default ReleasePageShell;
