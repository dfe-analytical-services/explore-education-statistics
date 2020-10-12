import fastTrackService from '@common/services/fastTrackService';
import tableBuilderService from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import TableToolPage, {
  TableToolPageProps,
} from '@frontend/modules/table-tool/TableToolPage';
import { GetServerSideProps } from 'next';

export default TableToolPage;

export const getServerSideProps: GetServerSideProps<TableToolPageProps> = async ({
  query,
}) => {
  const { fastTrackId } = query as Dictionary<string>;

  const [fastTrack, themeMeta] = await Promise.all([
    fastTrackService.getFastTrackTable(fastTrackId),
    tableBuilderService.getThemes(),
  ]);

  if (!fastTrack?.query.publicationId) {
    throw new Error('Fast track table does not have `query.publicationId`');
  }

  if (!fastTrack?.query.subjectId) {
    throw new Error('Fast track table does not have `query.subjectId`');
  }

  const [publicationMeta, subjectMeta] = await Promise.all([
    tableBuilderService.getPublicationMeta(fastTrack.query.publicationId),
    tableBuilderService.getSubjectMeta(fastTrack.query.subjectId),
  ]);

  return {
    props: {
      fastTrack,
      subjectMeta,
      publicationMeta,
      themeMeta,
    },
  };
};
