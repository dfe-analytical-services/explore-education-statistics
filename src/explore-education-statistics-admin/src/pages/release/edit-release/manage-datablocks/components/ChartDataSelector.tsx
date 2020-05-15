import ChartBuilderSaveButton from '@admin/pages/release/edit-release/manage-datablocks/components/ChartBuilderSaveButton';
import ChartDataConfiguration from '@admin/pages/release/edit-release/manage-datablocks/components/ChartDataConfiguration';
import { FormState } from '@admin/pages/release/edit-release/manage-datablocks/reducers/chartBuilderReducer';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { Form, FormFieldSelect } from '@common/components/form';
import {
  ChartCapabilities,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import {
  DataSet,
  DataSetConfiguration,
} from '@common/modules/charts/types/dataSet';
import { colours, symbols } from '@common/modules/charts/util/chartUtils';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import generateDefaultDataSetLabel from '@common/modules/charts/util/generateDefaultDataSetLabel';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import difference from 'lodash/difference';
import mapValues from 'lodash/mapValues';
import React, { ReactNode, useEffect, useMemo, useState } from 'react';
import styles from './ChartDataSelector.module.scss';

interface FormValues {
  filters: Dictionary<string>;
  indicator: string;
  location: string;
  timePeriod: string;
}

interface Props {
  buttons?: ReactNode;
  canSaveChart: boolean;
  chartType: ChartDefinition;
  dataSets?: DataSetConfiguration[];
  meta: FullTableMeta;
  capabilities: ChartCapabilities;
  onDataAdded?: (data: DataSetConfiguration) => void;
  onDataRemoved?: (data: DataSetConfiguration, index: number) => void;
  onDataChanged?: (data: DataSetConfiguration[]) => void;
  onFormStateChange: (
    state: {
      form: 'data';
    } & FormState,
  ) => void;
  onSubmit: (data: DataSetConfiguration[]) => void;
}

const formId = 'chartDataSelectorForm';

const ChartDataSelector = ({
  buttons,
  canSaveChart,
  meta,
  capabilities,
  dataSets = [],
  onDataRemoved,
  onDataAdded,
  onDataChanged,
  onFormStateChange,
  onSubmit,
}: Props) => {
  const indicatorOptions = useMemo(() => Object.values(meta.indicators), [
    meta.indicators,
  ]);

  const locationOptions = useMemo(
    () =>
      meta.locations.map(location => ({
        value: location.id,
        label: location.label,
      })),
    [meta.locations],
  );

  const [dataSetConfigs, setDataSetConfigs] = useState<DataSetConfiguration[]>([
    ...dataSets,
  ]);

  const [submitCount, setSubmitCount] = useState(0);

  useEffect(() => {
    onFormStateChange({
      form: 'data',
      isValid: true,
      submitCount,
    });
  }, [onFormStateChange, submitCount]);

  const removeSelected = (selected: DataSetConfiguration, index: number) => {
    const newDataSets = [...dataSetConfigs];
    const [removed] = newDataSets.splice(index, 1);

    setDataSetConfigs(newDataSets);

    if (onDataRemoved) {
      onDataRemoved(removed, index);
    }
  };

  return (
    <Formik<FormValues>
      initialValues={{
        filters: mapValues(meta.filters, filterGroup =>
          filterGroup.options.length === 1 ? filterGroup.options[0].value : '',
        ),
        indicator: meta.indicators.length === 1 ? meta.indicators[0].value : '',
        location: meta.locations.length === 1 ? meta.locations[0].id : '',
        timePeriod:
          meta.timePeriodRange.length === 1
            ? meta.timePeriodRange[0].value
            : '',
      }}
      validateOnBlur={false}
      validateOnChange={false}
      validationSchema={Yup.object<FormValues>({
        indicator: Yup.string().required('Select an indicator'),
        filters: Yup.object(
          mapValues(meta.filters, (filter, category) =>
            Yup.string().required(`Select a ${category.toLowerCase()}`),
          ),
        ),
        location: Yup.string(),
        timePeriod: Yup.string(),
      })}
      onSubmit={values => {
        const { indicator } = values;
        const filters = Object.values(values.filters);

        const timePeriod: DataSet['timePeriod'] = values.timePeriod
          ? values.timePeriod
          : undefined;

        const location: DataSet['location'] = values.location
          ? LocationFilter.parseCompositeId(values.location)
          : undefined;

        if (
          dataSetConfigs.find(dataSet => {
            return (
              dataSet.indicator === indicator &&
              difference(dataSet.filters, filters).length === 0 &&
              dataSet.location?.level === location?.level &&
              dataSet.location?.value === location?.value &&
              dataSet.timePeriod === timePeriod
            );
          })
        ) {
          throw new Error(
            'The selected options have already been added to the chart',
          );
        }

        const dataSet: DataSet = {
          filters,
          indicator,
          location,
          timePeriod,
        };

        const expandedDataSet = expandDataSet(dataSet, meta);

        const label = generateDefaultDataSetLabel(expandedDataSet);

        const newDataSetConfig: DataSetConfiguration = {
          ...dataSet,
          config: {
            label,
            colour: colours[dataSetConfigs.length % colours.length],
            symbol: symbols[dataSetConfigs.length % symbols.length],
          },
        };

        setDataSetConfigs([...dataSetConfigs, newDataSetConfig]);

        if (onDataAdded) {
          onDataAdded(newDataSetConfig);
        }
      }}
    >
      {form => (
        <>
          <Form {...form} id={formId} showSubmitError>
            <div className={styles.formSelectRow}>
              {Object.entries(meta.filters)
                .filter(([, filters]) => filters.options.length > 1)
                .map(([categoryName, filters]) => (
                  <FormFieldSelect
                    key={categoryName}
                    id={`${formId}-filters-${categoryName}`}
                    name={`filters.${categoryName}`}
                    label={categoryName}
                    formGroupClass={styles.formSelectGroup}
                    className="govuk-!-width-full"
                    placeholder={
                      filters.options.length > 1
                        ? `Select ${categoryName.toLowerCase()}`
                        : undefined
                    }
                    options={filters.options}
                  />
                ))}

              {indicatorOptions.length > 1 && (
                <FormFieldSelect<FormValues>
                  id={`${formId}-indicator`}
                  name="indicator"
                  label="Indicator"
                  formGroupClass={styles.formSelectGroup}
                  className="govuk-!-width-full"
                  placeholder="Select indicator"
                  options={indicatorOptions}
                />
              )}

              {locationOptions.length > 1 && (
                <FormFieldSelect<FormValues>
                  id={`${formId}-location`}
                  name="location"
                  label="Location"
                  formGroupClass={styles.formSelectGroup}
                  className="govuk-!-width-full"
                  placeholder="Any location"
                  options={locationOptions}
                />
              )}

              {meta.timePeriodRange.length > 1 && (
                <FormFieldSelect<FormValues>
                  id={`${formId}-timePeriod`}
                  name="timePeriod"
                  label="Time period"
                  formGroupClass={styles.formSelectGroup}
                  className="govuk-!-width-full"
                  placeholder="Any time period"
                  options={meta.timePeriodRange}
                  order={[]}
                />
              )}
            </div>

            <Button
              type="submit"
              className="govuk-!-margin-bottom-0 govuk-!-margin-top-6"
            >
              Add data
            </Button>
          </Form>

          {dataSetConfigs.length > 0 && (
            <>
              <hr />

              <ul className={styles.dataSets}>
                {dataSetConfigs.map((dataSet, index) => {
                  const expandedDataSet = expandDataSet(dataSet, meta);
                  const label = generateDefaultDataSetLabel(expandedDataSet);

                  return (
                    <li key={generateDataSetKey(dataSet)}>
                      <div className={styles.dataSetRow}>
                        <span>{label}</span>

                        <div>
                          <Button
                            onClick={() => removeSelected(dataSet, index)}
                            className="govuk-!-margin-bottom-0 govuk-button--secondary"
                          >
                            Remove
                          </Button>
                        </div>
                      </div>

                      <Details
                        summary="Change styling"
                        className="govuk-!-margin-bottom-3"
                      >
                        <ChartDataConfiguration
                          capabilities={capabilities}
                          dataSet={dataSet}
                          id={`${formId}-chartDataConfiguration-${index}`}
                          onConfigurationChange={updatedDataSetConfig => {
                            const nextDataSetConfigs = [...dataSetConfigs];

                            nextDataSetConfigs[index] = {
                              ...nextDataSetConfigs[index],
                              config: updatedDataSetConfig,
                            };

                            setDataSetConfigs(nextDataSetConfigs);

                            if (onDataChanged) {
                              onDataChanged(nextDataSetConfigs);
                            }
                          }}
                        />
                      </Details>
                    </li>
                  );
                })}
              </ul>

              <ChartBuilderSaveButton
                formId={formId}
                showSubmitError={submitCount > 0 && !canSaveChart}
                onClick={() => {
                  setSubmitCount(submitCount + 1);

                  if (canSaveChart) {
                    onSubmit(dataSetConfigs);
                  }
                }}
              />

              {buttons}
            </>
          )}
        </>
      )}
    </Formik>
  );
};

export default ChartDataSelector;
