import tableBuilderService from '@common/services/tableBuilderService';
import publicationService from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import TableToolPage, {
  TableToolPageProps,
} from '@frontend/modules/table-tool/TableToolPage';
import { GetServerSideProps } from 'next';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';

export default TableToolPage;

export const getServerSideProps: GetServerSideProps<TableToolPageProps> =
  withAxiosHandler(async ({ query }) => {
    const { dataBlockParentId } = query as Dictionary<string>;

    const [fastTrack, themeMeta] = await Promise.all([
      tableBuilderService.getFastTrackTableAndReleaseMeta(dataBlockParentId),
      publicationService.getPublicationTree({
        filter: 'FastTrack',
      }),
    ]);
    if (!fastTrack) {
      throw new Error('Fast track not found');
    }

    if (!fastTrack.query.publicationId) {
      throw new Error('Fast track table does not have `query.publicationId`');
    }

    if (!fastTrack.query.subjectId) {
      throw new Error('Fast track table does not have `query.subjectId`');
    }

    const selectedPublication = themeMeta
      .flatMap(option => option.publications)
      .find(option => option.id === fastTrack.query.publicationId);

    if (!selectedPublication) {
      throw new Error(
        'Fast track `query.publicationId` is not found in the themeMeta list',
      );
    }

    const [
      subjects,
      featuredTables,
      subjectMeta,
      publicationMethodologies,
      publicationSummary,
      releaseVersionSummary,
    ] = await Promise.all([
      tableBuilderService.listReleaseSubjects(fastTrack.releaseId),
      tableBuilderService.listReleaseFeaturedTables(fastTrack.releaseId),
      tableBuilderService.getSubjectMeta(
        fastTrack.query.subjectId,
        fastTrack.releaseId,
      ),
      publicationService.getPublicationMethodologies(selectedPublication.slug),
      publicationService.getPublicationSummary(selectedPublication.slug),
      publicationService.getReleaseVersionSummary(
        selectedPublication.slug,
        fastTrack.releaseSlug,
      ),
    ]);

    return {
      props: {
        fastTrack,
        featuredTables,
        publicationMethodologies,
        selectedPublication: {
          ...selectedPublication,
          contact: publicationSummary.contact,
          selectedRelease: {
            id: fastTrack.releaseId,
            latestData: fastTrack.latestData,
            publishingOrganisations:
              releaseVersionSummary.publishingOrganisations,
            slug: fastTrack.releaseSlug,
            title: fastTrack.latestReleaseTitle,
            type: fastTrack.releaseType,
          },
          latestRelease: {
            title: fastTrack.latestReleaseTitle,
            slug: fastTrack.latestReleaseSlug,
          },
        },
        subjectMeta,
        subjects,
        themeMeta,
      },
    };
  });
