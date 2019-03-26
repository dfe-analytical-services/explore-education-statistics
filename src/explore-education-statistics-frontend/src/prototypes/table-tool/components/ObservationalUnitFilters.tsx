import { FormikState } from 'formik';
import React from 'react';
import {
  FormFieldRadioGroup,
  FormFieldSelect,
  FormFieldset,
} from 'src/components/form';
import { MetaSpecification } from 'src/prototypes/table-tool/components/meta/initialSpec';
import { FormValues } from './FiltersForm';

enum LocationLevel {
  National = 'NATIONAL',
  Region = 'REGION',
  Local_Authority = 'LOCAL_AUTHORITY',
}

interface Props {
  form: FormikState<FormValues>;
  specification: MetaSpecification['observationalUnits'];
}

const ObservationalUnitFilters = ({ form, specification }: Props) => {
  return (
    <>
      <FormFieldset
        id="filter-location"
        legend="Location"
        hint="Filter statistics by location"
      >
        <FormFieldRadioGroup
          name="locationLevel"
          options={[
            {
              id: 'locationLevel-national',
              label: 'National',
              value: LocationLevel.National,
            },
            {
              id: 'locationLevel-region',
              label: 'Region',
              value: LocationLevel.Region,
            },
            {
              id: 'locationLevel-localAuthority',
              label: 'Local authority',
              value: LocationLevel.Local_Authority,
            },
          ]}
          id="filter-locationLevel"
        />

        {form.values.locationLevel === LocationLevel.National &&
          specification.country.length > 1 && (
            <FormFieldSelect
              name="country"
              id="filter-country"
              label="Country"
              options={specification.country}
            />
          )}

        {form.values.locationLevel === LocationLevel.Region && (
          <FormFieldSelect
            name="region"
            id="filter-region"
            label="Region"
            options={specification.region}
          />
        )}

        {form.values.locationLevel === LocationLevel.Local_Authority && (
          <FormFieldSelect
            name="localAuthority"
            id="filter-localAuthority"
            label="Local authority"
            options={specification.localAuthority}
          />
        )}
      </FormFieldset>

      <FormFieldset
        id="filter-startEndDates"
        legend={specification.startEndDate.legend}
        hint={specification.startEndDate.hint}
      >
        <FormFieldSelect
          name="startDate"
          id="filter-startDate"
          label="Start date"
          options={specification.startEndDate.options}
        />
        <FormFieldSelect
          name="endDate"
          id="filter-endDate"
          label="End date"
          options={specification.startEndDate.options}
        />
      </FormFieldset>
    </>
  );
};

export default ObservationalUnitFilters;
