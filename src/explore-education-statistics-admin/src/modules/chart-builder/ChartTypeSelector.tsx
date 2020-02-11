import classnames from 'classnames';
import React from 'react';
import { ChartDefinition } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import styles from './graph-builder.module.scss';

interface Props {
  chartTypes: ChartDefinition[];
  selectedChartType?: ChartDefinition;
  onSelectChart: (chart: ChartDefinition) => void | undefined;
  geoJsonAvailable: boolean;
}

const ChartTypeSelector = ({
  chartTypes,
  onSelectChart,
  selectedChartType,
  geoJsonAvailable,
}: Props) => {
  return (
    <div className={styles.chartContainer}>
      {chartTypes.map(chartType => (
        <>
          {!chartType.requiresGeoJson || geoJsonAvailable ? (
            <button
              type="button"
              key={chartType.type}
              className={classnames(styles.chart, {
                [styles.selected]: chartType === selectedChartType,
              })}
              onClick={() => {
                if (onSelectChart) onSelectChart(chartType);
              }}
            >
              <span className={styles.title}>{chartType.name}</span>
              <span
                className={classnames(styles.img, styles[chartType.type])}
              />
            </button>
          ) : (
            <div className="chartTypeUnavailable">
              <p>{chartType.name}</p>
              <p>This chart type is not available.</p>
              <p>There is no map data for this data block.</p>
            </div>
          )}
        </>
      ))}
    </div>
  );
};

export default ChartTypeSelector;
