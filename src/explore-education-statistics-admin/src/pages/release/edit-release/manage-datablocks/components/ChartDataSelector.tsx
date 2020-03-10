import ChartDataConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDataConfiguration';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { Form, FormFieldSelect, Formik } from '@common/components/form';
import Yup from '@common/lib/validation/yup';
import {
  ChartCapabilities,
  ChartDefinition,
  ChartMetaData,
} from '@common/modules/charts/types/chart';
import {
  colours,
  generateKeyFromDataSet,
  pairFiltersByValue,
  symbols,
} from '@common/modules/charts/util/chartUtils';
import { FilterOption } from '@common/modules/table-tool/services/tableBuilderService';
import {
  ChartDataSet,
  DataSetConfiguration,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import difference from 'lodash/difference';
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
  configuration: DataSetConfiguration;
}

interface Props {
  chartType: ChartDefinition;
  selectedData?: ChartDataSetAndConfiguration[];
  metaData: ChartMetaData;
  capabilities: ChartCapabilities;
  onDataAdded?: (data: SelectedData) => void;
  onDataRemoved?: (data: SelectedData, index: number) => void;
  onDataChanged?: (data: SelectedData[]) => void;
  onSubmit: (data: SelectedData[]) => void;
}

export interface ChartDataSetAndConfiguration {
  dataSet: ChartDataSet;
  configuration: DataSetConfiguration;
}

const formId = 'chartDataSelectorForm';

const ChartDataSelector = ({
  metaData,
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
      ...Object.values(metaData.indicators),
    ],
    [metaData.indicators],
  );

  const filtersByValue: Dictionary<FilterOption> = useMemo(
    () => pairFiltersByValue(metaData.filters),
    [metaData.filters],
  );

  const [chartData, setChartData] = useState<ChartDataSetAndConfiguration[]>([
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
      enableReinitialize
      initialValues={{
        filters: mapValues(metaData.filters, () => ''),
        indicator: '',
      }}
      validationSchema={Yup.object<FormValues>({
        indicator: Yup.string().required('Select an indicator'),
        filters: Yup.object(
          mapValues(metaData.filters, filter =>
            Yup.string().required(`Select a ${filter.legend.toLowerCase()}`),
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

        const name = `${metaData.indicators[indicator].label}${
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
            value: generateKeyFromDataSet(dataSet),
            label: name,
            colour: colours[chartData.length % colours.length],
            symbol: symbols[chartData.length % symbols.length],
            unit: metaData.indicators[indicator].unit || '',
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
              {Object.entries(metaData.filters).map(([filterKey, filter]) => (
                <div className="govuk-grid-column-one-third" key={filterKey}>
                  <FormFieldSelect
                    id={`${formId}-filters-${filterKey}`}
                    name={`filters.${filterKey}`}
                    label={filter.legend}
                    className="govuk-!-width-full"
                    options={[
                      {
                        label: `Select ${filter.legend.toLowerCase()}...`,
                        value: '',
                      },
                      ...Object.values(filter.options).flatMap(
                        opt => opt.options,
                      ),
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
                        {metaData.indicators[selected.dataSet.indicator].label}
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
                            value: DataSetConfiguration,
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
            </>
          )}

          <hr />

          <Button
            onClick={() => {
              onSubmit(selectedData);
            }}
          >
            Save chart options
          </Button>
        </>
      )}
    />
  );
};

export default ChartDataSelector;
