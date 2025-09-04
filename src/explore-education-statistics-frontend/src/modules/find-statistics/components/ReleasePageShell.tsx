import {
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import Page from '@frontend/components/Page';
import ReleasePageIntro from '@frontend/modules/find-statistics/components/ReleasePageIntro';
import ReleasePageTitle from '@frontend/modules/find-statistics/components/ReleasePageTitle';
import { NextPage } from 'next';
import React, { ReactNode } from 'react';

interface Props {
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
  children: ReactNode;
}

const ReleasePageShell: NextPage<Props> = ({
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
      {children}
    </Page>
  );
};

export default ReleasePageShell;
