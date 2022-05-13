import tableBuilderService, {
  ReleaseTableDataQuery,
  SubjectMeta,
  TableDataResponse,
} from '@common/services/tableBuilderService';

export interface InitialStepSubjectMeta {
  initialStep: number;
  subjectMeta?: SubjectMeta;
}

/**
 * Get the initial state for table tool given a {@param query}.
 * The {@param tableData} can also be provided in cases where
 * there is potentially a result.
 *
 * This allows us to partially setup table tool, even when
 * some of the query options are invalid. The user can then
 * go back through table tool and pick valid options.
 */
export default async function getInitialStepSubjectMeta(
  query: ReleaseTableDataQuery,
  tableData?: TableDataResponse,
): Promise<InitialStepSubjectMeta> {
  if (!query.releaseId) {
    return {
      initialStep: 1,
    };
  }

  if (!query.subjectId) {
    return {
      initialStep: 2,
    };
  }

  const subjectMeta = await tableBuilderService.getSubjectMeta(
    query.subjectId,
    query.releaseId,
  );

  // Just do really basic checks to see if table can be rendered
  // and don't bother checking the query itself.
  // We've decided to go with this approach for the time being
  // as it's quicker/easier to implement than if we were to try
  // to be more helpful and tell the user exactly what is wrong.
  // See: EES-1429
  if (!tableData || tableData.results.length === 0) {
    return {
      initialStep: 3,
      subjectMeta,
    };
  }

  return {
    initialStep: 6,
    subjectMeta,
  };
}
