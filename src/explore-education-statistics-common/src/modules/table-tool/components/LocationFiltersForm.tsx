import { Form, FormFieldset, Formik } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import {
  FilterOption,
  PublicationSubjectMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import { Dictionary } from '@common/types/util';
import useResetFormOnPreviousStep from '@common/modules/table-tool/components/hooks/useResetFormOnPreviousStep';
import { FormikProps } from 'formik';
import sortBy from 'lodash/sortBy';
import React, { useEffect, useMemo, useRef } from 'react';
import { useImmer } from 'use-immer';
import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import FormFieldCheckboxMenu from './FormFieldCheckboxMenu';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

export type LocationsFormValues = Dictionary<string[]>;

interface FormValues {
  locations: LocationsFormValues;
}

export type LocationFiltersFormSubmitHandler = (values: FormValues) => void;

interface Props {
  options: PublicationSubjectMeta['locations'];
  initialValues?: Dictionary<string[] | undefined>;
  onSubmit: LocationFiltersFormSubmitHandler;
}

export const calculateInitialValues = (
  initial: Dictionary<string[] | undefined> | undefined,
  opts: PublicationSubjectMeta['locations'],
): FormValues => {
  const initialNotUndef = initial || {};

  return {
    locations: Object.entries(opts).reduce((acc, [level, levelOptions]) => {
      const initialLevel = initialNotUndef[level] || [];
      return {
        ...acc,
        [level]: initialLevel.filter(
          initialId =>
            levelOptions.options.find(({ value }) => value === initialId) !==
            undefined,
        ),
      };
    }, {}),
  };
};

const LocationFiltersForm = (props: Props & InjectedWizardProps) => {
  const {
    options,
    onSubmit,
    isActive,
    goToNextStep,
    currentStep,
    stepNumber,
    initialValues,
  } = props;

  const formikRef = useRef<Formik<FormValues>>(null);
  const formId = 'locationFiltersForm';

  const formOptions = useMemo(() => options, [options]);

  const formInitialValues = useMemo(
    () => calculateInitialValues(initialValues, options),
    [initialValues, options],
  );

  const initialLocationLevels = useMemo(() => {
    return mapValuesWithKeys<Dictionary<string[]>, FilterOption[]>(
      formInitialValues.locations,
      (locationKey: string, locations: string[]) => {
        const locationOptions =
          (options[locationKey] && options[locationKey].options) || [];

        return locations.reduce<FilterOption[]>((v, n) => {
          const found = locationOptions.find(i => i.value === n);
          if (found) return [...v, found];
          return v;
        }, []);
      },
    );
  }, [formInitialValues.locations, options]);

  const [locationLevels, updateLocationLevels] = useImmer<
    Dictionary<{ label: string; value: string }[]>
  >(initialLocationLevels);

  useEffect(() => {
    updateLocationLevels(() => initialLocationLevels);
  }, [initialLocationLevels, updateLocationLevels]);

  useResetFormOnPreviousStep(formikRef, currentStep, stepNumber, () => {
    updateLocationLevels(() => {
      return initialLocationLevels;
    });
  });

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose locations
    </WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      ref={formikRef}
      initialValues={formInitialValues}
      validateOnBlur={false}
      validateOnChange={false}
      validationSchema={Yup.object<FormValues>({
        locations: Yup.mixed().test(
          'required',
          'Select at least one location',
          (value: Dictionary<string[]>) =>
            Object.values(value).some(groupOptions => groupOptions.length > 0),
        ),
      })}
      onSubmit={async values => {
        const locations = Object.entries(values.locations).reduce(
          (acc, [level, levelOptions]) => {
            if (levelOptions.length === 0) {
              return acc;
            }

            return {
              ...acc,
              [level]: levelOptions,
            };
          },
          {},
        );

        await onSubmit({ locations });
        goToNextStep();
      }}
      render={(form: FormikProps<FormValues>) => {
        return isActive ? (
          <Form {...form} id={formId} displayErrorMessageOnUncaughtErrors>
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
                        id={`${formId}-levels-${levelKey}`}
                        legend={level.legend}
                        legendHidden
                        onAllChange={() => {
                          updateLocationLevels(draft => {
                            if (!draft[levelKey]) {
                              draft[levelKey] = [];
                            }
                            draft[levelKey] =
                              draft[levelKey].length < level.options.length
                                ? level.options
                                : [];
                          });
                        }}
                        onChange={(event, option) => {
                          event.persist();

                          updateLocationLevels(draft => {
                            if (!draft[levelKey]) {
                              draft[levelKey] = [];
                            }

                            const { value } = event.target;

                            const matchingOption = draft[levelKey].find(
                              levelOption => levelOption.value === value,
                            );

                            if (matchingOption) {
                              draft[levelKey] = draft[levelKey].filter(
                                levelOption => levelOption.value !== value,
                              );
                            } else {
                              draft[levelKey].push({
                                value,
                                label: option.label,
                              });
                            }
                          });
                        }}
                      />
                    );
                  })}
                </div>
              </div>
            </FormFieldset>

            <WizardStepFormActions {...props} form={form} formId={formId} />
          </Form>
        ) : (
          <>
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
                    shouldCollapse
                  >
                    {sortBy(levelOptions, ['label']).map(level => (
                      <React.Fragment key={level.value}>
                        {level.label}
                        <br />
                      </React.Fragment>
                    ))}
                  </SummaryListItem>
                ))}
            </SummaryList>
          </>
        );
      }}
    />
  );
};

export default LocationFiltersForm;
