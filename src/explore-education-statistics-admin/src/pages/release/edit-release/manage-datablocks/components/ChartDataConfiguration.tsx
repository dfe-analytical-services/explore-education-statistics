import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import {
  FormFieldset,
  FormSelect,
  FormTextInput,
} from '@common/components/form';

import { SelectOption } from '@common/components/form/FormSelect';
import {
  ChartCapabilities,
  ChartSymbol,
  DataSetConfiguration,
  LineStyle,
} from '@common/modules/charts/types/chart';
import { colours, symbols } from '@common/modules/charts/util/chartUtils';
import React, { useState } from 'react';

interface Props {
  configuration: DataSetConfiguration;
  capabilities: ChartCapabilities;
  id: string;
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
  id,
  onConfigurationChange,
}: Props) => {
  const [config, setConfig] = useState<DataSetConfiguration>(configuration);

  const updateConfig = (newConfig: DataSetConfiguration) => {
    setConfig(newConfig);
    if (onConfigurationChange) onConfigurationChange(newConfig);
  };

  return (
    <div className={styles.chartDataConfiguration}>
      <datalist id="chartdataconfiguration_colours">
        {colourOptions.map(({ value }) => (
          <option key={value} value={value} />
        ))}
      </datalist>
      <FormFieldset id={configuration.value} legend="" legendHidden>
        <div className={styles.chartDataLabelConfiguration}>
          <div className={styles.chartDataItem}>
            <FormTextInput
              id={`${id}-label`}
              name="label"
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
            <FormTextInput
              id={`${id}-colour`}
              name="colour"
              label="Colour"
              type="color"
              value={configuration.colour}
              onChange={e =>
                updateConfig({
                  ...config,
                  colour: e.target.value,
                })
              }
              list="chartdataconfiguration_colours"
            />
          </div>

          {capabilities.dataSymbols && (
            <div className={styles.chartDataItem}>
              <FormSelect
                id={`${id}-symbol`}
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
                id={`${id}-lineStyle`}
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
