import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import { FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { ChartRendererProps } from '@common/modules/find-statistics/components/ChartRenderer';
import DataBlockService, {
  DataBlockRequest,
  DataBlockRerequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { Chart, ChartType } from '@common/services/publicationService';
import React from 'react';

// @ts-ignore
const options: ({ json: string; initialOptions?: Chart } & SelectOption)[] = [
  {
    label: 'Regional Absence',
    value: '0',
    json: `
{
  "subjectId": "1",
  "geographicLevel": "Local_Authority",
  "timePeriod": {
    "startYear": "2012",
    "startCode": "AY",
    "endYear": "2016",
    "endCode": "AY"
  },
  "filters": [
    "1",
    "2"
  ],
  "indicators": [
    "23",
    "26",
    "28"
  ],
  "country": null,
  "localAuthority": null,
  "region": null
}
`,
    initialOptions: undefined && {
      type: 'map' as ChartType,

      height: 600,

      labels: {
        '23_1_2_____': {
          label: 'Total unauthorised absence rate',
          name: '23_1_2_____',
          value: 'erm',
          colour: '#00ffff',
          unit: '',
        },
      },

      axes: {
        major: {
          type: 'major',
          name: 'major',
          groupBy: 'locations',
          dataSets: [
            {
              indicator: '23',
              filters: ['1', '2'],
            },
          ],
        },
      },
    },
  },

  {
    label: 'Absences, FSM eligibility, Local Authority Districts',
    value: '1',
    json: `
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
  `,
    initialOptions: {
      type: 'map' as ChartType,

      title:
        'Total authorised absences, based on FSM eligibility, for Local Authority Districts of England',

      height: 600,

      labels: {
        '6_2_3_43_____': {
          label: 'FSM eligible',
          name: '6_2_3_43_____',
          value: '6_2_3_43_____',
          colour: '#00ffff',
          unit: '',
        },
        '6_2_3_44_____': {
          label: 'Not FSM eligible',
          name: '6_2_3_44_____',
          value: '6_2_3_44_____',
          colour: '#00ffff',
          unit: '',
        },
        '6_2_3_45_____': {
          label: 'FSM unclassified',
          name: '6_2_3_45_____',
          value: '6_2_3_45_____',
          colour: '#00ffff',
          unit: '',
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
            {
              indicator: '6',
              filters: ['2', '3', '44'],
            },
            {
              indicator: '6',
              filters: ['2', '3', '45'],
            },
          ],
        },
      },
    },
  },

  {
    label: '2',
    value: '2',
    json: `
{
  "subjectId": 1,
  "geographicLevel": "Local_Authority_District",
  "timePeriod": {
    "startYear": "2016",
    "startCode": "AY",
    "endYear": "2017",
    "endCode": "AY"
  },
  "filters": [
    "1",
    "58"
  ],
  "indicators": [
    "23",
    "26",
    "28"
  ],
  "country": null,
  "localAuthority": null,
  "region": null
}
    
`,
  },
];

const initialState = 0;

const PrototypeChartTest = () => {
  const [selected, setSelected] = React.useState(`${initialState}`);

  const [request, setRequest] = React.useState<DataBlockRequest>(
    JSON.parse(options[initialState].json),
  );

  const [chartBuilderData, setChartBuilderData] = React.useState<
    DataBlockResponse
  >();

  const [requestConfiguration, setRequestConfiguration] = React.useState<
    Chart | undefined
  >(options[initialState].initialOptions);
  const [initialConfiguration, setInitialConfiguration] = React.useState<
    Chart | undefined
  >();

  React.useEffect(() => {
    // destroy the existing setup before the response completes
    setChartBuilderData(undefined);
    setInitialConfiguration(undefined);

    DataBlockService.getDataBlockForSubject(request).then(response => {
      setChartBuilderData(response);
      setInitialConfiguration(requestConfiguration);
    });
  }, [request, requestConfiguration]);

  // eslint-disable-next-line
  const onChartSave = (props: ChartRendererProps) => {
    const {
      title,
      labels,
      axes,
      stacked,
      height,
      fileId,
      geographicId,
      legend,
      legendHeight,
      type,
      width,
    } = props;

    const chart: Chart = {
      title,
      labels,
      axes,
      stacked,
      height,
      fileId,
      geographicId,
      legend,
      legendHeight,
      type: type === 'unknown' ? undefined : type,
      width,
    };

    // eslint-disable-next-line no-console
    console.log('Saved ', JSON.stringify(chart, null, 2));
  };

  const reRequestdata = (reRequest: DataBlockRerequest) => {
    setRequest({
      ...request,
      ...reRequest,
    });
  };

  return (
    <PrototypePage wide>
      <FormSelect
        id="requests"
        name="requests"
        label="Choose test request"
        options={options}
        value={selected}
        order={[]}
        onChange={e => {
          const newSelectedValue = +e.target.value;
          setSelected(e.target.value);
          setRequest(JSON.parse(options[newSelectedValue].json));
          setRequestConfiguration(options[newSelectedValue].initialOptions);
        }}
      />

      {chartBuilderData ? (
        <ChartBuilder
          data={chartBuilderData}
          onChartSave={onChartSave}
          initialConfiguration={initialConfiguration}
          onRequiresDataUpdate={reRequestdata}
        />
      ) : (
        <LoadingSpinner />
      )}
    </PrototypePage>
  );
};

export default PrototypeChartTest;
