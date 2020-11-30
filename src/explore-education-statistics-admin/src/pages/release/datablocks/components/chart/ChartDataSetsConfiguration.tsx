import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import styles from '@admin/pages/release/datablocks/components/chart/ChartDataSetsConfiguration.module.scss';
import { ChartBuilderForms } from '@admin/pages/release/datablocks/components/chart/types/chartBuilderForms';
import generateDataSetLabel from '@admin/pages/release/datablocks/components/chart/utils/generateDataSetLabel';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldSelect, FormSelect } from '@common/components/form';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { DataSet } from '@common/modules/charts/types/dataSet';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import difference from 'lodash/difference';
import mapValues from 'lodash/mapValues';
import React, { ReactNode, useMemo } from 'react';

const formId = 'chartDataSetsConfigurationForm';

interface FormValues {
  filters: Dictionary<string>;
  indicator: string;
  location: string;
  timePeriod: string;
}

interface Props {
  buttons?: ReactNode;
  dataSets?: DataSet[];
  forms: ChartBuilderForms;
  isSaving?: boolean;
  meta: FullTableMeta;
  onChange: (dataSets: DataSet[]) => void;
  onSubmit: () => void;
}

const ChartDataSetsConfiguration = ({
  buttons,
  isSaving,
  forms,
  meta,
  dataSets = [],
  onChange,
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

  return (
    <>
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
          indicator: Yup.string().required('Choose indicator'),
          filters: Yup.object(
            mapValues(meta.filters, (filter, category) =>
              Yup.string().required(`Choose ${category.toLowerCase()}`),
            ),
          ),
          location: Yup.string(),
          timePeriod: Yup.string(),
        })}
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
            dataSets.find(dataSet => {
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

          if (onChange) {
            const dataSet: DataSet = {
              filters,
              indicator,
              location,
              timePeriod,
            };

            onChange([...dataSets, dataSet]);
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
                    id={`${formId}-filters${categoryName}`}
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
                  placeholder="All locations"
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
            disabled={isSaving}
            formId={formId}
            forms={forms}
            onClick={() => {
              onSubmit();
            }}
            showSubmitError={forms.data.submitCount > 0}
          >
            {buttons}
          </ChartBuilderSaveActions>
        </>
      )}
    </>
  );
};

export default ChartDataSetsConfiguration;
