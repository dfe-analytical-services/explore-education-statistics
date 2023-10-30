import CollapsibleList from '@common/components/CollapsibleList';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldCheckboxGroupsMenu from '@common/components/form/rhf/RHFFormFieldCheckboxGroupsMenu';
import RHFFormFieldCheckboxMenu from '@common/components/form/rhf/RHFFormFieldCheckboxMenu';
import getErrorMessage from '@common/components/form/rhf/util/getErrorMessage';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
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
import mapValues from 'lodash/mapValues';
import sortBy from 'lodash/sortBy';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  locations: Dictionary<string[]>;
}

export type LocationFiltersFormSubmitHandler = (values: {
  locationIds: string[];
}) => void | Promise<void>;

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

  // For school options flatten the location hierarchy and include the URN and LA as a hint.
  // If we use search only for other location types later then may need to change this.
  const getSearchOnlyOptions = (
    opts: LocationOption[],
    hasSubGroups: boolean,
  ) => {
    if (!hasSubGroups) {
      return opts.map(opt => ({
        label: opt.label,
        value: opt.id ?? '',
        hint: `URN: ${opt.value}`,
        hintSmall: true,
      }));
    }
    return opts.flatMap(group => {
      const level =
        group.level && group.label
          ? `${locationLevelsMap[group.level].label}: ${group.label}`
          : '';
      return (
        group.options?.map(opt => ({
          label: opt.label,
          value: opt.id ?? '',
          hint: `URN: ${opt.value}; ${level}`,
          hintSmall: true,
        })) ?? []
      );
    });
  };

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

  const initialFormValues = useMemo<FormValues>(() => {
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

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      locations: Yup.object().test(
        'required',
        'Select at least one location',
        (value: Dictionary<string[]>) =>
          Object.values(value).some(groupOptions => groupOptions.length > 0),
      ),
    });
  }, []);

  const handleSubmit = async (values: FormValues) => {
    const locationIds = Object.values(values.locations).flat();
    await goToNextStep(async () => {
      await onSubmit({ locationIds });
    });
  };

  return (
    <FormProvider
      enableReinitialize
      initialValues={initialFormValues}
      validationSchema={validationSchema}
    >
      {({ formState, getValues, reset, trigger }) => {
        const showError =
          !formState.isValid &&
          (Object.keys(formState.touchedFields).length > 0 ||
            formState.isSubmitted);

        if (showError && !formState.errors.locations) {
          trigger('locations');
        }

        if (isActive) {
          return (
            <RHFForm
              id="locationFiltersForm"
              showErrorSummary={showError}
              onSubmit={handleSubmit}
            >
              <FormFieldset
                id="levels"
                legend={stepHeading}
                hint="Select at least one"
                error={
                  showError
                    ? getErrorMessage(formState.errors, 'locations')
                    : ''
                }
              >
                <div className="govuk-grid-row">
                  <div className="govuk-grid-column-one-half-from-desktop">
                    {formOptions.map(([levelKey, level]) => {
                      const hasSubGroups = level.options.some(
                        option => option.options,
                      );
                      const searchOnly = levelKey === 'school';

                      return hasSubGroups && !searchOnly ? (
                        <RHFFormFieldCheckboxGroupsMenu
                          key={levelKey}
                          name={`locations.${levelKey}`}
                          disabled={formState.isSubmitting}
                          groupLabel={level.legend}
                          legend={level.legend}
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
                        <RHFFormFieldCheckboxMenu
                          key={levelKey}
                          name={`locations.${levelKey}`}
                          disabled={formState.isSubmitting}
                          groupLabel={level.legend}
                          legend={level.legend}
                          legendHidden
                          open={hasSingleOption || searchOnly}
                          order={searchOnly ? 'label' : []}
                          options={
                            searchOnly
                              ? getSearchOnlyOptions(
                                  level.options,
                                  hasSubGroups,
                                )
                              : level.options.map(option => ({
                                  label: option.label,
                                  value: option.id ?? '',
                                }))
                          }
                          searchOnly={searchOnly}
                          searchHelpText={
                            searchOnly
                              ? 'Search by school name or unique reference number (URN), and select at least one option before continuing to the next step.'
                              : 'Search above and select at least one option before continuing to the next step.'
                          }
                        />
                      );
                    })}
                  </div>
                </div>
              </FormFieldset>

              <WizardStepFormActions
                {...stepProps}
                isSubmitting={formState.isSubmitting}
              />
            </RHFForm>
          );
        }

        const values = getValues();

        const locationLevels: Dictionary<FilterOption[]> = mapValues(
          values.locations,
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
                    <CollapsibleList
                      id="locationsList"
                      itemName="location"
                      itemNamePlural="locations"
                    >
                      {sortBy(levelOptions, ['label']).map(level => (
                        <li key={level.value}>{level.label}</li>
                      ))}
                    </CollapsibleList>
                  </SummaryListItem>
                ))}
            </SummaryList>

            <ResetFormOnPreviousStep {...stepProps} onReset={reset} />
          </WizardStepSummary>
        );
      }}
    </FormProvider>
  );
};

export default LocationFiltersForm;
