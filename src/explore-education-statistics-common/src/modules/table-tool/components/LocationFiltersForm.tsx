import CollapsibleList from '@common/components/CollapsibleList';
import { Form, FormFieldset } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import FormFieldCheckboxGroupsMenu from '@common/modules/table-tool/components/FormFieldCheckboxGroupsMenu';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import {
  FilterOption,
  LocationOption,
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

  const stepEnabled = currentStep > stepNumber;
  const stepHeading = useMemo(
    () => (
      <WizardStepHeading {...props} fieldsetHeading stepEnabled={stepEnabled}>
        Choose locations
      </WizardStepHeading>
    ),
    [props, stepEnabled],
  );

  const formOptions = useMemo(() => Object.entries(options), [options]);

  // Key options by their value to make future lookups faster/easier.
  const keyedOptions = useMemo(
    () =>
      formOptions.reduce<Dictionary<LocationOption>>((acc, [, level]) => {
        level.options.forEach(option => {
          // There are nested options, so we need
          // to iterate through these instead.
          if (option.options) {
            option.options.forEach(subOption => {
              acc[subOption.value] = subOption;
            });
          } else {
            acc[option.value] = option;
          }
        });

        return acc;
      }, {}),
    [formOptions],
  );

  const allOptions = useMemo(() => Object.values(keyedOptions), [keyedOptions]);

  const hasSingleOption = allOptions.length === 1;

  const initialFormValues = useMemo(() => {
    return {
      locations: mapValues(options, (levelOptions, level) => {
        // Automatically select and expand group if
        // only one location option is available.
        const initialLevelValues = hasSingleOption
          ? [allOptions[0].value]
          : initialValues[level] ?? [];

        return initialLevelValues.filter(value => keyedOptions[value]);
      }),
    };
  }, [allOptions, hasSingleOption, initialValues, keyedOptions, options]);

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
                    {formOptions.map(([levelKey, level]) => {
                      const hasSubGroups = level.options.some(
                        option => option.options,
                      );

                      return hasSubGroups ? (
                        <FormFieldCheckboxGroupsMenu
                          key={levelKey}
                          name={`locations.${levelKey}`}
                          disabled={form.isSubmitting}
                          legend={level.legend}
                          legendHidden
                          open={hasSingleOption}
                          order={[]}
                          options={level.options.map(group => ({
                            legend: group.label,
                            options: group.options ?? [],
                          }))}
                        />
                      ) : (
                        <FormFieldCheckboxMenu
                          key={levelKey}
                          name={`locations.${levelKey}`}
                          disabled={form.isSubmitting}
                          legend={level.legend}
                          legendHidden
                          open={hasSingleOption}
                          order={[]}
                          options={level.options}
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
          locations =>
            locations.reduce<FilterOption[]>((acc, value) => {
              if (keyedOptions[value]) {
                acc.push(keyedOptions[value]);
              }

              return acc;
            }, []),
        );

        return (
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              {stepHeading}
              <SummaryList noBorder>
                {Object.entries(locationLevels)
                  .filter(
                    ([levelKey, levelOptions]) =>
                      levelOptions.length > 0 && options[levelKey],
                  )
                  .map(([levelKey, levelOptions]) => (
                    <SummaryListItem
                      term={options[levelKey].legend}
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
