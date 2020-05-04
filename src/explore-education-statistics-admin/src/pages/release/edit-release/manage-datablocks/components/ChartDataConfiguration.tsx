import styles from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDataConfiguration.module.scss';
import {
  FormFieldset,
  FormSelect,
  FormTextInput,
} from '@common/components/form';
import FormColourInput from '@common/components/form/FormColourInput';
import { SelectOption } from '@common/components/form/FormSelect';
import {
  ChartCapabilities,
  ChartSymbol,
  LineStyle,
} from '@common/modules/charts/types/chart';
import {
  DataSetConfiguration,
  DataSetConfigurationOptions,
} from '@common/modules/charts/types/dataSet';
import { colours, symbols } from '@common/modules/charts/util/chartUtils';
import React from 'react';

interface Props {
  capabilities: ChartCapabilities;
  dataSet: DataSetConfiguration;
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
  capabilities,
  dataSet,
  id,
  onConfigurationChange,
}: Props) => {
  const { config } = dataSet;

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
          {dataSet.timePeriod && dataSet.location && (
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
          )}

          <div className={styles.chartDataItem}>
            <FormColourInput
              id={`${id}-colour`}
              name="colour"
              label="Colour"
              value={config.colour}
              list={`${id}-colours`}
              onChange={e =>
                updateConfig({
                  ...config,
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
                value={config.symbol}
                placeholder="none"
                options={symbolOptions}
                onChange={e =>
                  updateConfig({
                    ...config,
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
                value={config.lineStyle}
                order={[]}
                options={lineStyleOptions}
                onChange={e =>
                  updateConfig({
                    ...config,
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
