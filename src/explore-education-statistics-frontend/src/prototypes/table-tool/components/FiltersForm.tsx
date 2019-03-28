import { Form, Formik, FormikErrors, FormikProps, FormikTouched } from 'formik';
import get from 'lodash/get';
import mapValues from 'lodash/mapValues';
import React, { Component, createRef } from 'react';
import Button from 'src/components/Button';
import ErrorSummary, { ErrorSummaryMessage } from 'src/components/ErrorSummary';
import { FormFieldset, FormGroup } from 'src/components/form';
import createErrorHelper from 'src/lib/validation/createErrorHelper';
import Yup from 'src/lib/validation/yup';
import CategoricalFilters from './CategoricalFilters';
import { MetaSpecification } from './meta/initialSpec';
import ObservationalUnitFilters from './ObservationalUnitFilters';
import SearchableGroupedFilterMenus from './SearchableGroupedFilterMenus';

export interface FormValues {
  endDate: number;
  indicators: string[];
  location: {
    level: string;
    national: string;
    region: string;
    localAuthority: string;
  };
  startDate: number;
  categoricalFilters: {
    [key: string]: string[];
  };
}

export type FilterFormSubmitHandler = (values: FormValues) => void;

interface Props {
  specification: MetaSpecification;
  onSubmit: FilterFormSubmitHandler;
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
      .filter(([errorName]) => get(touched, errorName))
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

    let locationRules = Yup.object<FormValues['location']>({
      level: Yup.string().notRequired(),
      localAuthority: Yup.string().notRequired(),
      national: Yup.string().notRequired(),
      region: Yup.string().notRequired(),
    });

    if (
      Object.keys(specification.observationalUnits.location.options).length > 1
    ) {
      locationRules = locationRules.shape({
        ...mapValues(
          specification.observationalUnits.location.options,
          locationLevel =>
            Yup.string().when('level', {
              is: locationLevel,
              then: Yup.string().required('Select a location'),
            }),
        ),
        level: Yup.string().required('Select a location level'),
      });
    }

    return (
      <Formik<FormValues>
        initialValues={{
          categoricalFilters: mapValues(
            specification.categoricalFilters,
            () => [],
          ),
          endDate: 2016,
          indicators: [],
          location: {
            level: '',
            localAuthority: '',
            national: '',
            region: '',
          },
          startDate: 2012,
        }}
        validationSchema={Yup.object<FormValues>({
          categoricalFilters: Yup.object(
            mapValues(specification.categoricalFilters, () =>
              Yup.array()
                .of(Yup.string())
                .required('Select at least one option'),
            ),
          ),
          endDate: Yup.number()
            .required('End date is required')
            .oneOf(startEndDateValues, 'Must be one of provided dates')
            .moreThanOrEqual(
              Yup.ref('startDate'),
              'Must be after or same as start date',
            ),
          indicators: Yup.array()
            .of(Yup.string())
            .required('Select at least one option'),
          location: locationRules,
          startDate: Yup.number()
            .required('Start date is required')
            .oneOf(startEndDateValues, 'Must be one of provided dates')
            .lessThanOrEqual(
              Yup.ref('endDate'),
              'Must be before or same as end date',
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
          const { getAllErrors, getError } = createErrorHelper({
            errors,
            touched,
          });

          return (
            <div ref={this.ref}>
              <ErrorSummary
                errors={this.getSummaryErrors(getAllErrors(), touched)}
                id="filter-errors"
              />

              <Form>
                <div className="govuk-grid-row">
                  <FormGroup className="govuk-grid-column-one-quarter-from-desktop">
                    <ObservationalUnitFilters
                      form={form}
                      specification={specification}
                    />
                  </FormGroup>
                  <FormGroup className="govuk-grid-column-one-half-from-desktop">
                    <CategoricalFilters
                      form={form}
                      specification={specification.categoricalFilters}
                    />
                  </FormGroup>
                  <FormGroup className="govuk-grid-column-one-quarter-from-desktop">
                    <FormFieldset
                      id="filter-indicators"
                      legend="Indicators"
                      hint="Filter by at least one statistical indicator from the publication"
                      error={getError('indicators')}
                    >
                      <SearchableGroupedFilterMenus<FormValues>
                        menuOptions={specification.indicators}
                        name="indicators"
                        values={values.indicators}
                      />
                    </FormFieldset>
                  </FormGroup>
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
                    ? 'Creating table...'
                    : 'Create table'}
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
