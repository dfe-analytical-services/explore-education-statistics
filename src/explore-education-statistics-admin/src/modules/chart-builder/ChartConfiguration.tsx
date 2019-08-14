import styles from '@admin/modules/chart-builder/graph-builder.module.scss';
import React from 'react';
import {ChartDefinition} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import {
  FormCheckbox,
  FormGroup,
  FormSelect,
  FormTextInput,
} from '@common/components/form';
import {DataBlockMetadata} from '@common/services/dataBlockService';

interface Props {
  selectedChartType: ChartDefinition;
  chartOptions: ChartOptions;
  onChange: (chartOptions: ChartOptions) => void;
  meta: DataBlockMetadata;
}

export interface ChartOptions {
  stacked: boolean;
  legend: 'none' | 'top' | 'bottom';
  legendHeight: string;
  height?: number;
  width?: number;
  title?: string;
  geographicId?: string;
}

const ChartConfiguration = ({
  chartOptions: initialChartOptions,
  selectedChartType,
  onChange,
  meta,
}: Props) => {

  const [chartOptions, setChartOptions] = React.useState<ChartOptions>(
    initialChartOptions,
  );
  const updateChartOptions = (options: ChartOptions) => {
    setChartOptions(options);
    if (onChange) onChange(options);
  };

  const [chartWidth, setChartWidth] = React.useState(
    `${initialChartOptions.width || ''}`,
  );
  const [chartHeight, setChartHeight] = React.useState(
    `${initialChartOptions.height || ''}`,
  );

  return (
    <div className={styles.axesOptions}>
      <FormGroup className={styles.formGroup}>
        <FormTextInput
          id="chart-title"
          name="chart-title"
          label="Chart title"
          value={chartOptions.title}
          width={20}
          onChange={e => {
            updateChartOptions({
              ...chartOptions,
              title: e.target.value,
            });
          }}
        />
        {selectedChartType.capabilities.stackable && (
          <FormCheckbox
            id="stacked"
            name="stacked"
            label="Stacked bars"
            checked={chartOptions.stacked}
            value="stacked"
            className={styles['margin-top-30']}
            onChange={e => {
              updateChartOptions({
                ...chartOptions,
                stacked: e.target.checked,
              });
            }}
          />
        )}
      </FormGroup>
      <FormGroup className={styles.formGroup}>
        <FormSelect
          id="legend-position"
          name="legend-position"
          value={chartOptions.legend}
          label="Legend Position"
          options={[
            {label: 'Top', value: 'top'},
            {label: 'Bottom', value: 'bottom'},
            {label: 'None', value: 'none'},
          ]}
          order={[]}
          onChange={e => {
            updateChartOptions({
              ...chartOptions,
              // @ts-ignore
              legend: e.target.value,
            });
          }}
        />
        {chartOptions.legend !== 'none' && (
          <FormTextInput
            id="legend-height"
            name="legend-height"
            label="Legend Height (blank for automatic)"
            value={chartOptions.legendHeight}
            width={5}
            onChange={e => {
              updateChartOptions({
                ...chartOptions,
                legendHeight: e.target.value,
              });
            }}
          />
        )}
      </FormGroup>

      {selectedChartType.capabilities.canSize && (
        <FormGroup className={styles.formGroup}>
          <FormTextInput
            id="chart-height"
            name="chart-height"
            label="Chart Height"
            value={chartHeight}
            width={5}
            onChange={e => {
              setChartHeight(e.target.value);
              updateChartOptions({
                ...chartOptions,
                height: parseInt(e.target.value, 10) || undefined,
              });
            }}
          />
          <FormTextInput
            id="chart-width"
            name="chart-width"
            label="Chart Width (blank to fill)"
            value={chartWidth}
            width={5}
            onChange={e => {
              setChartWidth(e.target.value);
              updateChartOptions({
                ...chartOptions,
                width: parseInt(e.target.value, 10) || undefined,
              });
            }}
          />
        </FormGroup>
      )}

      {selectedChartType.type === 'map' && meta.locationsOptions && meta.locationsOptions.length > 0 && (
        <FormGroup className={styles.formGroup}>
          <FormSelect
            id="geographicId"
            label="Select a version of geographical data to use"
            name="geographicId"
            order={[]}
            options={
              [
                {label: "Latest", value: ''},
                ...meta.locationsOptions
              ]
            }
            onChange={e => {
              updateChartOptions({
                ...chartOptions,
                geographicId: e.target.value
              });
            }}
            value={chartOptions.geographicId}
          />
        </FormGroup>
      )}
    </div>
  );
};

export default ChartConfiguration;
