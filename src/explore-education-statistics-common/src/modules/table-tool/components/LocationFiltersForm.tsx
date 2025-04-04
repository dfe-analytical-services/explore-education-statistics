import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldCheckboxGroupsMenu from '@common/components/form/FormFieldCheckboxGroupsMenu';
import FormFieldCheckboxMenu from '@common/components/form/FormFieldCheckboxMenu';
import getErrorMessage from '@common/components/form/util/getErrorMessage';
import {
  LocationOption,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import locationLevelsMap, {
  LocationLevelKey,
} from '@common/utils/locationLevelsMap';
import { Dictionary } from '@common/types/util';
import Yup from '@common/validation/yup';
import mapValues from 'lodash/mapValues';
import React, { ReactNode, useMemo } from 'react';
import { ObjectSchema } from 'yup';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';

interface FormValues {
  locations: Dictionary<string[]>;
}

export type LocationFiltersFormSubmitHandler = (values: {
  locationIds: string[];
}) => void | Promise<void>;

export interface LocationFiltersFormProps extends InjectedWizardProps {
  options: SubjectMeta['locations'];
  initialValues?: string[];
  stepHeading?: ReactNode;
  onSubmit: LocationFiltersFormSubmitHandler;
}

const LocationFiltersForm = ({
  initialValues = [],
  options,
  stepHeading,
  onSubmit,
  ...stepProps
}: LocationFiltersFormProps) => {
  const { goToNextStep } = stepProps;

  const levelKeys = Object.keys(options);

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
          ? `${locationLevelsMap[group.level as LocationLevelKey].label}: ${
              group.label
            }`
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
          Object.values(value).some(
            groupOptions => groupOptions && groupOptions.length > 0,
          ),
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
      {({ formState, trigger }) => {
        const isInvalidOnEdit = !!(!formState.isValid && initialValues.length);

        const showError =
          !formState.isValid &&
          (Object.keys(formState.touchedFields).length > 0 ||
            formState.isSubmitted ||
            isInvalidOnEdit);

        if (showError && !formState.errors.locations) {
          trigger('locations');
        }

        return (
          <Form
            id="locationFiltersForm"
            initialTouched={initialValues.length ? ['locations'] : []}
            showErrorSummary={showError}
            onSubmit={handleSubmit}
          >
            <FormFieldset
              id="locations"
              legend={stepHeading}
              hint="Select at least one"
              error={
                showError ? getErrorMessage(formState.errors, 'locations') : ''
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
                      <FormFieldCheckboxGroupsMenu
                        id={`locations-${levelKey}`}
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
                      <FormFieldCheckboxMenu
                        id={`locations-${levelKey}`}
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
                            ? getSearchOnlyOptions(level.options, hasSubGroups)
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
                        small
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
          </Form>
        );
      }}
    </FormProvider>
  );
};

export default LocationFiltersForm;
