import {
  ChartSymbol,
  DataSetConfiguration,
  LineStyle,
} from '@common/services/publicationService';
import * as React from 'react';
import {
  FormFieldset,
  FormSelect,
  FormTextInput,
} from '@common/components/form';
import {
  ChartCapabilities,
  colours,
  symbols,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';

import { SelectOption } from '@common/components/form/FormSelect';

import styles from './graph-builder.module.scss';

interface Props {
  configuration: DataSetConfiguration;
  capabilities: ChartCapabilities;

  onConfigurationChange?: (value: DataSetConfiguration) => void;
}

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

const lineStyleOptions: SelectOption[] = [
  { label: 'Solid', value: 'solid' },
  { label: 'Dashed', value: 'dashed' },
  { label: 'Dotted', value: 'dotted' },
];

const ChartDataConfiguration = ({
  configuration,
  capabilities,
  onConfigurationChange,
}: Props) => {
  const [config, setConfig] = React.useState<DataSetConfiguration>(
    configuration,
  );

  const updateConfig = (newConfig: DataSetConfiguration) => {
    setConfig(newConfig);
    if (onConfigurationChange) onConfigurationChange(newConfig);
  };

  return (
    <div className={styles.chartDataConfiguration}>
      <FormFieldset id={configuration.value} legend="" legendHidden>
        <div className={styles.chartDataLabelConfiguration}>
          <div className={styles.chartDataItem}>
            <FormTextInput
              id="label"
              name="label"
              width={10}
              value={config.label}
              label="Label"
              onChange={e =>
                updateConfig({
                  ...config,
                  label: e.target.value,
                })
              }
            />
          </div>

          <div className={styles.chartDataItem}>
            <FormSelect
              id="colour"
              name="colour"
              label="Colour"
              value={configuration.colour}
              onChange={e =>
                updateConfig({
                  ...config,
                  colour: e.target.value,
                })
              }
              options={colourOptions}
            />
          </div>

          {capabilities.dataSymbols && (
            <div className={styles.chartDataItem}>
              <FormSelect
                id="symbol"
                name="symbol"
                label="Symbol"
                value={configuration.symbol}
                onChange={e =>
                  updateConfig({
                    ...config,
                    symbol: e.target.value as ChartSymbol,
                  })
                }
                options={symbolOptions}
              />
            </div>
          )}

          {capabilities.lineStyle && (
            <div className={styles.chartDataItem}>
              <FormSelect
                id="lineStyle"
                name="lineStyle"
                label="Style"
                value={configuration.lineStyle}
                order={[]}
                onChange={e =>
                  updateConfig({
                    ...config,
                    lineStyle: e.target.value as LineStyle,
                  })
                }
                options={lineStyleOptions}
              />
            </div>
          )}
        </div>
      </FormFieldset>
    </div>
  );
};

export default ChartDataConfiguration;
