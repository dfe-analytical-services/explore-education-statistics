import styles from '@admin/modules/chart-builder/graph-builder.module.scss';
import React from 'react';
import {ChartDefinition} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import {
  FormCheckbox,
  FormGroup,
  FormSelect,
  FormTextInput,
} from '@common/components/form';

interface Props {
  selectedChartType: ChartDefinition;
  chartOptions: ChartOptions;
  onChange: (chartOptions: ChartOptions) => void;
}

export interface ChartOptions {
  stacked: boolean;
  legend: 'none' | 'top' | 'bottom';
  legendHeight: string;
  height?: number;
  width?: number;
  title?: string;
  fileId?: string;
}

const ChartConfiguration = (props: Props) => {
  const {
    chartOptions: initialChartOptions,
    selectedChartType,
    onChange,
  } = props;

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
    <>
      {selectedChartType.type === 'infographic' && (
        <>
          <FormGroup>
            <FormTextInput
              id="infographic-fileid"
              name="infographic-fileid"
              label="Select the file to show"
              value={chartOptions.fileId}
              width={5}
              onChange={e => {
                updateChartOptions({
                  ...chartOptions,
                  fileId: e.target.value
                });
              }}
            />
          </FormGroup>

          <hr />
        </>
      )}

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


      </div>
    </>
  );
};

export default ChartConfiguration;
