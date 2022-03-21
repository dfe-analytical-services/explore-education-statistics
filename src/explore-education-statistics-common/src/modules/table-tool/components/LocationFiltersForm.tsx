import CollapsibleList from '@common/components/CollapsibleList';
import { Form, FormFieldset } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import FormFieldCheckboxGroupsMenu from '@common/modules/table-tool/components/FormFieldCheckboxGroupsMenu';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import {
  FilterOption,
  LocationOption,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import locationLevelsMap from '@common/utils/locationLevelsMap';
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

export interface LocationFormValues {
  locations: Dictionary<string[]>;
}

export type LocationFiltersFormSubmitHandler = (values: {
  locationIds: string[];
}) => void;

const formId = 'locationFiltersForm';

interface Props extends InjectedWizardProps {
  options: SubjectMeta['locations'];
  initialValues?: string[];
  onSubmit: LocationFiltersFormSubmitHandler;
}

const LocationFiltersForm = ({
  initialValues = [],
  options,
  onSubmit,
  ...stepProps
}: Props) => {
  const { isActive, goToNextStep } = stepProps;
  const levelKeys = Object.keys(options);

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      {levelKeys.length === 1 && locationLevelsMap[levelKeys[0]]
        ? `Choose ${locationLevelsMap[levelKeys[0]].plural}`
        : 'Choose locations'}
    </WizardStepHeading>
  );

  const formOptions = useMemo(() => Object.entries(options), [options]);

  // Key options by level and then by id to make future lookups faster/easier.
  const keyedOptions = useMemo(
    () =>
      mapValues(options, level =>
        level.options.reduce<Dictionary<LocationOption>>((acc, option) => {
          // There are nested options, so we need
          // to iterate through these instead.
          if (option.options) {
            option.options.forEach(subOption => {
              acc[subOption.id ?? ''] = subOption;
            });
          } else {
            acc[option.id ?? ''] = option;
          }
          return acc;
        }, {}),
      ),
    [options],
  );

  // A single option exists when there's a single level
  // and a single key in the dictionary of options for that level.
  const hasSingleOption =
    levelKeys.length === 1 &&
    Object.keys(keyedOptions[levelKeys[0]]).length === 1;

  const initialFormValues = useMemo(() => {
    return {
      locations: mapValues(options, (_, levelKey) => {
        const level = keyedOptions[levelKey];
        const levelValues = Object.keys(level);
        // Automatically select and expand group if
        // only one location option is available.
        if (hasSingleOption) {
          return [levelValues[0]];
        }

        return levelValues.filter(value => initialValues.includes(value));
      }),
    };
  }, [initialValues, hasSingleOption, keyedOptions, options]);

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
        const locationIds = Object.values(values.locations).flat();
        await goToNextStep(async () => {
          await onSubmit({ locationIds });
        });
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
                            options:
                              group.options?.map(option => ({
                                label: option.label,
                                value: option.id ?? '',
                              })) ?? [],
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
                          options={level.options.map(option => ({
                            label: option.label,
                            value: option.id ?? '',
                          }))}
                        />
                      );
                    })}
                  </div>
                </div>
              </FormFieldset>

              <WizardStepFormActions {...stepProps} />
            </Form>
          );
        }

        const locationLevels: Dictionary<FilterOption[]> = mapValues(
          form.values.locations,
          (locations, level) =>
            locations.reduce<FilterOption[]>((acc, value) => {
              const levelOptions = keyedOptions[level];
              if (levelOptions && levelOptions[value]) {
                acc.push(levelOptions[value]);
              }

              return acc;
            }, []),
        );

        return (
          <WizardStepSummary {...stepProps} goToButtonText="Edit locations">
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

            <ResetFormOnPreviousStep {...stepProps} />
          </WizardStepSummary>
        );
      }}
    </Formik>
  );
};

export default LocationFiltersForm;
