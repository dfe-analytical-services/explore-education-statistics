import CollapsibleList from '@common/components/CollapsibleList';
import { Form, FormFieldset } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import {
  FilterOption,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types/util';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import mapValues from 'lodash/mapValues';
import sortBy from 'lodash/sortBy';
import React, { useMemo } from 'react';
import FormFieldCheckboxMenu from './FormFieldCheckboxMenu';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';
import WizardStepEditButton from './WizardStepEditButton';

export interface LocationFormValues {
  locations: Dictionary<string[]>;
}

export type LocationFiltersFormSubmitHandler = (values: {
  locations: Dictionary<string[]>;
}) => void;

const formId = 'locationFiltersForm';

interface Props {
  options: SubjectMeta['locations'];
  initialValues?: Dictionary<string[]>;
  onSubmit: LocationFiltersFormSubmitHandler;
}

const LocationFiltersForm = (props: Props & InjectedWizardProps) => {
  const {
    options,
    onSubmit,
    isActive,
    goToNextStep,
    currentStep,
    stepNumber,
    initialValues = {},
  } = props;

  const formOptions = useMemo(() => options, [options]);
  const stepEnabled = currentStep > stepNumber;
  const stepHeading = useMemo(
    () => (
      <WizardStepHeading {...props} fieldsetHeading stepEnabled={stepEnabled}>
        Choose locations
      </WizardStepHeading>
    ),
    [props, stepEnabled],
  );

  // Automatically select and expand group if only one location available
  const autoSelectLocation =
    Object.entries(formOptions).length === 1 &&
    Object.entries(formOptions)[0][1].options.length === 1;

  const initialFormValues = useMemo(() => {
    return {
      locations: mapValues(options, (levelOptions, level) => {
        const initialLevel = autoSelectLocation
          ? [levelOptions.options[0].value]
          : initialValues[level] ?? [];
        return initialLevel.filter(locationId =>
          levelOptions.options.some(({ value }) => value === locationId),
        );
      }),
    };
  }, [autoSelectLocation, initialValues, options]);

  return (
    <Formik<LocationFormValues>
      enableReinitialize
      initialValues={initialFormValues}
      validateOnBlur={false}
      validationSchema={Yup.object<LocationFormValues>({
        locations: Yup.mixed().test(
          'required',
          'Select at least one location',
          (value: Dictionary<string[]>) =>
            Object.values(value).some(groupOptions => groupOptions.length > 0),
        ),
      })}
      onSubmit={async values => {
        const locations = Object.entries(values.locations).reduce<
          Dictionary<string[]>
        >((acc, [level, levelOptions]) => {
          if (levelOptions.length > 0) {
            acc[level] = levelOptions;
          }

          return acc;
        }, {});

        await onSubmit({ locations });
        goToNextStep();
      }}
    >
      {form => {
        if (isActive) {
          return (
            <Form {...form} id={formId} showSubmitError>
              <FormFieldset
                id="levels"
                legend={stepHeading}
                hint="Select at least one"
                error={
                  typeof form.errors.locations === 'string'
                    ? form.errors.locations
                    : ''
                }
              >
                <div className="govuk-grid-row">
                  <div className="govuk-grid-column-one-half-from-desktop">
                    {Object.entries(formOptions).map(([levelKey, level]) => {
                      return (
                        <FormFieldCheckboxMenu
                          name={`locations.${levelKey}`}
                          key={levelKey}
                          options={level.options}
                          legend={level.legend}
                          legendHidden
                          disabled={form.isSubmitting}
                          open={autoSelectLocation}
                        />
                      );
                    })}
                  </div>
                </div>
              </FormFieldset>

              <WizardStepFormActions {...props} />
            </Form>
          );
        }

        const locationLevels: Dictionary<FilterOption[]> = mapValues(
          form.values.locations,
          (locations, locationKey) => {
            const locationOptions =
              (options[locationKey] && options[locationKey].options) || [];

            return locations.reduce<FilterOption[]>((acc, n) => {
              const found = locationOptions.find(option => option.value === n);

              if (found) {
                acc.push(found);
              }

              return acc;
            }, []);
          },
        );

        return (
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              {stepHeading}
              <SummaryList noBorder>
                {Object.entries(locationLevels)
                  .filter(
                    ([levelKey, levelOptions]) =>
                      levelOptions.length > 0 && formOptions[levelKey],
                  )
                  .map(([levelKey, levelOptions]) => (
                    <SummaryListItem
                      term={formOptions[levelKey].legend}
                      key={levelKey}
                    >
                      <CollapsibleList>
                        {sortBy(levelOptions, ['label']).map(level => (
                          <li key={level.value}>{level.label}</li>
                        ))}
                      </CollapsibleList>
                    </SummaryListItem>
                  ))}
              </SummaryList>
            </div>
            <div className="govuk-grid-column-one-third dfe-align--right">
              {stepEnabled && (
                <WizardStepEditButton {...props} editTitle="Edit locations" />
              )}
              <ResetFormOnPreviousStep
                currentStep={currentStep}
                stepNumber={stepNumber}
              />
            </div>
          </div>
        );
      }}
    </Formik>
  );
};

export default LocationFiltersForm;
