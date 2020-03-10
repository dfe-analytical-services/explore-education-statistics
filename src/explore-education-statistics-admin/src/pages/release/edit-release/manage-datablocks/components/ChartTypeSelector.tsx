import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import { ChartDefinition } from '@common/modules/charts/types/chart';
import classnames from 'classnames';
import React from 'react';

interface Props {
  chartDefinitions: ChartDefinition[];
  selectedChartDefinition?: ChartDefinition;
  geoJsonAvailable: boolean;
  onSelectChart: (chart: ChartDefinition) => void | undefined;
}

const ChartTypeSelector = ({
  chartDefinitions,
  selectedChartDefinition,
  geoJsonAvailable,
  onSelectChart,
}: Props) => {
  return (
    <div className={styles.chartContainer}>
      {chartDefinitions.map(definition => (
        <React.Fragment key={definition.type}>
          {!definition.requiresGeoJson || geoJsonAvailable ? (
            <button
              type="button"
              className={classnames(styles.chart, {
                [styles.selected]: definition === selectedChartDefinition,
              })}
              onClick={() => {
                if (onSelectChart) onSelectChart(definition);
              }}
            >
              <span className={styles.title}>{definition.name}</span>
              <span
                className={classnames(styles.img, styles[definition.type])}
              />
            </button>
          ) : (
            <div className={styles.chartTypeUnavailable}>
              <span className={styles.title}>
                <strong>{definition.name}</strong>
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
