import { ChartBuilderForm } from '@admin/pages/release/datablocks/components/ChartBuilder';
import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/ChartBuilderSaveActions';
import styles from '@admin/pages/release/datablocks/components/ChartDataSetsConfiguration.module.scss';
import ChartDataSetsAddForm from '@admin/pages/release/datablocks/components/ChartDataSetsAddForm';
import { FormState } from '@admin/pages/release/datablocks/reducers/chartBuilderReducer';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import Effect from '@common/components/Effect';
import {
  Form,
  FormFieldSelect,
  FormFieldset,
  FormFieldTextInput,
} from '@common/components/form';
import FormFieldColourInput from '@common/components/form/FormFieldColourInput';
import { SelectOption } from '@common/components/form/FormSelect';
import { ChartDefinition } from '@common/modules/charts/types/chart';
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
import { Formik } from 'formik';
import difference from 'lodash/difference';
import upperFirst from 'lodash/upperFirst';
import React, { ReactNode, useState } from 'react';

const symbolOptions: SelectOption[] = symbols.map<SelectOption>(symbol => ({
  label: upperFirst(symbol),
  value: symbol,
}));

const lineStyleOptions: SelectOption[] = [
  { label: 'Solid', value: 'solid' },
  { label: 'Dashed', value: 'dashed' },
  { label: 'Dotted', value: 'dotted' },
];

interface FormValues {
  dataSets: DataSetConfiguration[];
}

interface Props {
  buttons?: ReactNode;
  canSaveChart: boolean;
  dataSets?: DataSetConfiguration[];
  definition: ChartDefinition;
  forms: Dictionary<ChartBuilderForm>;
  isSaving?: boolean;
  meta: FullTableMeta;
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

const formId = 'chartDataSetsConfigurationForm';

const ChartDataSetsConfiguration = ({
  buttons,
  canSaveChart,
  isSaving,
  forms,
  meta,
  dataSets = [],
  definition,
  onDataRemoved,
  onDataAdded,
  onDataChanged,
  onFormStateChange,
  onSubmit,
}: Props) => {
  const { capabilities } = definition;

  const [dataSetConfigs, setDataSetConfigs] = useState<DataSetConfiguration[]>([
    ...dataSets,
  ]);

  const removeSelected = (selected: DataSetConfiguration, index: number) => {
    const newDataSets = [...dataSetConfigs];
    const [removed] = newDataSets.splice(index, 1);

    setDataSetConfigs(newDataSets);

    if (onDataRemoved) {
      onDataRemoved(removed, index);
    }
  };

  return (
    <>
      <ChartDataSetsAddForm
        meta={meta}
        onSubmit={values => {
          const { indicator } = values;
          const filters = Object.values(values.filters);

          // Convert empty strings from form values to undefined
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
      />

      <hr />

      <Formik<FormValues>
        enableReinitialize
        initialValues={{
          dataSets: dataSetConfigs,
        }}
        onSubmit={values => {
          if (canSaveChart) {
            onSubmit(values.dataSets);
          }
        }}
      >
        {form => (
          <Form id={formId}>
            <Effect
              value={{
                form: 'data',
                isValid: form.isValid,
                submitCount: form.submitCount,
              }}
              onChange={onFormStateChange}
              onMount={onFormStateChange}
            />
            <Effect value={form.values.dataSets} onChange={onDataChanged} />

            <ul className={styles.dataSets}>
              {dataSetConfigs.map((dataSet, index) => {
                const id = `${formId}-dataSet-${index}`;

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
                      <FormFieldset
                        id={id}
                        legend="Styling options"
                        legendHidden
                      >
                        <div className={styles.configuration}>
                          {dataSet.timePeriod && dataSet.location && (
                            <div className={styles.labelInput}>
                              <FormFieldTextInput
                                id={`${id}-label`}
                                name={`dataSets[${index}].config.label`}
                                label="Label"
                                formGroup={false}
                              />
                            </div>
                          )}

                          <div className={styles.colourInput}>
                            <FormFieldColourInput
                              id={`${id}-colour`}
                              name={`dataSets[${index}].config.colour`}
                              label="Colour"
                              colours={colours}
                              formGroup={false}
                            />
                          </div>

                          {capabilities.dataSymbols && (
                            <div className={styles.configurationInput}>
                              <FormFieldSelect
                                id={`${id}-symbol`}
                                name={`dataSets[${index}].config.symbol`}
                                label="Symbol"
                                placeholder="None"
                                formGroup={false}
                                options={symbolOptions}
                              />
                            </div>
                          )}

                          {capabilities.lineStyle && (
                            <div className={styles.configurationInput}>
                              <FormFieldSelect
                                id={`${id}-lineStyle`}
                                name={`dataSets[${index}].config.lineStyle`}
                                label="Style"
                                order={[]}
                                formGroup={false}
                                options={lineStyleOptions}
                              />
                            </div>
                          )}
                        </div>
                      </FormFieldset>
                    </Details>
                  </li>
                );
              })}
            </ul>

            <ChartBuilderSaveActions
              disabled={isSaving}
              formId={formId}
              forms={forms}
              showSubmitError={
                form.isValid && form.submitCount > 0 && !canSaveChart
              }
            >
              {buttons}
            </ChartBuilderSaveActions>
          </Form>
        )}
      </Formik>
    </>
  );
};

export default ChartDataSetsConfiguration;
