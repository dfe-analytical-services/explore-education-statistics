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
import {FormSelect} from '@common/components/form';
import {SelectOption} from '@common/components/form/FormSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';


const options: ({ json: string, initialOptions?: Chart } & SelectOption)[] = [

  {
    label: "Regional Absence",
    value: "0",
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
              indicator: '23',
              filters: ['1', '2']
            },
          ],
        },
      },
    }
  },

  {
    label: "Local Authority District",
    value: "1",
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

      title: "Authorised absences for Local Authority Districts of England",

      height: 600,

      labels: {
        '6_2_3_43_____': {
          label: 'Total FSM eligible, authorised absences',
          name: '6_2_3_43_____',
          value: '6_2_3_43_____',
          colour: "#00ffff",
          unit: ""
        },
        '6_2_3_44_____': {
          label: 'Total not FSM eligible, authorised absences',
          name: '6_2_3_44_____',
          value: '6_2_3_44_____',
          colour: "#00ffff",
          unit: ""
        },
        '6_2_3_45_____': {
          label: 'Total FSM unclassified, authorised absences',
          name: '6_2_3_45_____',
          value: '6_2_3_45_____',
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
    }

  }

];


const PrototypeChartTest = () => {

  const [selected, setSelected] = React.useState("0");

  const [request, setRequest] = React.useState<DataBlockRequest>(JSON.parse(options[0].json));

  const [chartBuilderData, setChartBuilderData] = React.useState<DataBlockResponse>();

  const [requestConfiguration, setRequestConfiguration] = React.useState<Chart | undefined>(options[0].initialOptions);
  const [initialConfiguration, setInitialConfiguration] = React.useState<Chart | undefined>();

  React.useEffect(() => {

    // destroy the existing setup before the response completes
    setChartBuilderData(undefined);
    setInitialConfiguration(undefined);

    DataBlockService.getDataBlockForSubject(request).then(response => {
      setChartBuilderData(response);
      setInitialConfiguration(requestConfiguration);
    });
  }, [request, requestConfiguration]);

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

      <FormSelect
        id="requests"
        name="requests"
        label="Choose test request"
        options={options}
        value={selected}
        order={[]}
        onChange={(e) => {
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
