import ChartDataConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDataConfiguration';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { Form, FormFieldSelect, Formik } from '@common/components/form';
import {
  ChartCapabilities,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import {
  DataSetAndConfiguration,
  DeprecatedDataSetConfiguration,
} from '@common/modules/charts/types/dataSet';
import { colours, symbols } from '@common/modules/charts/util/chartUtils';
import { generateDeprecatedDataSetKey } from '@common/modules/charts/util/deprecatedDataSetKey';
import { CategoryFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import difference from 'lodash/difference';
import keyBy from 'lodash/keyBy';
import mapValues from 'lodash/mapValues';
import React, { useMemo, useState } from 'react';
import styles from './ChartDataSelector.module.scss';

interface FormValues {
  filters: Dictionary<string>;
  indicator: string;
}

export interface SelectedData {
  dataSet: {
    indicator: string;
    filters: string[];
  };
  configuration: DeprecatedDataSetConfiguration;
}

interface Props {
  canSaveChart?: boolean;
  chartType: ChartDefinition;
  selectedData?: DataSetAndConfiguration[];
  meta: FullTableMeta;
  capabilities: ChartCapabilities;
  onDataAdded?: (data: SelectedData) => void;
  onDataRemoved?: (data: SelectedData, index: number) => void;
  onDataChanged?: (data: SelectedData[]) => void;
  onSubmit: (data: SelectedData[]) => void;
}

const formId = 'chartDataSelectorForm';

const ChartDataSelector = ({
  canSaveChart,
  meta,
  capabilities,
  selectedData = [],
  onDataRemoved,
  onDataAdded,
  onDataChanged,
  onSubmit,
}: Props) => {
  const indicatorOptions = useMemo(
    () => [
      {
        label: 'Select an indicator...',
        value: '',
      },
      ...Object.values(meta.indicators),
    ],
    [meta.indicators],
  );

  const filtersByValue: Dictionary<CategoryFilter> = useMemo(
    () => keyBy(Object.values(meta.filters).flat(), filter => filter.value),
    [meta.filters],
  );

  const [chartData, setChartData] = useState<DataSetAndConfiguration[]>([
    ...selectedData,
  ]);

  const removeSelected = (selected: SelectedData, index: number) => {
    const newChartData = [...chartData];
    const [removed] = newChartData.splice(index, 1);

    setChartData(newChartData);

    if (onDataRemoved) {
      onDataRemoved(removed, index);
    }
  };

  return (
    <Formik<FormValues>
      initialValues={{
        filters: mapValues(meta.filters, () => ''),
        indicator: '',
      }}
      validationSchema={Yup.object<FormValues>({
        indicator: Yup.string().required('Select an indicator'),
        filters: Yup.object(
          mapValues(meta.filters, (filter, category) =>
            Yup.string().required(`Select a ${category.toLowerCase()}`),
          ),
        ),
      })}
      onSubmit={({ filters, indicator }, form) => {
        const filterOptions = Object.values(filters);

        if (
          chartData.find(({ dataSet }) => {
            return (
              dataSet.indicator === indicator &&
              difference(dataSet.filters, filterOptions).length === 0
            );
          })
        ) {
          throw new Error(
            'The selected options have already been added to the chart',
          );
        }

        const matchingIndicator = meta.indicators.find(
          i => i.value === indicator,
        );
        const name = `${matchingIndicator?.label}${
          filterOptions.length
            ? ` (${filterOptions
                .map(filter => filtersByValue[filter].label)
                .join(', ')})`
            : ''
        }`;

        const dataSet = {
          filters: filterOptions,
          indicator,
        };

        const newChartData = {
          dataSet,
          configuration: {
            name,
            value: generateDeprecatedDataSetKey(dataSet),
            label: name,
            colour: colours[chartData.length % colours.length],
            symbol: symbols[chartData.length % symbols.length],
            unit: matchingIndicator?.unit || '',
          },
        };

        setChartData([...chartData, newChartData]);

        if (onDataAdded) {
          onDataAdded(newChartData);
        }

        form.resetForm();
      }}
      render={form => (
        <>
          <Form {...form} id={formId} showSubmitError>
            <div className="govuk-grid-row">
              {Object.entries(meta.filters).map(([categoryName, filters]) => (
                <div className="govuk-grid-column-one-third" key={categoryName}>
                  <FormFieldSelect
                    id={`${formId}-filters-${categoryName}`}
                    name={`filters.${categoryName}`}
                    label={categoryName}
                    className="govuk-!-width-full"
                    options={[
                      {
                        label: `Select ${categoryName.toLowerCase()}...`,
                        value: '',
                      },
                      ...filters,
                    ]}
                    order={[]}
                  />
                </div>
              ))}
              <div className="govuk-grid-column-one-third">
                <FormFieldSelect
                  id={`${formId}-indicators`}
                  name="indicator"
                  label="Indicator"
                  className="govuk-!-width-full"
                  options={indicatorOptions}
                  order={[]}
                />
              </div>
            </div>

            <Button
              type="submit"
              className="govuk-!-margin-bottom-0 govuk-!-margin-top-6"
            >
              Add data
            </Button>
          </Form>

          {chartData.length > 0 && (
            <>
              <hr />

              {chartData.map((selected, index) => (
                <React.Fragment
                  key={`${
                    selected.dataSet.indicator
                  }_${selected.dataSet.filters.join('_')}`}
                >
                  <div className={styles.selectedData}>
                    <div className={styles.selectedDataRow}>
                      {selected.dataSet.filters.length > 0 && (
                        <div className={styles.selectedDataFilter}>
                          <span>
                            {selected.dataSet.filters
                              .map(filter => filtersByValue[filter].label)
                              .join(', ')}
                          </span>
                        </div>
                      )}

                      <div className={styles.selectedDataIndicator}>
                        {
                          meta.indicators.find(
                            indicator =>
                              indicator.value === selected.dataSet.indicator,
                          )?.label
                        }
                      </div>

                      <div className={styles.selectedDataAction}>
                        <Button
                          type="button"
                          onClick={() => removeSelected(selected, index)}
                          className="govuk-!-margin-bottom-0 govuk-button--secondary"
                        >
                          Remove
                        </Button>
                      </div>
                    </div>
                    <div>
                      <Details
                        summary="Change styling"
                        className="govuk-!-margin-bottom-3 govuk-body-s"
                      >
                        <ChartDataConfiguration
                          configuration={selected.configuration}
                          capabilities={capabilities}
                          id={`${formId}-chartDataConfiguration-${index}`}
                          onConfigurationChange={(
                            value: DeprecatedDataSetConfiguration,
                          ) => {
                            const newData = [...chartData];

                            newData[index] = {
                              ...newData[index],
                              configuration: value,
                            };

                            setChartData(newData);

                            if (onDataChanged) {
                              onDataChanged(newData);
                            }
                          }}
                        />
                      </Details>
                    </div>
                  </div>
                </React.Fragment>
              ))}

              <hr />

              <Button
                disabled={!canSaveChart}
                onClick={() => {
                  onSubmit(chartData);
                }}
              >
                Save chart options
              </Button>
            </>
          )}
        </>
      )}
    />
  );
};

export default ChartDataSelector;
