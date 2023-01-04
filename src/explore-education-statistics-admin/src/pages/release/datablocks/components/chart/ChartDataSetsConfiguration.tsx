import DroppableArea from '@admin/components/DroppableArea';
import DraggableItem, { DragHandle } from '@admin/components/DraggableItem';
import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import styles from '@admin/pages/release/datablocks/components/chart/ChartDataSetsConfiguration.module.scss';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import generateDataSetLabel from '@admin/pages/release/datablocks/components/chart/utils/generateDataSetLabel';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import ErrorSummary from '@common/components/ErrorSummary';
import { Form, FormFieldSelect, FormSelect } from '@common/components/form';
import ModalConfirm from '@common/components/ModalConfirm';
import VisuallyHidden from '@common/components/VisuallyHidden';
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
import { Formik } from 'formik';
import difference from 'lodash/difference';
import mapValues from 'lodash/mapValues';
import orderBy from 'lodash/orderBy';
import React, { ReactNode, useEffect, useMemo } from 'react';
import { DragDropContext, Droppable } from 'react-beautiful-dnd';
import classNames from 'classnames';

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
  dataSets = [],
  dataSetsUnits = [],
  onChange,
}: Props) => {
  const { forms, updateForm, submitForms } = useChartBuilderFormsContext();
  const [isReordering, toggleIsReordering] = useToggle(false);
  const [showConfirmModal, toggleConfirmModal] = useToggle(false);

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

  const hasMixedUnits = !dataSetsUnits.every(unit => unit === dataSetsUnits[0]);

  useEffect(() => {
    updateForm({
      formKey: 'dataSets',
      isValid: dataSets.length > 0,
    });
  }, [dataSets.length, updateForm]);

  return (
    <>
      {hasMixedUnits && (
        <WarningMessage>
          Selected data sets have different indicator units.
        </WarningMessage>
      )}
      <Formik<FormValues>
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
        validateOnBlur={false}
        validateOnChange={false}
        validationSchema={Yup.object<FormValues>({
          indicator: Yup.string(),
          filters: Yup.object(mapValues(meta.filters, () => Yup.string())),
          location: Yup.string(),
          timePeriod: Yup.string(),
        })}
        onSubmit={values => {
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
                  difference(dataSet.filters, newDataSet.filters).length ===
                    0 &&
                  dataSet.location?.level === newDataSet.location?.level &&
                  dataSet.location?.value === newDataSet.location?.value &&
                  dataSet.timePeriod === newDataSet.timePeriod
                );
              })
            ) {
              throw new Error(
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
        }}
      >
        {() => (
          <Form id={formId} showSubmitError>
            <div className={styles.formSelectRow}>
              {orderBy(
                Object.entries(meta.filters),
                ([_, value]) => value.order,
              )
                .filter(([, filters]) => filters.options.length > 1)
                .map(([categoryName, filters]) => (
                  <FormFieldSelect
                    key={categoryName}
                    name={`filters.${categoryName}`}
                    label={categoryName}
                    formGroupClass={styles.formSelectGroup}
                    className="govuk-!-width-full"
                    placeholder={
                      filters.options.length > 1 ? 'All options' : undefined
                    }
                    options={filters.options}
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
        )}
      </Formik>

      {forms.dataSets &&
        forms.dataSets.submitCount > 0 &&
        dataSets.length === 0 && (
          <ErrorSummary
            title="Cannot save chart"
            id={`${formId}-dataSets-errorSummary`}
            errors={[
              {
                id: forms.dataSets.id,
                message: 'One or more data sets are required.',
              },
            ]}
          />
        )}

      <DragDropContext
        onDragEnd={result => {
          if (!result.destination) {
            return;
          }
          const reorderedDataSets = reorder(
            dataSets,
            result.source.index,
            result.destination.index,
          ).map((dataSet, index) => {
            return {
              ...dataSet,
              order: index,
            };
          });
          onChange(reorderedDataSets);
        }}
      >
        <Droppable droppableId="dataSets" isDropDisabled={!isReordering}>
          {(droppableProvided, droppableSnapshot) => (
            <table data-testid="chart-data-sets">
              <thead>
                <tr>
                  <th>Data set</th>
                  {!isReordering && (
                    <th className="dfe-align--right">
                      {dataSets.length > 0 && (
                        <ButtonText
                          className="govuk-!-margin-bottom-0"
                          onClick={toggleConfirmModal.on}
                        >
                          Remove all<VisuallyHidden> data sets</VisuallyHidden>
                        </ButtonText>
                      )}
                    </th>
                  )}
                </tr>
              </thead>
              <DroppableArea
                droppableProvided={droppableProvided}
                droppableSnapshot={droppableSnapshot}
                tag="tbody"
              >
                {orderBy(dataSets, 'order').map((dataSet, index) => {
                  const expandedDataSet = expandDataSet(dataSet, meta);
                  const label = generateDataSetLabel(expandedDataSet);
                  const key = generateDataSetKey(dataSet);

                  return (
                    <DraggableItem
                      hideDragHandle
                      id={key}
                      index={index}
                      isReordering={isReordering}
                      key={key}
                      tag="tr"
                    >
                      <td
                        className={classNames({
                          [styles.labelReordering]: isReordering,
                        })}
                      >
                        {label}

                        {isReordering && (
                          <DragHandle className={styles.dragHandle} />
                        )}
                      </td>
                      {!isReordering && (
                        <td className="dfe-align--right">
                          <ButtonText
                            className="govuk-!-margin-bottom-0"
                            onClick={() => {
                              const nextDataSets = [...dataSets];
                              nextDataSets.splice(index, 1);
                              onChange(nextDataSets);
                            }}
                          >
                            Remove
                          </ButtonText>
                        </td>
                      )}
                    </DraggableItem>
                  );
                })}
              </DroppableArea>
            </table>
          )}
        </Droppable>
      </DragDropContext>

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
            {isReordering ? 'Finish reordering' : 'Reorder data sets'}
          </Button>
        )}
      </div>
      <ModalConfirm
        title="Remove all data sets"
        open={showConfirmModal}
        onConfirm={() => {
          onChange([]);
          toggleConfirmModal.off();
        }}
        onExit={toggleConfirmModal.off}
        onCancel={toggleConfirmModal.off}
      >
        <p>Are you sure you want to remove all data sets?</p>
      </ModalConfirm>
    </>
  );
};

export default ChartDataSetsConfiguration;
