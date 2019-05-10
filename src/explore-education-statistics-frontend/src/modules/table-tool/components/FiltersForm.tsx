import { Form, FormFieldset, FormGroup } from '@common/components/form';
import FormFieldCheckboxSearchSubGroups from '@common/components/form/FormFieldCheckboxSearchSubGroups';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import { PublicationSubjectMeta } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types/util';
import WizardStepFormActions from '@frontend/modules/table-tool/components/WizardStepFormActions';
import { Formik, FormikProps } from 'formik';
import camelCase from 'lodash/camelCase';
import mapValues from 'lodash/mapValues';
import React, { useRef } from 'react';
import FormFieldCheckboxGroupsMenu from './FormFieldCheckboxGroupsMenu';
import { InjectedWizardProps } from './Wizard';
import WizardStepHeading from './WizardStepHeading';

export interface FormValues {
  indicators: string[];
  filters: {
    [key: string]: string[];
  };
}

export type FilterFormSubmitHandler = (values: FormValues) => void;

interface Props {
  specification: PublicationSubjectMeta;
  onSubmit: FilterFormSubmitHandler;
}

const FiltersForm = (props: Props & InjectedWizardProps) => {
  const { onSubmit, specification, goToNextStep } = props;

  const ref = useRef<HTMLDivElement>(null);

  const formId = 'filtersForm';

  const stepHeading = (
    <WizardStepHeading {...props}>Choose your filters</WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      initialValues={{
        filters: mapValues(specification.filters, () => []),
        indicators: [],
      }}
      validationSchema={Yup.object<FormValues>({
        filters: Yup.mixed().test(
          'required',
          'Must select options from only two categories',
          (value: Dictionary<string[]>) =>
            Object.values(value)
              .map(group => group.length > 0)
              .filter(Boolean).length === 2,
        ),
        indicators: Yup.array()
          .of(Yup.string())
          .required('Must select at least one indicator'),
      })}
      onSubmit={async values => {
        await onSubmit(values);
        goToNextStep();
      }}
      render={(form: FormikProps<FormValues>) => {
        const { getError } = createErrorHelper(form);

        return (
          <div ref={ref}>
            <Form {...form} id={formId}>
              {stepHeading}

              <FormGroup>
                <div className="govuk-grid-row">
                  <div className="govuk-grid-column-one-half-from-desktop">
                    <FormFieldset
                      id={`${formId}-filters`}
                      legend="Categories"
                      legendSize="s"
                      hint="Select options from two categories"
                      error={getError('filters')}
                    >
                      {Object.entries(specification.filters).map(
                        ([filterKey, filterGroup]) => {
                          const filterName = `filters.${filterKey}`;

                          return (
                            <FormFieldCheckboxGroupsMenu<FormValues>
                              key={filterKey}
                              name={filterName}
                              id={`${formId}-${camelCase(filterName)}`}
                              legend={filterGroup.legend}
                              hint={filterGroup.hint}
                              error={getError(filterName)}
                              selectAll
                              options={Object.entries(filterGroup.options).map(
                                ([_, group]) => {
                                  return {
                                    legend: group.label,
                                    options: group.options,
                                  };
                                },
                              )}
                            />
                          );
                        },
                      )}
                    </FormFieldset>
                  </div>
                  <div className="govuk-grid-column-one-half-from-desktop">
                    <FormFieldCheckboxSearchSubGroups
                      name="indicators"
                      id={`${formId}-indicators`}
                      legend="Indicators"
                      legendSize="s"
                      hint="Select at least one indicator"
                      error={getError('indicators')}
                      options={Object.entries(specification.indicators).map(
                        ([_, group]) => {
                          return {
                            legend: group.label,
                            options: group.options,
                          };
                        },
                      )}
                    />
                  </div>
                </div>
              </FormGroup>

              <WizardStepFormActions
                {...props}
                form={form}
                formId={formId}
                submitText="Create table"
                submittingText="Creating table"
              />
            </Form>
          </div>
        );
      }}
    />
  );
};

export default FiltersForm;
