import React from 'react';

import classnames from 'classnames';

import Details from '@common/components/Details';
import {
  FormSelect,
  FormCheckboxGroup,
  FormFieldset,
} from '@common/components/form';
import useToggle from '@common/hooks/useToggle';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';
import { Axis } from '@common/services/publicationService';
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

  const [selectedAxes, updateSelectedAxes] = React.useState<string[]>([]);

  const [useLegend, setUseLegend] = useToggle(false);

  const isValidChartOptions = () => {
    if (selectedChartType) {
      if (selectedIndicators.length === 0) {
        return false;
      }

      if (selectedChartType.axis.length > 0) {
        return (
          selectedAxes && selectedAxes.length > 0 && selectedAxes[0] !== ''
        );
      }

      return true;
    }

    return false;
  };

  const renderDataChart = () => {
    if (selectedChartType) {
      const xAxis: Axis = {
        title: '',
      };

      const yAxis: Axis = {
        title: '',
      };

      return (
        <ChartRenderer
          type={selectedChartType.type}
          data={Data.responseData}
          meta={Data.responseData.metaData}
          indicators={selectedIndicators}
          xAxis={xAxis}
          yAxis={yAxis}
        />
      );
    }

    return <div>Chart is not set up correctly</div>;
  };

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
        {selectedChartType && (
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
        )}
      </Details>

      {selectedChartType && selectedChartType.axis.length > 0 && (
        <Details
          summary="Select axis and grouping options"
          open={currentStep >= 2}
        >
          {selectedIndicators.length > 0 &&
            selectedChartType.axis.map((axis, index) => (
              <div key={axis}>
                <FormFieldset legend={axis} id={`${axis}_fieldset`}>
                  <FormSelect
                    id={axis}
                    label={`Select a filter to use for the ${axis} ${
                      index === 0 ? '(required)' : '(optional)'
                    }`}
                    name={axis}
                    order={[]}
                    options={[
                      {
                        value: '',
                        label: `Select...`,
                      },
                      ...Object.values(Data.filters),
                    ]}
                    value={selectedAxes[index]}
                    onChange={e => {
                      const newAxis = [...selectedAxes];
                      newAxis.splice(index, 1, e.currentTarget.value);
                      updateSelectedAxes(newAxis);
                    }}
                  />
                </FormFieldset>
              </div>
            ))}
        </Details>
      )}

      {isValidChartOptions() && (
        <Details summary="Preview" open>
          {renderDataChart()}
        </Details>
      )}
    </div>
  );
};

export default PrototypeChartEditor;
