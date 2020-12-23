import styles from '@admin/pages/release/datablocks/components/chart/ChartDefinitionSelector.module.scss';
import ButtonText from '@common/components/ButtonText';
import { infographicBlockDefinition } from '@common/modules/charts/components/InfographicBlock';
import { ChartDefinition } from '@common/modules/charts/types/chart';
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
    <>
      <h3>Choose chart type</h3>

      <div className={styles.buttonContainer}>
        {chartDefinitions
          .filter(definition => {
            if (definition.capabilities.requiresGeoJson) {
              return geoJsonAvailable;
            }

            return true;
          })
          .map(definition => (
            <button
              key={definition.type}
              aria-pressed={selectedChartDefinition === definition}
              type="button"
              className={styles.button}
              onClick={() => {
                onChange(definition);
              }}
            >
              {definition.name}
              <span
                className={styles.image}
                style={{
                  backgroundImage: `url(/assets/images/chart-types/${definition.type}.png)`,
                }}
              />
            </button>
          ))}
      </div>

      <div className="dfe-align--right">
        <ButtonText
          aria-pressed={selectedChartDefinition === infographicBlockDefinition}
          className="govuk-body-s"
          onClick={() => {
            onChange(infographicBlockDefinition);
          }}
        >
          Choose an infographic as alternative
        </ButtonText>
      </div>
    </>
  );
};

export default ChartDefinitionSelector;
