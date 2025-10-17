import { NavItem } from '@common/components/PageNavExpandable';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import {
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
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
