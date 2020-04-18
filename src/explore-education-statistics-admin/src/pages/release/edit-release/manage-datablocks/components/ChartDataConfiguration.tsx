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
  LineStyle,
} from '@common/modules/charts/types/chart';
import { DataSetConfigurationOptions } from '@common/modules/charts/types/dataSet';
import { colours, symbols } from '@common/modules/charts/util/chartUtils';
import React from 'react';

interface Props {
  configuration: DataSetConfigurationOptions;
  capabilities: ChartCapabilities;
  id: string;
  onConfigurationChange?: (value: DataSetConfigurationOptions) => void;
}

const colourOptions: SelectOption[] = colours.map(color => {
  return {
    label: color,
    value: color,
    style: { backgroundColor: color },
  };
});

const symbolOptions: SelectOption[] = symbols.map<SelectOption>(symbol => ({
  label: symbol,
  value: symbol,
}));

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
  const updateConfig = (newConfig: DataSetConfigurationOptions) => {
    if (onConfigurationChange) {
      onConfigurationChange(newConfig);
    }
  };

  return (
    <div className={styles.chartDataConfiguration}>
      <datalist id={`${id}-colours`}>
        {colourOptions.map(({ value }) => (
          <option key={value} value={value} />
        ))}
      </datalist>
      <FormFieldset id={id} legend="" legendHidden>
        <div className={styles.chartDataLabelConfiguration}>
          <div className={styles.chartDataItem}>
            <FormTextInput
              id={`${id}-label`}
              name="label"
              value={configuration.label}
              label="Label"
              onChange={e =>
                updateConfig({
                  ...configuration,
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
              list={`${id}-colours`}
              onChange={e =>
                updateConfig({
                  ...configuration,
                  colour: e.target.value,
                })
              }
            />
          </div>

          {capabilities.dataSymbols && (
            <div className={styles.chartDataItem}>
              <FormSelect
                id={`${id}-symbol`}
                name="symbol"
                label="Symbol"
                value={configuration.symbol}
                placeholder="none"
                options={symbolOptions}
                onChange={e =>
                  updateConfig({
                    ...configuration,
                    symbol: e.target.value as ChartSymbol,
                  })
                }
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
                options={lineStyleOptions}
                onChange={e =>
                  updateConfig({
                    ...configuration,
                    lineStyle: e.target.value as LineStyle,
                  })
                }
              />
            </div>
          )}
        </div>
      </FormFieldset>
    </div>
  );
};

export default ChartDataConfiguration;
