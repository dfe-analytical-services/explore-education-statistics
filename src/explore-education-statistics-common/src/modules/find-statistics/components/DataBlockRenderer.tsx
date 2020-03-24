import LoadingSpinner from '@common/components/LoadingSpinner';
import ChartRenderer from '@common/modules/charts/components/ChartRenderer';
import { parseMetaData } from '@common/modules/charts/util/chartUtils';
import dataBlockService, {
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { DataBlock } from '@common/services/types/blocks';
import React, { useEffect, useState } from 'react';
import TimePeriodDataTableRenderer from './TimePeriodDataTableRenderer';

interface DataBlockWithOptionalResponse extends DataBlock {
  dataBlockResponse?: DataBlockResponse;
}

interface Props {
  dataBlock: DataBlockWithOptionalResponse;
  type: 'table' | 'chart';
}

/**
 * Component that takes a {@param dataBlock} (with optional response)
 * and a {@param type} to handle rendering of tables/charts from
 * data block data.
 *
 * The component will handle requesting the data block's
 * response if it was not provided.
 */
const DataBlockRenderer = ({ dataBlock, type }: Props) => {
  const [dataBlockResponse, setDataBlockResponse] = useState<
    DataBlockResponse | undefined
  >(dataBlock.dataBlockResponse);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    if (!dataBlockResponse) {
      dataBlockService
        .getDataBlockForSubject(dataBlock.dataBlockRequest)
        .then(setDataBlockResponse)
        .catch(() => {
          setDataBlockResponse(undefined);
          setError('Unable to get data response from the server');
        });
    }
  }, [dataBlock, dataBlockResponse]);

  if (!dataBlockResponse && error) {
    return <>{error}</>;
  }

  if (!dataBlockResponse) {
    return <LoadingSpinner text="Loading data" />;
  }

  if (type === 'table' && dataBlock.tables.length) {
    return (
      <>
        <TimePeriodDataTableRenderer
          heading={dataBlock.heading}
          response={dataBlockResponse}
          tableHeaders={dataBlock.tables[0].tableHeaders}
        />
      </>
    );
  }

  if (type === 'chart' && dataBlock.charts.length) {
    // There is a presumption that the configuration from the API is valid.
    // The data coming from the API is required to be optional for the ChartRenderer
    // But the data for the charts is required. The charts have validation that
    // prevent them from attempting to render.
    return (
      <ChartRenderer
        {...dataBlock.charts[0]}
        data={dataBlockResponse}
        meta={parseMetaData(dataBlockResponse.metaData)}
      />
    );
  }

  return null;
};

export default DataBlockRenderer;
