import {
  FormFieldRadioGroup,
  FormFieldSelect,
  FormFieldset,
} from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import { FormikState } from 'formik';
import React from 'react';
import {
  LocationLevel,
  MetaSpecification,
} from 'src/prototypes/table-tool/components/meta/initialSpec';
import { FormValues } from './FiltersForm';

interface Props {
  form: FormikState<FormValues>;
  specification: MetaSpecification;
}

const ObservationalUnitFilters = ({ form, specification }: Props) => {
  const locationSpecification =
    specification.observationalUnits.location.options;

  const timePeriodOptions: SelectOption[] = specification.observationalUnits.timePeriod.options.map(
    option => {
      return {
        label: option.label,
        value: `${option.year}_${option.code}`,
      };
    },
  );

  return (
    <>
      {Object.keys(specification.observationalUnits.location.options).length >
        1 && (
        <FormFieldset
          id="filter-location"
          legend="Location"
          hint="Filter statistics by location"
        >
          <FormFieldRadioGroup
            name="location.level"
            options={Object.entries(locationSpecification).map(
              ([locationLevel, option]) => ({
                id: `filter-locationLevel-${locationLevel}`,
                label: option.label,
                value: locationLevel,
              }),
            )}
            id="filter-locationLevel"
          />

          {form.values.location.level === LocationLevel.National &&
            locationSpecification.national.options.length > 1 && (
              <FormFieldSelect
                name="location.country"
                id="filter-country"
                label="Country"
                options={locationSpecification.national.options}
              />
            )}

          {form.values.location.level === LocationLevel.Region && (
            <FormFieldSelect
              name="location.region"
              id="filter-region"
              label="Region"
              options={[
                {
                  label: 'Select an option',
                  value: '',
                },
                ...locationSpecification.region.options,
              ]}
            />
          )}

          {form.values.location.level === LocationLevel.LocalAuthority && (
            <FormFieldSelect
              name="location.localAuthority"
              id="filter-localAuthority"
              label="Local authority"
              options={[
                {
                  label: 'Select an option',
                  value: '',
                },
                ...locationSpecification.localAuthority.options,
              ]}
            />
          )}
        </FormFieldset>
      )}

      <FormFieldset
        id="filter-startEndDates"
        legend={specification.observationalUnits.timePeriod.legend}
        hint={specification.observationalUnits.timePeriod.hint}
      >
        <FormFieldSelect
          name="timePeriod.start"
          id="filter-timePeriodStart"
          label="Start date"
          options={timePeriodOptions}
        />
        <FormFieldSelect
          name="timePeriod.end"
          id="filter-timePeriodEnd"
          label="End date"
          options={timePeriodOptions}
        />
      </FormFieldset>
    </>
  );
};

export default ObservationalUnitFilters;
