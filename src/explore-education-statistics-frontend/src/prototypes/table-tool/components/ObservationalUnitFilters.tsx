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
  National = 'national',
  Region = 'region',
  Local_Authority = 'localAuthority',
  School = 'school',
}

const defaultLocationLevelOptions = [
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
  {
    id: 'locationLevel-school',
    label: 'School',
    value: LocationLevel.School,
  },
];

interface Props {
  form: FormikState<FormValues>;
  publicationSubject: MetaSpecification['publicationSubject'][0];
  specification: MetaSpecification;
}

const ObservationalUnitFilters = ({
  form,
  publicationSubject,
  specification,
}: Props) => {
  const locationLevelOptions = defaultLocationLevelOptions.filter(
    option =>
      publicationSubject.supports.observationalUnits.location.indexOf(
        option.value,
      ) > -1,
  );

  const locationSpecification = specification.observationalUnits.location;

  return (
    <>
      {locationLevelOptions.length > 1 && (
        <FormFieldset
          id="filter-location"
          legend="Location"
          hint="Filter statistics by location"
        >
          <FormFieldRadioGroup
            name="location.level"
            options={locationLevelOptions}
            id="filter-locationLevel"
          />

          {form.values.location.level === LocationLevel.National &&
            locationSpecification.national.length > 1 && (
              <FormFieldSelect
                name="location.country"
                id="filter-country"
                label="Country"
                options={locationSpecification.national}
              />
            )}

          {form.values.location.level === LocationLevel.Region && (
            <FormFieldSelect
              name="location.region"
              id="filter-region"
              label="Region"
              options={[
                {
                  text: 'Select an option',
                  value: '',
                },
                ...locationSpecification.region,
              ]}
            />
          )}

          {form.values.location.level === LocationLevel.Local_Authority && (
            <FormFieldSelect
              name="location.localAuthority"
              id="filter-localAuthority"
              label="Local authority"
              options={[
                {
                  text: 'Select an option',
                  value: '',
                },
                ...locationSpecification.localAuthority,
              ]}
            />
          )}
        </FormFieldset>
      )}

      <FormFieldset
        id="filter-startEndDates"
        legend={specification.observationalUnits.startEndDate.legend}
        hint={specification.observationalUnits.startEndDate.hint}
      >
        <FormFieldSelect
          name="startDate"
          id="filter-startDate"
          label="Start date"
          options={specification.observationalUnits.startEndDate.options}
        />
        <FormFieldSelect
          name="endDate"
          id="filter-endDate"
          label="End date"
          options={specification.observationalUnits.startEndDate.options}
        />
      </FormFieldset>
    </>
  );
};

export default ObservationalUnitFilters;
