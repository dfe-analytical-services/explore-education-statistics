import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { DataBlock } from '@common/services/types/blocks';

interface DataBlockWithOptionalResponse extends DataBlock {
  dataBlockResponse?: TableDataResponse;
}

export default function useDataBlockQuery(
  releaseId: string,
  dataBlock: DataBlockWithOptionalResponse | undefined,
): AsyncRetryState<TableDataResponse | undefined> {
  return useAsyncRetry<TableDataResponse | undefined>(async () => {
    if (!dataBlock) {
      return undefined;
    }

    const { dataBlockRequest, dataBlockResponse } = dataBlock;

    if (!dataBlockResponse) {
      return tableBuilderService.getTableData(releaseId, dataBlockRequest);
    }

    return dataBlockResponse;
  }, [dataBlock]);
}
