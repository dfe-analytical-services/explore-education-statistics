import {
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import Page from '@frontend/components/Page';
import ReleasePageIntro from '@frontend/modules/find-statistics/components/ReleasePageIntro';
import ReleasePageTabNav from '@frontend/modules/find-statistics/components/ReleasePageTabNav';
import ReleasePageTitle from '@frontend/modules/find-statistics/components/ReleasePageTitle';
import { ReleasePageTabRouteKey } from '@frontend/modules/find-statistics/PublicationReleasePage';
import { NextPage } from 'next';
import React, { ReactNode } from 'react';

interface Props {
  activePage: ReleasePageTabRouteKey;
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
  children: ReactNode;
}

const ReleasePageShell: NextPage<Props> = ({
  activePage,
  publicationSummary,
  releaseVersionSummary,
  children,
}) => {
  return (
    <Page
      title={publicationSummary.title}
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
      />
      {children}
    </Page>
  );
};

export default ReleasePageShell;
