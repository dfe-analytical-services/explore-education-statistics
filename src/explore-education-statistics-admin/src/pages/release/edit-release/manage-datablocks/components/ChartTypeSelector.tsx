import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import { ChartDefinition } from '@common/modules/find-statistics/components/charts/util/chartUtils';
import classnames from 'classnames';
import React from 'react';

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
        <React.Fragment key={chartType.type}>
          {!chartType.requiresGeoJson || geoJsonAvailable ? (
            <button
              type="button"
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
            <div className={styles.chartTypeUnavailable}>
              <span className={styles.title}>
                <strong>{chartType.name}</strong>
              </span>
              <p>This chart type is not available.</p>
              <p>There is no map data for this data block.</p>
            </div>
          )}
        </React.Fragment>
      ))}
    </div>
  );
};

export default ChartTypeSelector;
