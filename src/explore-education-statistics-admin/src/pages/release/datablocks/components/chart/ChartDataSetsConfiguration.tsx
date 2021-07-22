import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import styles from '@admin/pages/release/datablocks/components/chart/ChartDataSetsConfiguration.module.scss';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import generateDataSetLabel from '@admin/pages/release/datablocks/components/chart/utils/generateDataSetLabel';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldSelect, FormSelect } from '@common/components/form';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { DataSet } from '@common/modules/charts/types/dataSet';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import WarningMessage from '@common/components/WarningMessage';
import formatSelectedDataSets from '@admin/pages/release/datablocks/components/chart/utils/formatSelectedDataSets';
import { Formik } from 'formik';
import difference from 'lodash/difference';
import mapValues from 'lodash/mapValues';
import React, { ReactNode, useEffect, useMemo } from 'react';

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
  meta: FullTableMeta;
  onChange: (dataSets: DataSet[]) => void;
}

const ChartDataSetsConfiguration = ({
  buttons,
  meta,
  dataSets = [],
  onChange,
}: Props) => {
  const { forms, updateForm, submit } = useChartBuilderFormsContext();

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

  const hasMixedUnits = useMemo(() => {
    const units: string[] = [];
    dataSets.forEach(dataSet => {
      const foundIndicator = meta.indicators.find(
        indicator => indicator.value === dataSet.indicator,
      );
      if (foundIndicator) {
        units.push(foundIndicator.unit);
      }
    });
    return !units.every(unit => unit === units[0]);
  }, [dataSets, meta.indicators]);

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
          const selectedDataSets = formatSelectedDataSets({
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

          if (onChange) {
            onChange([...dataSets, ...selectedDataSets]);
          }
        }}
      >
        {() => (
          <Form id={formId} showSubmitError>
            <div className={styles.formSelectRow}>
              {Object.entries(meta.filters)
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

      {dataSets?.length > 0 && (
        <>
          <table>
            <thead>
              <tr>
                <th>Data set</th>
                <th>
                  <VisuallyHidden>Actions</VisuallyHidden>
                </th>
              </tr>
            </thead>
            <tbody>
              {dataSets.map((dataSet, index) => {
                const expandedDataSet = expandDataSet(dataSet, meta);
                const label = generateDataSetLabel(expandedDataSet);

                return (
                  <tr key={generateDataSetKey(dataSet)}>
                    <td>{label}</td>
                    <td className="dfe-align--right">
                      <ButtonText
                        className="govuk-!-margin-bottom-0"
                        onClick={() => {
                          if (onChange) {
                            const nextDataSets = [...dataSets];
                            nextDataSets.splice(index, 1);

                            onChange(nextDataSets);
                          }
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

          <ChartBuilderSaveActions
            formId={formId}
            formKey="dataSets"
            onClick={() => {
              if (!forms.dataSets) {
                return;
              }

              updateForm({
                formKey: 'dataSets',
                submitCount: forms.dataSets.submitCount + 1,
              });

              submit();
            }}
          >
            {buttons}
          </ChartBuilderSaveActions>
        </>
      )}
    </>
  );
};

export default ChartDataSetsConfiguration;
