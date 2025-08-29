import { ReleaseVersion } from '@common/services/publicationService';
import Page from '@frontend/components/Page';
import ReleasePageIntro from '@frontend/modules/find-statistics/components/ReleasePageIntro';
import ReleasePageTitle from '@frontend/modules/find-statistics/components/ReleasePageTitle';
import { NextPage } from 'next';
import React, { ReactNode } from 'react';

interface Props {
  releaseVersion: ReleaseVersion;
  children: ReactNode;
}

const ReleasePageShell: NextPage<Props> = ({ releaseVersion, children }) => {
  return (
    <Page
      title={releaseVersion.publication.title}
      description={releaseVersion.publication.summary}
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
      ]}
      pageTitleComponent={<ReleasePageTitle releaseVersion={releaseVersion} />}
      wide
    >
      <ReleasePageIntro releaseVersion={releaseVersion} />
      {children}
    </Page>
  );
};

export default ReleasePageShell;
