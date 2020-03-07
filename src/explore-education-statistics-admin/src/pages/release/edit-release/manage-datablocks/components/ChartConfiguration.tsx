import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import { ErrorControlProps } from '@admin/validation/withErrorControl';
import {
  FormCheckbox,
  FormGroup,
  FormSelect,
  FormTextInput,
} from '@common/components/form';
import {
  ChartDefinition,
  ChartMetaData,
} from '@common/modules/charts/types/chart';
import { DataBlockResponse } from '@common/services/dataBlockService';
import React, { useState } from 'react';
import InfographicChartForm from './InfographicChartForm';

interface Props {
  selectedChartType: ChartDefinition;
  chartOptions: ChartOptions;
  onChange: (chartOptions: ChartOptions) => void;
  data: DataBlockResponse;
  meta: ChartMetaData;

  onBoundaryLevelChange?: (boundaryLevel: string) => void;
}

export interface ChartOptions {
  stacked: boolean;
  legend: 'none' | 'top' | 'bottom';
  height?: number;
  width?: number;
  title?: string;
  fileId?: string;
  geographicId?: string;
}

const ChartConfiguration = ({
  chartOptions: initialChartOptions,
  selectedChartType,
  onChange,
  meta,
  data,
  onBoundaryLevelChange,
  handleApiErrors,
  handleManualErrors,
}: Props & ErrorControlProps) => {
  const [chartOptions, setChartOptions] = useState<ChartOptions>(
    initialChartOptions,
  );
  const updateChartOptions = (options: ChartOptions) => {
    setChartOptions(options);
    if (onChange) onChange(options);
  };
  const [chartWidth, setChartWidth] = useState(
    `${initialChartOptions.width || ''}`,
  );
  const [chartHeight, setChartHeight] = useState(
    `${initialChartOptions.height || ''}`,
  );
  return (
    <>
      {selectedChartType.type === 'infographic' && (
        <>
          <InfographicChartForm
            releaseId={data.releaseId}
            fileId={chartOptions.fileId || ''}
            onChange={fileId => {
              updateChartOptions({
                ...chartOptions,
                fileId,
              });
            }}
            handleApiErrors={handleApiErrors}
            handleManualErrors={handleManualErrors}
          />
          <hr />
        </>
      )}
      <div>
        <FormGroup>
          <FormTextInput
            id="chart-title"
            name="chart-title"
            label="Chart title"
            value={chartOptions.title}
            percentageWidth="three-quarters"
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
        {selectedChartType.capabilities.hasLegend && (
          <FormGroup className={styles.formGroup}>
            <FormSelect
              id="legend-position"
              name="legend-position"
              value={chartOptions.legend}
              label="Legend position"
              options={[
                { label: 'Top', value: 'top' },
                { label: 'Bottom', value: 'bottom' },
                { label: 'None', value: 'none' },
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
          </FormGroup>
        )}

        {selectedChartType.capabilities.canSize && (
          <>
            <FormGroup className={styles.formGroup}>
              <FormTextInput
                type="number"
                id="chart-height"
                name="chart-height"
                label="Chart height (px)"
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
            </FormGroup>
            <FormGroup className={styles.formGroup}>
              <FormTextInput
                type="number"
                id="chart-width"
                name="chart-width"
                label="Chart width (px)"
                hint="Leave blank to set as full width"
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
          </>
        )}

        {selectedChartType.type === 'map' && meta.boundaryLevels && (
          <>
            {meta.boundaryLevels.length === 1 && (
              <div>
                Using <em>{meta.boundaryLevels[0].label}</em>
              </div>
            )}
            {meta.boundaryLevels.length > 1 && (
              <FormGroup className={styles.formGroup}>
                <FormSelect
                  id="geographicId"
                  label="Select a version of geographical data to use"
                  name="geographicId"
                  order={[]}
                  options={[
                    { label: 'Latest', value: '' },
                    ...meta.boundaryLevels.map(({ id, label }) => ({
                      value: id,
                      label,
                    })),
                  ]}
                  onChange={e => {
                    if (onBoundaryLevelChange) {
                      onBoundaryLevelChange(e.target.value);
                    }

                    updateChartOptions({
                      ...chartOptions,
                      geographicId: e.target.value,
                    });
                  }}
                  value={chartOptions.geographicId}
                />
              </FormGroup>
            )}
          </>
        )}
      </div>
    </>
  );
};

export default ChartConfiguration;
