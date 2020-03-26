import { useErrorControl } from '@common/contexts/ErrorControlContext';
import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import dataBlockService, {
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { DataBlock } from '@common/services/types/blocks';

interface DataBlockWithOptionalResponse extends DataBlock {
  dataBlockResponse?: DataBlockResponse;
}

export default function useDataBlockQuery(
  dataBlock: DataBlockWithOptionalResponse | undefined,
): AsyncRetryState<DataBlockResponse | undefined> {
  const { withoutErrorHandling } = useErrorControl();

  return useAsyncRetry<DataBlockResponse | undefined>(async () => {
    if (!dataBlock) {
      return undefined;
    }

    const { dataBlockRequest, dataBlockResponse } = dataBlock;

    if (!dataBlockResponse) {
      return withoutErrorHandling(() =>
        dataBlockService.getDataBlockForSubject(dataBlockRequest),
      );
    }

    return dataBlockResponse;
  }, [dataBlock]);
}
