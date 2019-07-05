import {
  DataBlockResponse,
  DataBlockMetadata,
} from '@common/services/dataBlockService';
import {
  ChartDataSet,
  DataLabelConfigurationItem,
} from '@common/services/publicationService';
import * as React from 'react';
import { FormFieldset, FormTextInput } from '@common/components/form';
import { Dictionary } from '@common/types';
import { generateKeyFromDataSet } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { difference } from 'lodash';

interface Props {
  dataSets: ChartDataSet[];
  data: DataBlockResponse;
  meta: DataBlockMetadata;

  onDataLabelsChange?: (
    dataLabels: Dictionary<DataLabelConfigurationItem>,
  ) => void;
}

const buildConfigItem = (
  meta: DataBlockMetadata,
  dataset: ChartDataSet,
  key: string,
) => {
  const filters = (dataset.filters || [])
    .map(f => meta.filters[f].label)
    .join(',');

  return {
    name: `${meta.indicators[dataset.indicator].label} (${filters})`,
    label: `${meta.indicators[dataset.indicator].label} (${filters})`,
    value: key,
    unit: meta.indicators[dataset.indicator].unit,
  };
};

function updateLabels(
  meta: DataBlockMetadata,
  dataSets: ChartDataSet[],
  existingDataLabels: Dictionary<DataLabelConfigurationItem>,
) {
  const newDataLabels = dataSets.reduce((results, dataset) => {
    const key = generateKeyFromDataSet(dataset);
    return {
      ...results,
      [key]: buildConfigItem(meta, dataset, key),
    };
  }, {});

  const newKeys = difference(
    Object.keys(newDataLabels),
    Object.keys(existingDataLabels),
  );
  if (newKeys.length > 0) {
    return {
      ...newDataLabels,
      ...existingDataLabels,
    };
  }

  return existingDataLabels;
}

const ChartDataConfiguration = ({
  dataSets,
  // data,
  meta,
  onDataLabelsChange,
}: Props) => {
  const [dataLabels, setDataLabels] = React.useState<
    Dictionary<DataLabelConfigurationItem>
  >(() => updateLabels(meta, dataSets, {}));

  React.useEffect(() => {
    setDataLabels(updateLabels(meta, dataSets, dataLabels));
  }, [meta, dataSets, dataLabels]);

  React.useEffect(() => {
    if (onDataLabelsChange && dataLabels) onDataLabelsChange(dataLabels);
  }, [dataLabels, onDataLabelsChange]);

  return (
    <FormFieldset id="chart-configuration" legend="" legendHidden>
      <table className="govuk-table">
        <caption>
          Update the label used for each dataset in the chart from the default
        </caption>
        <thead>
          <tr>
            <th>Field</th>
            <th>Options</th>
          </tr>
        </thead>
        <tbody>
          {dataLabels &&
            Object.entries(dataLabels).map(([key, entry]) => (
              <tr key={key}>
                <td>{entry.name}</td>
                <td>
                  <FormTextInput
                    id={key}
                    name={key}
                    defaultValue={entry.label}
                    onChange={e => {
                      dataLabels[key].label = e.target.value;
                      setDataLabels({ ...dataLabels });
                    }}
                    label="label"
                  />
                </td>
              </tr>
            ))}
        </tbody>
      </table>
    </FormFieldset>
  );
};

export default ChartDataConfiguration;
