import { Form, Formik, FormikErrors, FormikProps, FormikTouched } from 'formik';
import React, { Component, createRef } from 'react';
import Button from 'src/components/Button';
import ErrorSummary, { ErrorSummaryMessage } from 'src/components/ErrorSummary';
import { FormFieldset } from 'src/components/form';
import createErrorHelper from 'src/lib/validation/createErrorHelper';
import Yup from 'src/lib/validation/yup';
import SchoolType from 'src/services/types/SchoolType';
import CategoricalFilters from './CategoricalFilters';
import { MetaSpecification } from './meta/initialSpec';
import ObservationalUnitFilters from './ObservationalUnitFilters';
import SearchableGroupedFilterMenus from './SearchableGroupedFilterMenus';

export interface FormValues {
  characteristics: string[];
  endYear: number;
  indicators: string[];
  locationLevel: string;
  locationCountry: string;
  locationRegion: string;
  locationLocalAuthority: string;
  schoolTypes: SchoolType[];
  startYear: number;
}

export type CharacteristicsFilterFormSubmitHandler = (
  values: FormValues,
) => void;

interface Props {
  specification: MetaSpecification;
  onSubmit: CharacteristicsFilterFormSubmitHandler;
}

interface State {
  isSubmitted: boolean;
  submitError: string;
}

class FiltersForm extends Component<Props, State> {
  public state: State = {
    isSubmitted: false,
    submitError: '',
  };

  private ref = createRef<HTMLDivElement>();

  private getSummaryErrors(
    errors: FormikErrors<FormValues>,
    touched: FormikTouched<FormValues>,
  ) {
    const summaryErrors: ErrorSummaryMessage[] = Object.entries(errors)
      .filter(([errorName]) => touched[errorName as keyof FormValues])
      .map(([errorName, message]) => ({
        id: `filter-${errorName}`,
        message: typeof message === 'string' ? message : '',
      }));

    if (this.state.submitError) {
      summaryErrors.push({
        id: 'submit-button',
        message: 'Could not submit filters. Please try again later.',
      });
    }

    return summaryErrors;
  }

  public render() {
    const { specification } = this.props;

    const startEndDateValues = specification.observationalUnits.startEndDate.options.map(
      ({ value }) => value,
    );

    return (
      <Formik
        initialValues={{
          characteristics: [],
          endYear: 2016,
          indicators: [],
          locationCountry: '',
          locationLevel: '',
          locationLocalAuthority: '',
          locationRegion: '',
          schoolTypes: [],
          startYear: 2012,
        }}
        validationSchema={Yup.object({
          characteristics: Yup.array().required('Select at least one option'),
          endYear: Yup.number()
            .required('End year is required')
            .oneOf(startEndDateValues, 'Must be one of provided years')
            .moreThanOrEqual(
              Yup.ref('startYear'),
              'Must be after or same as start year',
            ),
          indicators: Yup.array().required('Select at least one option'),
          schoolTypes: Yup.array().required('Select at least one option'),
          startYear: Yup.number()
            .required('Start year is required')
            .oneOf(startEndDateValues, 'Must be one of provided years')
            .lessThanOrEqual(
              Yup.ref('endYear'),
              'Must be before or same as end year',
            ),
        })}
        onSubmit={async (form, actions) => {
          try {
            await this.props.onSubmit(form);
          } catch (error) {
            this.setState({
              submitError: error.message,
            });
          }

          actions.setSubmitting(false);
        }}
        render={(form: FormikProps<FormValues>) => {
          const { errors, touched, values } = form;
          const { getError } = createErrorHelper({ errors, touched });

          return (
            <div ref={this.ref}>
              <ErrorSummary
                errors={this.getSummaryErrors(errors, touched)}
                id="filter-errors"
              />

              <Form>
                <div className="govuk-grid-row">
                  <div className="govuk-grid-column-one-third govuk-form-group">
                    <ObservationalUnitFilters
                      form={form}
                      specification={specification.observationalUnits}
                    />
                  </div>
                  <div className="govuk-grid-column-one-third govuk-form-group">
                    <CategoricalFilters
                      form={form}
                      specification={specification.categoricalFilters}
                    />
                  </div>
                  <div className="govuk-grid-column-one-third govuk-form-group">
                    <FormFieldset
                      id="filter-indicators"
                      legend="Indicators"
                      hint="Filter by at least one statistical indicator from the publication"
                      error={getError('indicators')}
                    >
                      <SearchableGroupedFilterMenus<FormValues>
                        menuOptions={this.props.specification.indicators}
                        name="indicators"
                        values={values.indicators}
                      />
                    </FormFieldset>
                  </div>
                </div>

                <Button
                  disabled={form.isSubmitting}
                  id="submit-button"
                  onClick={event => {
                    event.preventDefault();

                    // Manually validate/submit so we can scroll
                    // back to the top of the form if there are errors
                    form.validateForm().then(validationErrors => {
                      form.submitForm();

                      if (
                        Object.keys(validationErrors).length > 0 &&
                        this.ref.current
                      ) {
                        this.ref.current.scrollIntoView({
                          behavior: 'smooth',
                          block: 'start',
                        });
                      }
                    });
                  }}
                  type="submit"
                >
                  {form.isSubmitting && form.isValid
                    ? 'Submitting'
                    : 'Confirm filters'}
                </Button>
              </Form>
            </div>
          );
        }}
      />
    );
  }
}

export default FiltersForm;
