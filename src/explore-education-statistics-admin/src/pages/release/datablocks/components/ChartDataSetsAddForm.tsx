import styles from '@admin/pages/release/datablocks/components/ChartDataSetsAddForm.module.scss';
import Button from '@common/components/Button';
import { Form, FormFieldSelect } from '@common/components/form';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import mapValues from 'lodash/mapValues';
import React, { useMemo } from 'react';

export interface ChartDataSetsAddFormValues {
  filters: Dictionary<string>;
  indicator: string;
  location: string;
  timePeriod: string;
}

const formId = 'chartDataSetsAddForm';

interface Props {
  meta: FullTableMeta;
  onSubmit: (values: ChartDataSetsAddFormValues) => void;
}

const ChartDataSetsAddForm = ({ meta, onSubmit }: Props) => {
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
    <Formik<ChartDataSetsAddFormValues>
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
      validationSchema={Yup.object<ChartDataSetsAddFormValues>({
        indicator: Yup.string().required('Select an indicator'),
        filters: Yup.object(
          mapValues(meta.filters, (filter, category) =>
            Yup.string().required(`Select a ${category.toLowerCase()}`),
          ),
        ),
        location: Yup.string(),
        timePeriod: Yup.string(),
      })}
      onSubmit={values => onSubmit(values)}
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
              <FormFieldSelect<ChartDataSetsAddFormValues>
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
              <FormFieldSelect<ChartDataSetsAddFormValues>
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
              <FormFieldSelect<ChartDataSetsAddFormValues>
                id={`${formId}-timePeriod`}
                name="timePeriod"
                label="Time period"
                formGroupClass={styles.formSelectGroup}
                className="govuk-!-width-full"
                placeholder="All time periods"
                options={meta.timePeriodRange}
                order={[]}
              />
            )}
          </div>

          <Button type="submit">Add data set</Button>
        </Form>
      )}
    </Formik>
  );
};

export default ChartDataSetsAddForm;
