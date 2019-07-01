import {
  DataBlockResponse,
  DataBlockMetadata,
} from '@common/services/dataBlockService';
import {
  ChartDataSet,
  ChartConfigurationOptions,
} from '@common/services/publicationService';
import * as React from 'react';
import { FormFieldset, FormTextInput } from '@common/components/form';

interface Props {
  dataSets: ChartDataSet[];
  data: DataBlockResponse;
  meta: DataBlockMetadata;

  onConfigurationChange?: (configuration: ChartConfigurationOptions) => void;
}

const ChartDataConfiguration = ({
  dataSets,
  // data,
  meta,
  onConfigurationChange,
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

  const [configuration, setConfiguration] = React.useState<
    ChartConfigurationOptions
  >();

  React.useEffect(() => {
    const dataLabels = dataSets.reduce((results, dataset) => {
      const key = `${dataset.indicator}_${dataset.filters &&
        dataset.filters.join('_')}`;
      return {
        ...results,
        [key]:
          (configuration && configuration.dataLabels[key]) ||
          buildConfigItem(dataset, key),
      };
    }, {});

    setConfiguration({
      dataLabels,
    });
  }, [dataSets]);

  React.useEffect(() => {
    if (onConfigurationChange && configuration)
      onConfigurationChange(configuration);
  }, [configuration, onConfigurationChange]);

  return (
    <FormFieldset id="chart-configuration" legend="" legendHidden>
      <p>
        Update the label used for each dataset in the chart from the default
      </p>
      {configuration &&
        Object.entries(configuration.dataLabels).map(([key, entry]) => (
          <FormTextInput
            key={key}
            id={key}
            name={key}
            defaultValue={entry.label}
            onChange={e => {
              configuration.dataLabels[key].label = e.target.value;
              setConfiguration({ ...configuration });
            }}
            label={entry.name}
          />
        ))}
    </FormFieldset>
  );
};

export default ChartDataConfiguration;
