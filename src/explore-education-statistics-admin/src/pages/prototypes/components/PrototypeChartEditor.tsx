import React from 'react';

import classnames from 'classnames';

import Details from '@common/components/Details';
import {
  FormSelect,
  FormCheckboxGroup,
  FormFieldset,
} from '@common/components/form';
import styles from './PrototypeChartEditor.module.scss';
import Data, { ChartType } from '../PrototypeData';

const PrototypeChartEditor = (props: {}) => {
  const { chartTypes } = Data;

  const [currentStep, setStep] = React.useState(0);

  const [selectedChartType, selectChartType] = React.useState<
    ChartType | undefined
  >();

  const [selectedIndicators, selectIndicators] = React.useState(
    new Array<string>(),
  );

  const indicatorOptions = Object.values(Data.indicators).map(
    ({ value, label }) => ({ value, label, checked: false }),
  );

  return (
    <div className={styles.editor}>
      <Details summary="Select chart type" open={currentStep >= 0}>
        <div className={styles.chartContainer}>
          {chartTypes.map(chartType => (
            <button
              type="button"
              key={chartType.type}
              className={classnames(styles.chart, {
                [styles.selected]: chartType === selectedChartType,
              })}
              onClick={() => {
                selectChartType(chartType);
                setStep(1);
              }}
            >
              <span className={styles.title}>{chartType.type}</span>
              <span
                className={classnames(styles.img, styles[chartType.type])}
              />
            </button>
          ))}
        </div>
      </Details>

      <Details
        summary="Select data to display on chart"
        open={currentStep >= 1}
      >
        {selectedChartType ? (
          <div>
            <FormCheckboxGroup
              legend="Indicators"
              name="indicators"
              id="indicators"
              options={indicatorOptions}
              value={selectedIndicators}
              onChange={({ target }) => {
                const filtered = selectedIndicators.filter(
                  indicator => indicator !== target.value,
                );

                if (target.checked) {
                  filtered.push(target.value);
                }

                selectIndicators(filtered);

                if (filtered.length > 0) {
                  setStep(2);
                }
              }}
            />
          </div>
        ) : (
          undefined
        )}
      </Details>

      <Details
        summary="Select axis and grouping options"
        open={currentStep >= 2}
      >
        {selectedChartType && selectedIndicators.length > 0 ? (
          <div>
            <FormFieldset legend="Axis" id="axis">
              <FormSelect
                id="axis"
                label={`Select ${selectedChartType.axis}`}
                name="Axis"
                order={[]}
                options={[
                  {
                    value: '',
                    label: `Select a filter to use for the ${
                      selectedChartType.axis
                    }`,
                  },
                  ...Object.values(Data.filters),
                ]}
              />
            </FormFieldset>
          </div>
        ) : (
          undefined
        )}
      </Details>
    </div>
  );
};

export default PrototypeChartEditor;
