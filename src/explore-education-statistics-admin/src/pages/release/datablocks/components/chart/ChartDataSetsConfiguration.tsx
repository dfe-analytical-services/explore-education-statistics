import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import styles from '@admin/pages/release/datablocks/components/chart/ChartDataSetsConfiguration.module.scss';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import generateDataSetLabel from '@admin/pages/release/datablocks/components/chart/utils/generateDataSetLabel';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import ErrorSummary from '@common/components/ErrorSummary';
import { FormSelect } from '@common/components/form';
import SubmitError from '@common/components/form/util/SubmitError';
import ModalConfirm from '@common/components/ModalConfirm';
import VisuallyHidden from '@common/components/VisuallyHidden';
import FormProvider from '@common/components/form/FormProvider';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import Form from '@common/components/form/Form';
import ReorderableList from '@common/components/ReorderableList';
import { DataSet } from '@common/modules/charts/types/dataSet';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import WarningMessage from '@common/components/WarningMessage';
import getSelectedDataSets from '@admin/pages/release/datablocks/components/chart/utils/getSelectedDataSets';
import reorder from '@common/utils/reorder';
import difference from 'lodash/difference';
import mapValues from 'lodash/mapValues';
import orderBy from 'lodash/orderBy';
import React, { ReactNode, useEffect, useMemo, useState } from 'react';

const formId = 'chartDataSetsConfigurationForm';

export interface FormValues {
  filters: Dictionary<string>;
  indicator: string;
  location: string;
  timePeriod: string;
}

interface Props {
  buttons?: ReactNode;
  dataSets?: DataSet[];
  dataSetsUnits?: string[];
  meta: FullTableMeta;
  onChange: (dataSets: DataSet[]) => void;
}

