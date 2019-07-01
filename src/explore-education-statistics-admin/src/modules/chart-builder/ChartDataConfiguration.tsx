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

interface Props {
  dataSets: ChartDataSet[];
  data: DataBlockResponse;
  meta: DataBlockMetadata;

  onDataLabelsChange?: (
    dataLabels: Dictionary<DataLabelConfigurationItem>,
  ) => void;
}

const ChartDataConfiguration = ({
  dataSets,
  // data,
  meta,
  onDataLabelsChange,
}: Props) => {
  const buildConfigItem = (dataset: ChartDataSet, key: string) => {
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

  const [dataLabels, setDataLabels] = React.useState<
    Dictionary<DataLabelConfigurationItem>
  >({});

  React.useEffect(() => {
    const dl = dataSets.reduce((results, dataset) => {
      const key = `${dataset.indicator}_${dataset.filters &&
        dataset.filters.join('_')}`;
      return {
        ...results,
        [key]: (dataLabels && dataLabels[key]) || buildConfigItem(dataset, key),
      };
    }, {});

    setDataLabels(dl);
  }, [dataSets]);

  React.useEffect(() => {
    if (onDataLabelsChange && dataLabels) onDataLabelsChange(dataLabels);
  }, [dataLabels, onDataLabelsChange]);

  return (
    <FormFieldset id="chart-configuration" legend="" legendHidden>
      <p>
        Update the label used for each dataset in the chart from the default
      </p>
      {dataLabels &&
        Object.entries(dataLabels).map(([key, entry]) => (
          <FormTextInput
            key={key}
            id={key}
            name={key}
            defaultValue={entry.label}
            onChange={e => {
              dataLabels[key].label = e.target.value;
              setDataLabels({ ...dataLabels });
            }}
            label={entry.name}
          />
        ))}
    </FormFieldset>
  );
};

export default ChartDataConfiguration;
