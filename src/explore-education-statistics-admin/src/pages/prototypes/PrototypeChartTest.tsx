import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import { ChartRendererProps } from '@common/modules/find-statistics/components/ChartRenderer';
import DataBlockService, {
  DataBlockRequest,
  DataBlockResponse,
  GeographicLevel,
} from '@common/services/dataBlockService';
import React from 'react';

const PrototypeChartTest = () => {
  const request: DataBlockRequest = {
    geographicLevel: GeographicLevel.LocalAuthority,
    subjectId: 12,
    indicators: ['160', '158', '156'],
    filters: ['423'],
    timePeriod: {
      startYear: '2009',
      startCode: 'AY',
      endYear: '2016',
      endCode: 'AY',
    },
  };

  const [chartBuilderData, setChartBuilderData] = React.useState<
    DataBlockResponse
  >();

  React.useEffect(() => {
    DataBlockService.getDataBlockForSubject(request).then(response => {
      setChartBuilderData(response);
    });
  }, []); // eslint-disable-line

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const onChartSave = (props: ChartRendererProps) => {
    // console.log('Saved ', props);
  };

  return (
    <PrototypePage wide>
      {chartBuilderData && (
        <ChartBuilder data={chartBuilderData} onChartSave={onChartSave} />
      )}
    </PrototypePage>
  );
};

export default PrototypeChartTest;
