import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import {ChartRendererProps} from '@common/modules/find-statistics/components/ChartRenderer';
import DataBlockService, {
  DataBlockRequest,
  DataBlockResponse,
  GeographicLevel,
  DataBlockRerequest,
} from '@common/services/dataBlockService';
import React from 'react';
import {ChartType, Chart} from '@common/services/publicationService';

const PrototypeChartTest = () => {

  const [request, setRequest] = React.useState<DataBlockRequest>(JSON.parse(`
{
  "geographicLevel": "Local_Authority_District",
  "subjectId": "1",
  "indicators": [
    "6",
    "7",
    "8",
    "9",
    "10"
  ],
  "filters": [
    "43",
    "44",
    "45"
  ],
  "timePeriod": {
    "startYear": "2009",
    "startCode": "AY",
    "endYear": "2016",
    "endCode": "AY"
  }
}
  `));

  const [chartBuilderData, setChartBuilderData] = React.useState<DataBlockResponse>();

  const [initialConfiguration] = React.useState<Chart>(
    {
      type: 'map' as ChartType,

      height: 600,

      labels: {
        '6_2_3_43_____': {
          label: 'A Label',
          name: '6_2_3_43_____',
          value: 'name',
          colour: "#00ffff",
          unit: ""
        },
      },

      axes: {
        major: {
          type: 'major',
          name: 'major',
          groupBy: 'locations',
          dataSets: [
            {
              indicator: '6',
              filters: ['2', '3', '43'],
            },
          ],
        },
      },
    }
  );

  React.useEffect(() => {
    DataBlockService.getDataBlockForSubject(request).then(response => {
      setChartBuilderData(response);
    });
  }, [request]); // eslint-disable-line

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const onChartSave = (props: ChartRendererProps) => {
    // console.log('Saved ', props);
  };

  const reRequestdata = (reRequest: DataBlockRerequest) => {

    setRequest({
      ...request,
      ...reRequest
    });

  };

  return (
    <PrototypePage wide>
      {chartBuilderData && (
        <ChartBuilder
          data={chartBuilderData}
          onChartSave={onChartSave}
          initialConfiguration={initialConfiguration}
          onRequiresDataUpdate={reRequestdata}
        />
      )}
    </PrototypePage>
  );
};

export default PrototypeChartTest;
