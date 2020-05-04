import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import { ChartDefinition } from '@common/modules/charts/types/chart';
import classNames from 'classnames';
import React from 'react';

interface Props {
  chartDefinitions: ChartDefinition[];
  selectedChartDefinition?: ChartDefinition;
  geoJsonAvailable: boolean;
  onChange: (chart: ChartDefinition) => void;
}

const ChartDefinitionSelector = ({
  chartDefinitions,
  selectedChartDefinition,
  geoJsonAvailable,
  onChange,
}: Props) => {
  return (
    <div className={styles.chartContainer}>
      {chartDefinitions.map(definition => (
        <React.Fragment key={definition.type}>
          {!definition.capabilities.requiresGeoJson || geoJsonAvailable ? (
            <button
              type="button"
              className={classNames(styles.chart, {
                [styles.selected]: definition === selectedChartDefinition,
              })}
              onClick={() => {
                if (onChange) {
                  onChange(definition);
                }
              }}
            >
              <span className={styles.title}>{definition.name}</span>
              <span
                className={classNames(styles.img, styles[definition.type])}
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

export default ChartDefinitionSelector;
