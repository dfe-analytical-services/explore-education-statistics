import { DataBlockMetadata } from '@common/services/dataBlockService';
import {
  ChartDataSet,
  ChartConfiguration,
  ChartSymbol,
} from '@common/services/publicationService';
import * as React from 'react';
import {
  FormFieldset,
  FormTextInput,
  FormSelect,
} from '@common/components/form';
import {
  colours,
  symbols,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';

import { SelectOption } from '@common/components/form/FormSelect';

interface Props {
  configuration: ChartConfiguration;

  onConfigurationChange?: (value: ChartConfiguration) => void;
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

const colourOptions: SelectOption[] = colours.map(color => {
  return {
    label: color,
    value: color,
    style: { backgroundColor: `${color}` },
  };
});

const symbolOptions: SelectOption[] = [
  {
    label: 'none',
    value: '',
  },
  ...symbols.map<SelectOption>(symbol => ({
    label: symbol,
    value: symbol,
  })),
];

const ChartDataConfiguration = ({
  configuration,
  onConfigurationChange,
}: Props) => {
  const [config, setConfig] = React.useState<ChartConfiguration>(configuration);

  const updateConfig = (newConfig: ChartConfiguration) => {
    setConfig(newConfig);
    if (onConfigurationChange) onConfigurationChange(newConfig);
  };

  return (
    <FormFieldset id={configuration.value} legend="" legendHidden>
      <p>{configuration.name}</p>

      <FormTextInput
        id="label"
        name="label"
        value={config.label}
        label="Enter Label"
        onChange={e =>
          updateConfig({
            ...config,
            label: e.target.value,
          })
        }
      />

      <FormSelect
        id="colour"
        name="colour"
        label="Select colour"
        value={configuration.colour}
        onChange={e =>
          updateConfig({
            ...config,
            colour: e.target.value,
          })
        }
        options={colourOptions}
      />

      <FormSelect
        id="symbol"
        name="symbol"
        label="Select symbol"
        value={configuration.symbol}
        onChange={e =>
          updateConfig({
            ...config,
            symbol: e.target.value as ChartSymbol,
          })
        }
        options={symbolOptions}
      />
    </FormFieldset>
  );
};

export default ChartDataConfiguration;