const ChartDataSetsConfiguration = ({
  buttons,
  meta,
  dataSets: initialDataSets = [],
  dataSetsUnits = [],
  onChange,
}: Props) => {
  const { forms, updateForm, submitForms } = useChartBuilderFormsContext();
  const [dataSets, setDataSets] = useState<DataSet[]>(initialDataSets);
  const [isReordering, toggleIsReordering] = useToggle(false);

  const indicatorOptions = useMemo(
    () => Object.values(meta.indicators),
    [meta.indicators],
  );
  const locationOptions = useMemo(
    () =>
      meta.locations.map(location => ({
        value: location.id,
        label: location.label,
      })),
    [meta.locations],
  );

  const hasMixedUnits = !dataSetsUnits.every(unit => unit === dataSetsUnits[0]);

  useEffect(() => {
    updateForm({
      formKey: 'dataSets',
      isValid: initialDataSets.length > 0,
    });
  }, [initialDataSets.length, updateForm]);

  const handleSubmit = (values: FormValues) => {
    const selectedDataSets = getSelectedDataSets({
      filters: meta.filters,
      indicatorOptions,
      values,
    });
    selectedDataSets.forEach(newDataSet => {
      if (
        dataSets.find(dataSet => {
          return (
            dataSet.indicator === newDataSet.indicator &&
            difference(dataSet.filters, newDataSet.filters).length === 0 &&
            dataSet.location?.level === newDataSet.location?.level &&
            dataSet.location?.value === newDataSet.location?.value &&
            dataSet.timePeriod === newDataSet.timePeriod
          );
        })
      ) {
        throw new SubmitError(
          'The selected options have already been added to the chart',
        );
      }
    });

    const updatedDataSets = [...dataSets, ...selectedDataSets].map(
      (dataSet, index) => {
        return {
          ...dataSet,
          order: index,
        };
      },
    );
    onChange(updatedDataSets);
    setDataSets(updatedDataSets);
  };

  return (
    <>
      {hasMixedUnits && (
        <WarningMessage>
          Selected data sets have different indicator units.
        </WarningMessage>
      )}
      <FormProvider
        initialValues={{
          filters: mapValues(meta.filters, filterGroup =>
            filterGroup.options.length === 1
              ? filterGroup.options[0].value
              : '',
          ),
          indicator:
            meta.indicators.length === 1 ? meta.indicators[0].value : '',
          location: meta.locations.length === 1 ? meta.locations[0].id : '',
          timePeriod:
            meta.timePeriodRange.length === 1
              ? meta.timePeriodRange[0].value
              : '',
        }}
        validationSchema={Yup.object<FormValues>({
          indicator: Yup.string(),
          filters: Yup.object(mapValues(meta.filters, () => Yup.string())),
          location: Yup.string(),
          timePeriod: Yup.string(),
        })}
      >
        <Form id={formId} onSubmit={handleSubmit}>
          <div className={styles.formSelectRow}>
            {orderBy(Object.entries(meta.filters), ([_, value]) => value.order)
              .filter(([, filters]) => filters.options.length > 1)
              .map(([categoryKey, category]) => (
                <FormFieldSelect
                  key={categoryKey}
                  name={`filters.${categoryKey}`}
                  label={category.legend}
                  formGroupClass={styles.formSelectGroup}
                  className="govuk-!-width-full"
                  placeholder={
                    category.options.length > 1 ? 'All options' : undefined
                  }
                  options={category.options}
                  order={FormSelect.unordered}
                />
              ))}

            {indicatorOptions.length > 1 && (
              <FormFieldSelect<FormValues>
                name="indicator"
                label="Indicator"
                formGroupClass={styles.formSelectGroup}
                className="govuk-!-width-full"
                placeholder="All indicators"
                options={indicatorOptions}
                order={FormSelect.unordered}
              />
            )}

            {locationOptions.length > 1 && (
              <FormFieldSelect<FormValues>
                name="location"
                label="Location"
                formGroupClass={styles.formSelectGroup}
                className="govuk-!-width-full"
                placeholder="All locations"
                options={locationOptions}
              />
            )}

            {meta.timePeriodRange.length > 1 && (
              <FormFieldSelect<FormValues>
                name="timePeriod"
                label="Time period"
                formGroupClass={styles.formSelectGroup}
                className="govuk-!-width-full"
                placeholder="All time periods"
                options={meta.timePeriodRange}
                order={FormSelect.unordered}
              />
            )}
          </div>

          <Button type="submit">Add data set</Button>
        </Form>
      </FormProvider>

      {forms.dataSets &&
        forms.dataSets.submitCount > 0 &&
        dataSets.length === 0 && (
          <ErrorSummary
            title="Cannot save chart"
            errors={[
              {
                id: forms.dataSets.id,
                message: 'One or more data sets are required.',
              },
            ]}
          />
        )}

      {isReordering ? (
        <ReorderableList
          heading="Reorder data sets"
          id="reorder-data-sets"
          list={dataSets.map(dataSet => {
            const expandedDataSet = expandDataSet(dataSet, meta);
            const label = generateDataSetLabel(expandedDataSet);
            const key = generateDataSetKey(dataSet);

            return {
              id: key,
              label,
            };
          })}
          onCancel={() => {
            setDataSets(initialDataSets);
            toggleIsReordering.off();
          }}
          onConfirm={() => {
            onChange(dataSets);
            toggleIsReordering.off();
          }}
          onMoveItem={({ prevIndex, nextIndex }) => {
            const reorderedDataSets = reorder(
              dataSets,
              prevIndex,
              nextIndex,
            ).map((dataSet, index) => {
              return {
                ...dataSet,
                order: index,
              };
            });
            setDataSets(reorderedDataSets);
          }}
          onReverse={() => {
            setDataSets(
              dataSets.toReversed().map((dataSet, index) => {
                return {
                  ...dataSet,
                  order: index,
                };
              }),
            );
          }}
        />
      ) : (
        <table data-testid="chart-data-sets">
          <thead>
            <tr>
              <th>Data set</th>
              <th className="govuk-!-text-align-right">
                {dataSets.length > 0 && (
                  <ModalConfirm
                    title="Remove all data sets"
                    triggerButton={
                      <ButtonText className="govuk-!-margin-bottom-0">
                        Remove all
                        <VisuallyHidden> data sets</VisuallyHidden>
                      </ButtonText>
                    }
                    onConfirm={() => {
                      onChange([]);
                      setDataSets([]);
                    }}
                  >
                    <p>Are you sure you want to remove all data sets?</p>
                  </ModalConfirm>
                )}
              </th>
            </tr>
          </thead>
          <tbody>
            {orderBy(dataSets, 'order').map((dataSet, index) => {
              const expandedDataSet = expandDataSet(dataSet, meta);
              const label = generateDataSetLabel(expandedDataSet);
              const key = generateDataSetKey(dataSet);

              return (
                <tr key={key}>
                  <td>{label}</td>
                  <td className="govuk-!-text-align-right">
                    <ButtonText
                      className="govuk-!-margin-bottom-0"
                      onClick={() => {
                        const nextDataSets = [...dataSets];
                        nextDataSets.splice(index, 1);
                        onChange(nextDataSets);
                        setDataSets(nextDataSets);
                      }}
                    >
                      Remove
                    </ButtonText>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}

      {!isReordering && (
        <div className="dfe-flex dfe-justify-content--space-between">
          <div className="dfe-flex-grow--1">
            <ChartBuilderSaveActions
              formId={formId}
              formKey="dataSets"
              onClick={async () => {
                updateForm({
                  formKey: 'dataSets',
                  submitCount: forms.dataSets
                    ? forms.dataSets.submitCount + 1
                    : 1,
                });

                await submitForms();
              }}
            >
              {buttons}
            </ChartBuilderSaveActions>
          </div>
          {dataSets.length > 1 && (
            <Button
              className="dfe-align-self-end govuk-!-margin-left-2"
              variant="secondary"
              onClick={toggleIsReordering}
            >
              Reorder data sets
            </Button>
          )}
        </div>
      )}
    </>
  );
};

export default ChartDataSetsConfiguration;
