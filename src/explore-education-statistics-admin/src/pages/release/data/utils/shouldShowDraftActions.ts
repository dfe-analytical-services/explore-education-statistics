import { ApiDataSet } from '@admin/services/apiDataSetService';

export default function shouldShowDraftActions(
  isPatch: boolean,
  canUpdateRelease: boolean,
  dataSet?: ApiDataSet,
): boolean {
  if (dataSet?.draftVersion?.status === 'Processing') {
    return false;
  }

  return (
    dataSet?.draftVersion?.status === 'Draft' || (!isPatch && canUpdateRelease)
  );
}
