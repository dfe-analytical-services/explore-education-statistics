import ReleaseMetaGuidancePageContent from '@common/modules/release/components/ReleaseMetaGuidancePageContent';
import releaseMetaGuidanceService, {
  ReleaseMetaGuidanceSummary,
} from '@common/services/releaseMetaGuidanceService';
import Page from '@frontend/components/Page';
import { GetServerSideProps } from 'next';
import React from 'react';

interface Props {
  release: ReleaseMetaGuidanceSummary;
}

const ReleaseMetaGuidancePage = ({ release }: Props) => {
  return (
    <Page
      title={release.publication.title}
      caption={release.title}
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        {
          name: release.publication.title,
          link: release.latestRelease
            ? `/find-statistics/${release.publication.slug}`
            : `/find-statistics/${release.publication.slug}/${release.slug}`,
        },
      ]}
      breadcrumbLabel="Metadata guidance document"
    >
      <h2>Metadata guidance document</h2>

      <ReleaseMetaGuidancePageContent
        published={release.published}
        metaGuidance={release.metaGuidance}
        subjects={release.subjects}
      />
    </Page>
  );
};

export default ReleaseMetaGuidancePage;

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const { publication, release } = query as {
    publication: string;
    release: string;
  };

  const data = await (release
    ? releaseMetaGuidanceService.getReleaseMetaGuidance(publication, release)
    : releaseMetaGuidanceService.getLatestReleaseMetaGuidance(publication));

  return {
    props: {
      release: data,
    },
  };
};
