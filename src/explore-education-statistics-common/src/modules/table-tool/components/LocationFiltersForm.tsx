import CollapsibleList from '@common/components/CollapsibleList';
import { Form, FormFieldset } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import {
  FilterOption,
  PublicationSubjectMeta,
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

interface FormValues {
  locations: Dictionary<string[]>;
}

export type LocationFiltersFormSubmitHandler = (values: {
  locations: Dictionary<string[]>;
}) => void;

interface Props {
  options: PublicationSubjectMeta['locations'];
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

  const formId = 'locationFiltersForm';

  const formOptions = useMemo(() => options, [options]);

  const stepHeading = useMemo(
    () => (
      <WizardStepHeading {...props} fieldsetHeading>
        Choose locations
      </WizardStepHeading>
    ),
    [props],
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        locations: mapValues(options, (levelOptions, level) => {
          const initialLevel = initialValues[level] ?? [];

          return initialLevel.filter(
            initialId =>
              levelOptions.options.find(({ value }) => value === initialId) !==
              undefined,
          );
        }),
      }}
      validateOnBlur={false}
      validationSchema={Yup.object<FormValues>({
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
                id={`${formId}-levels`}
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
                          order={[]}
                          id={`${formId}-levels-${levelKey}`}
                          legend={level.legend}
                          legendHidden
                          disabled={form.isSubmitting}
                        />
                      );
                    })}
                  </div>
                </div>
              </FormFieldset>

              <WizardStepFormActions {...props} form={form} formId={formId} />
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
          <>
            {stepHeading}

            <ResetFormOnPreviousStep
              currentStep={currentStep}
              stepNumber={stepNumber}
            />

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
          </>
        );
      }}
    </Formik>
  );
};

export default LocationFiltersForm;
