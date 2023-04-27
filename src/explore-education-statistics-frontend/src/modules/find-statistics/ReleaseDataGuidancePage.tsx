import ReleaseDataGuidancePageContent from '@common/modules/release/components/ReleaseDataGuidancePageContent';
import releaseDataGuidanceService, {
  ReleaseDataGuidanceSummary,
} from '@common/services/releaseDataGuidanceService';
import Page from '@frontend/components/Page';
import Link from '@frontend/components/Link';
import { GetServerSideProps, NextPage } from 'next';

interface Props {
  release: ReleaseDataGuidanceSummary;
}

const ReleaseDataGuidancePage: NextPage<Props> = ({ release }) => {
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
      breadcrumbLabel="Data guidance"
    >
      <h2>Data guidance</h2>

      <ReleaseDataGuidancePageContent
        published={release.published}
        dataGuidance={release.dataGuidance}
        renderDataCatalogueLink={
          <Link
            to={`/data-catalogue/${release.publication.slug}/${release.slug}`}
          >
            data catalogue
          </Link>
        }
        subjects={release.subjects}
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
