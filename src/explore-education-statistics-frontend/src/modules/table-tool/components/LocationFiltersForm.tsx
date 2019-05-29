import { Form, FormFieldset, Formik } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { PublicationSubjectMeta } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types/util';
import { FormikProps } from 'formik';
import sortBy from 'lodash/sortBy';
import React, { useEffect } from 'react';
import { useImmer } from 'use-immer';
import FormFieldCheckboxMenu from './FormFieldCheckboxMenu';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  locations: {
    [level: string]: string[];
  };
}

export type LocationFiltersFormSubmitHandler = (values: FormValues) => void;

interface Props {
  options: PublicationSubjectMeta['locations'];
  onSubmit: LocationFiltersFormSubmitHandler;
}

const LocationFiltersForm = (props: Props & InjectedWizardProps) => {
  const { options, onSubmit, isActive, goToNextStep } = props;

  const formId = 'locationFiltersForm';

  const [locationLevels, updateLocationLevels] = useImmer<
    Dictionary<{ label: string; value: string }[]>
  >({});

  useEffect(() => {
    updateLocationLevels(draft => {
      Object.keys(options).forEach(key => {
        draft[key] = [];
      });
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [options]);

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose locations
    </WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      onSubmit={async values => {
        await onSubmit(values);
        goToNextStep();
      }}
      initialValues={{
        locations: Object.keys(options).reduce((acc, level) => {
          return {
            ...acc,
            [level]: [],
          };
        }, {}),
      }}
      validationSchema={Yup.object<FormValues>({
        locations: Yup.mixed().test(
          'required',
          'Select at least one option',
          (value: Dictionary<string[]>) =>
            Object.values(value).some(groupOptions => groupOptions.length > 0),
        ),
      })}
      render={(form: FormikProps<FormValues>) => {
        return isActive ? (
          <Form {...form} id={formId}>
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
                  {Object.entries(options).map(([levelKey, level]) => {
                    return (
                      <FormFieldCheckboxMenu
                        name={`locations.${levelKey}`}
                        key={levelKey}
                        options={level.options}
                        id={`${formId}-levels-${levelKey}`}
                        legend={level.legend}
                        legendHidden
                        selectAll={false}
                        onAllChange={() => {
                          updateLocationLevels(draft => {
                            draft[levelKey] =
                              draft[levelKey].length < level.options.length
                                ? level.options
                                : [];
                          });
                        }}
                        onChange={(event, option) => {
                          updateLocationLevels(draft => {
                            const matchingOption = locationLevels[
                              levelKey
                            ].find(
                              levelOption => levelOption.value === option.value,
                            );

                            if (matchingOption) {
                              draft[levelKey] = locationLevels[levelKey].filter(
                                levelOption =>
                                  levelOption.value !== option.value,
                              );
                            } else {
                              draft[levelKey].push(option);
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
                .filter(([_, levelOptions]) => levelOptions.length > 0)
                .map(([levelKey, levelOptions]) => (
                  <SummaryListItem
                    term={options[levelKey].legend}
                    key={levelKey}
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
