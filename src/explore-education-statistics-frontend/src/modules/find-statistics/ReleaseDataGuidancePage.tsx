import ReleaseDataGuidancePageContent from '@common/modules/release/components/ReleaseDataGuidancePageContent';
import releaseDataGuidanceService, {
  ReleaseDataGuidanceSummary,
} from '@common/services/releaseDataGuidanceService';
import Page from '@frontend/components/Page';
import Link from '@frontend/components/Link';
import { GetServerSideProps } from 'next';
import React from 'react';

interface Props {
  release: ReleaseDataGuidanceSummary;
}

const ReleaseDataGuidancePage = ({ release }: Props) => {
  return (
    <Page
      title={release.publication.title}
      metaTitle={`${
        release.publication.title
      } data guidance ${release.title.toLocaleLowerCase()}`}
      description={`Data guidance describing the contents of files containing statistics from ${release.publication.title.toLocaleLowerCase()} ${release.title.toLocaleLowerCase()}.`}
      caption={release.title}
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        {
          name: release.publication.title,
          link: `/find-statistics/${release.publication.slug}/${release.slug}`,
        },
      ]}
      breadcrumbLabel="Data guidance"
    >
      <h2>Data guidance</h2>

      <ReleaseDataGuidancePageContent
        published={release.published}
        dataGuidance={release.dataGuidance}
        renderDataCatalogueLink={
          <Link
            to={`/data-catalogue?publicationId=${release.publication.id}&releaseVersionId=${release.id}`}
          >
            data catalogue
          </Link>
        }
        dataSets={release.dataSets}
      />
    </Page>
  );
};

export default ReleaseDataGuidancePage;

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const { publication, release } = query as {
    publication: string;
    release: string;
  };

  const data = await (release
    ? releaseDataGuidanceService.getReleaseDataGuidance(publication, release)
    : releaseDataGuidanceService.getLatestReleaseDataGuidance(publication));

  return {
    props: {
      release: data,
    },
  };
};
