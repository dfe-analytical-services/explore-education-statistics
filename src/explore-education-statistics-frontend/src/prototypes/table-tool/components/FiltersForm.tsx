import Button from '@common/components/Button';
import ErrorSummary, {
  ErrorSummaryMessage,
} from '@common/components/ErrorSummary';
import { FormFieldset, FormGroup } from '@common/components/form';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import { Form, Formik, FormikErrors, FormikProps, FormikTouched } from 'formik';
import get from 'lodash/get';
import mapValues from 'lodash/mapValues';
import React, { Component, createRef } from 'react';
import CategoricalFilters from './CategoricalFilters';
import { MetaSpecification } from './meta/initialSpec';
import SearchableGroupedFilterMenus from './SearchableGroupedFilterMenus';

export interface FormValues {
  indicators: string[];
  filters: {
    [key: string]: string[];
  };
}

export type FilterFormSubmitHandler = (values: FormValues) => void;

interface Props {
  specification: MetaSpecification;
  onSubmit: FilterFormSubmitHandler;
}

interface State {
  submitError: string;
}

class FiltersForm extends Component<Props, State> {
  public state: State = {
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

    const { submitError } = this.state;

    if (submitError) {
      summaryErrors.push({
        id: 'submit-button',
        message: 'Could not submit filters. Please try again later.',
      });
    }

    return summaryErrors;
  }

  public render() {
    const { onSubmit, specification } = this.props;

    return (
      <Formik<FormValues>
        initialValues={{
          filters: mapValues(specification.filters, () => []),
          indicators: [],
        }}
        validationSchema={Yup.object<FormValues>({
          filters: Yup.object(
            mapValues(specification.filters, () =>
              Yup.array()
                .of(Yup.string())
                .required('Select at least one option'),
            ),
          ),
          indicators: Yup.array()
            .of(Yup.string())
            .required('Select at least one option'),
        })}
        onSubmit={async (values, actions) => {
          try {
            await onSubmit(values);
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
                  <FormGroup className="govuk-grid-column-one-half-from-desktop">
                    <CategoricalFilters
                      form={form}
                      specification={specification.filters}
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
