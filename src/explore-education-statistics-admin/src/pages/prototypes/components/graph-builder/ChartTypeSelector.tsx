import classnames from 'classnames';
import React, { EventHandler } from 'react';
import { ChartDefinition } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import styles from './graph-builder.module.scss';

interface Props {
  chartTypes: ChartDefinition[];
  selectedChartType?: ChartDefinition;
  onSelectChart: (chart: ChartDefinition) => void | undefined;
}

const ChartTypeSelector = ({
  chartTypes,
  onSelectChart,
  selectedChartType,
}: Props) => {
  return (
    <div className={styles.chartContainer}>
      {chartTypes.map(chartType => (
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
          <span className={classnames(styles.img, styles[chartType.type])} />
        </button>
      ))}
    </div>
  );
};

export default ChartTypeSelector;
