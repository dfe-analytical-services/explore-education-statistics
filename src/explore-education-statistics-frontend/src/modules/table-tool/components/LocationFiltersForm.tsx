import Button from '@common/components/Button';
import { Form, FormFieldset, FormGroup } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { Dictionary } from '@common/types/util';
import { Formik, FormikProps } from 'formik';
import mapValues from 'lodash/mapValues';
import sortBy from 'lodash/sortBy';
import React from 'react';
import { useImmer } from 'use-immer';
import FormFieldCheckboxMenu from './FormFieldCheckboxMenu';
import { MetaSpecification } from './meta/initialSpec';
import { InjectedWizardProps } from './Wizard';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  locations: {
    [level: string]: string[];
  };
}

interface Props {
  specification: MetaSpecification;
  onSubmit: (values: FormValues['locations']) => void;
}

const LocationFiltersForm = (props: Props & InjectedWizardProps) => {
  const {
    specification,
    onSubmit,
    isActive,
    goToNextStep,
    goToPreviousStep,
  } = props;
  const { locations } = specification;

  const [locationLevels, updateLocationLevels] = useImmer<
    Dictionary<{ label: string; value: string }[]>
  >(mapValues(specification.locations, () => []));

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose locations
    </WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      onSubmit={values => {
        onSubmit(values.locations);
        goToNextStep();
      }}
      initialValues={{
        locations: Object.keys(locations).reduce((acc, level) => {
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
          (value: string) =>
            Object.values(value).some(options => options.length > 0),
        ),
      })}
      render={(form: FormikProps<FormValues>) => {
        return isActive ? (
          <Form {...form} id="locationFiltersForm">
            <FormFieldset
              id="locationFiltersForm-levels"
              legend={stepHeading}
              hint="Select at least one"
              error={
                typeof form.errors.locations === 'string'
                  ? form.errors.locations
                  : ''
              }
            >
              {Object.entries(locations).map(([levelKey, level]) => {
                return (
                  <FormFieldCheckboxMenu
                    name={`locations.${levelKey}`}
                    key={levelKey}
                    options={level.options}
                    id={`locationFiltersForm-levels-${levelKey}`}
                    legend={level.legend}
                    legendHidden
                    onAllChange={event => {
                      updateLocationLevels(draft => {
                        draft[levelKey] = event.target.checked
                          ? level.options
                          : [];
                      });
                    }}
                    onChange={(event, option) => {
                      updateLocationLevels(draft => {
                        const matchingOption = locationLevels[levelKey].find(
                          levelOption => levelOption.value === option.value,
                        );

                        if (matchingOption) {
                          draft[levelKey] = locationLevels[levelKey].filter(
                            levelOption => levelOption.value !== option.value,
                          );
                        } else {
                          draft[levelKey].push(option);
                        }
                      });
                    }}
                  />
                );
              })}
            </FormFieldset>

            <FormGroup>
              <Button type="submit">Next step</Button>

              <Button
                type="button"
                variant="secondary"
                onClick={goToPreviousStep}
              >
                Previous step
              </Button>
            </FormGroup>
          </Form>
        ) : (
          <>
            {stepHeading}
            <SummaryList noBorder>
              {Object.entries(locationLevels)
                .filter(([_, levelOptions]) => levelOptions.length > 0)
                .map(([levelKey, levelOptions]) => (
                  <SummaryListItem
                    term={specification.locations[levelKey].legend}
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
