import tableBuilderService from '@common/services/tableBuilderService';
import publicationService from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import TableToolPage, {
  TableToolPageProps,
} from '@frontend/modules/table-tool/TableToolPage';
import { GetServerSideProps } from 'next';

export default TableToolPage;

export const getServerSideProps: GetServerSideProps<TableToolPageProps> = async ({
  query,
}) => {
  const { dataBlockId } = query as Dictionary<string>;

  const [fastTrack, themeMeta] = await Promise.all([
    tableBuilderService.getFastTrackTableAndReleaseMeta(dataBlockId),
    publicationService.getPublicationTree({
      publicationFilter: 'FastTrack',
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
    .flatMap(option => option.topics)
    .flatMap(option => option.publications)
    .find(option => option.id === fastTrack.query.publicationId);

  if (!selectedPublication) {
    throw new Error(
      'Fast track `query.publicationId` is not found in the themeMeta list',
    );
  }

  const [subjects, featuredTables, subjectMeta] = await Promise.all([
    tableBuilderService.listReleaseSubjects(fastTrack.releaseId),
    tableBuilderService.listReleaseFeaturedTables(fastTrack.releaseId),
    tableBuilderService.getSubjectMeta(
      fastTrack.query.subjectId,
      fastTrack.releaseId,
    ),
  ]);

  return {
    props: {
      fastTrack,
      featuredTables,
      selectedPublication: {
        id: selectedPublication.id,
        title: selectedPublication.title,
        slug: selectedPublication.slug,
        selectedRelease: {
          id: fastTrack.releaseId,
          slug: fastTrack.releaseSlug,
          latestData: fastTrack.latestData,
          title: fastTrack.latestReleaseTitle,
        },
        latestRelease: {
          title: fastTrack.latestReleaseTitle,
        },
      },
      subjectMeta,
      subjects,
      themeMeta,
    },
  };
};
