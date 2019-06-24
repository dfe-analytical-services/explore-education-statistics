/* eslint-disable @typescript-eslint/no-unused-vars */
import React from 'react';

import classnames from 'classnames';

import Details from '@common/components/Details';
import {
  FormCheckboxGroup,
  FormFieldset,
  FormSelect,
} from '@common/components/form';
import useToggle from '@common/hooks/useToggle';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';
import { Axis } from '@common/services/publicationService';
import DataBlockService, {
  GeographicLevel,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { CheckboxOption } from '@common/components/form/FormCheckboxGroup';
import { Dictionary } from '@common/types';
import { SelectOption } from '@common/components/form/FormSelect';
import styles from './PrototypeChartEditor.module.scss';
import ConstData, { ChartType } from '../PrototypeData';

const PrototypeChartEditor = (props: {}) => {
  const [Data, updateData] = React.useState<DataBlockResponse | undefined>(
    undefined,
  );

  const { chartTypes } = ConstData;

  const [currentStep, setStep] = React.useState(0);

  const [selectedChartType, selectChartType] = React.useState<
    ChartType | undefined
  >();

  const [selectedIndicators, selectIndicators] = React.useState(
    new Array<string>(),
  );

  const [indicatorOptions, setIndicatorOptions] = React.useState<
    CheckboxOption[]
  >([]);

  const [selectedAxes, updateSelectedAxes] = React.useState<string[][]>([]);

  const [useLegend, setUseLegend] = useToggle(false);

  const showMixedUnits = (): boolean => {
    if (Data && selectedIndicators) {
      return (
        new Set(
          selectedIndicators.map(
            selected => Data.metaData.indicators[selected].unit,
          ),
        ).size > 1
      );
    }
    return false;
  };

  interface AllowedFilter extends SelectOption {
    ids: string[];
  }

  const [allowedFilters, setAllowedFilters] = React.useState<AllowedFilter[]>();

  React.useEffect(() => {
    const fetchData = async () => {
      const newData = await DataBlockService.getDataBlockForSubject({
        subjectId: 1,
        startYear: '2012',
        endYear: '2016',
        filters: ['2', '71', '72', '73'],
        geographicLevel: GeographicLevel.National,
        indicators: ['23', '26', '28'],
      });
      updateData(newData);

      const uniqueFilterIds: string[][] = Object.values(
        newData.result.reduce((filterSet, result) => {
          const filterIds = Array.from(result.filters);

          return {
            ...filterSet,
            [filterIds.join('_')]: filterIds,
          };
        }, {}),
      );

      const filters = uniqueFilterIds.map(ids => {
        const allFilters = ids.map(
          (id: string) => newData.metaData.filters[id].label,
        );

        return {
          ids,
          label: allFilters.join(','),
          value: ids.join(','),
        };
      });

      setAllowedFilters(filters);

      setIndicatorOptions(
        Object.values(newData.metaData.indicators).map(({ value, label }) => ({
          value,
          label,
          checked: false,
        })),
      );
    };

    fetchData();
  }, []);

  const isValidChartOptions = () => {
    if (selectedChartType) {
      if (selectedIndicators.length === 0) {
        return false;
      }

      if (selectedChartType.dataOptions.length > 0) {
        return (
          selectedAxes &&
          selectedAxes.length > 0 &&
          selectedAxes[0].length !== 0
        );
      }

      return true;
    }

    return false;
  };

  const renderDataChart = () => {
    if (Data && selectedChartType) {
      const xAxis: Axis = {
        title: '',
        key: [],
      };

      const yAxis: Axis = {
        title: '',
        key: [],
      };

      const mappingFunctions: Dictionary<(value: string[]) => void> = {
        xaxis: (value: string[]) => {
          // @ts-ignore
          return xAxis.key.push(value);
        },
        yaxis: (value: string[]) => {
          // @ts-ignore
          return yAxis.key.push(value);
        },
        geojson: (value: string[]) => {},
      };

      const { dataMapping } = selectedChartType;

      selectedAxes.forEach((axes: string[], index) => {
        const mapping =
          index < dataMapping.length
            ? dataMapping[index]
            : dataMapping[dataMapping.length - 1];

        if (mappingFunctions[mapping.mapTo]) {
          axes.forEach(axis => {
            const indicators = axis.split(',');
            mappingFunctions[mapping.mapTo](indicators);
          });
        }
      });

      return (
        <ChartRenderer
          type={selectedChartType.type}
          data={Data}
          meta={Data.metaData}
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
            {showMixedUnits() && (
              <div>
                Warning: Indicators with different unit types are selected. This
                will cause unexpected results.
              </div>
            )}
          </div>
        )}
      </Details>

      {Data && selectedChartType && selectedChartType.dataOptions.length > 0 && (
        <Details
          summary="Select axis and grouping options"
          open={currentStep >= 2}
        >
          {allowedFilters &&
            selectedIndicators.length > 0 &&
            selectedChartType.dataOptions.map((axis, index) => (
              <div key={axis} className={styles.formselect}>
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
                      ...allowedFilters,
                    ]}
                    multiple
                    onChange={e => {
                      const newAxis = [...selectedAxes];
                      newAxis.splice(
                        index,
                        1,
                        Array.from(e.currentTarget.options)
                          .filter(_ => _.selected)
                          .map(_ => _.value),
                      );

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

          <FormFieldset id="chartoptions" legend="Select chart options">
            options
          </FormFieldset>
        </Details>
      )}
    </div>
  );
};

export default PrototypeChartEditor;
