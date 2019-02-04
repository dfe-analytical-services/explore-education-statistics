import {
  Field,
  FieldArray,
  FieldProps,
  Form,
  Formik,
  FormikErrors,
  FormikProps,
  FormikTouched,
} from 'formik';
import debounce from 'lodash/debounce';
import difference from 'lodash/difference';
import sortBy from 'lodash/sortBy';
import React, { ChangeEvent, Component, createRef } from 'react';
import Button from '../../../components/Button';
import ErrorSummary, {
  ErrorSummaryMessage,
} from '../../../components/ErrorSummary';
import {
  FormCheckboxGroup,
  FormFieldSet,
  FormGroup,
  FormSelect,
  FormTextInput,
} from '../../../components/form';
import Yup from '../../../lib/validation/yup';
import { PublicationMeta } from '../../../services/dataTableService';
import MenuDetails from './MenuDetails';

interface FormValues {
  attributes: string[];
  characteristics: string[];
  endYear: number;
  startYear: number;
}

export type CharacteristicsFilterFormSubmitHandler = (values: {
  attributes: string[];
  characteristics: string[];
}) => void;

interface Props {
  publicationMeta: Pick<PublicationMeta, 'attributes' | 'characteristics'>;
  onSubmit: CharacteristicsFilterFormSubmitHandler;
}

interface State {
  isSubmitted: boolean;
  searchTerm: string;
  submitError: string;
}

class CharacteristicsFilterForm extends Component<Props, State> {
  public state = {
    isSubmitted: false,
    searchTerm: '',
    submitError: '',
  };

  private ref = createRef<HTMLDivElement>();

  private setDebouncedFilterSearch = debounce(
    (event: ChangeEvent<HTMLInputElement>) => {
      this.setState({
        searchTerm: event.target.value,
      });
    },
    300,
  );

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

  private renderGroupedOptions(
    formKey: 'characteristics' | 'attributes',
    formValues: FormValues,
  ) {
    const groupData = this.props.publicationMeta[formKey];
    const values = formValues[formKey];

    const containsSearchTerm = (value: string) =>
      value.search(new RegExp(this.state.searchTerm, 'i')) > -1;

    const groups = Object.entries(groupData)
      .filter(
        ([groupKey]) =>
          this.state.searchTerm === '' ||
          groupData[groupKey].some(
            item =>
              containsSearchTerm(item.label) || values.indexOf(item.name) > -1,
          ),
      )
      .map(([groupKey, items]) => {
        const isMenuOpen = groupData[groupKey].some(
          item =>
            (this.state.searchTerm !== '' && containsSearchTerm(item.label)) ||
            values.indexOf(item.name) > -1,
        );

        const options = sortBy(
          items
            .filter(
              item =>
                this.state.searchTerm === '' ||
                containsSearchTerm(item.label) ||
                values.indexOf(item.name) > -1,
            )
            .map(item => ({
              id: item.name,
              label: item.label,
              value: item.name,
            })),
          ['label'],
        );

        const checkedState = groupData[groupKey].reduce((acc, option) => {
          return {
            ...acc,
            [option.name]: values.indexOf(option.name) > -1,
          };
        }, {});

        return (
          <MenuDetails summary={groupKey} key={groupKey} open={isMenuOpen}>
            <FieldArray name={formKey}>
              {({ form, ...helpers }) => (
                <FormCheckboxGroup
                  checkedValues={checkedState}
                  name={formKey}
                  id={`${formKey}-${groupKey}`}
                  onAllChange={event => {
                    if (this.state.searchTerm !== '') {
                      return;
                    }

                    const allOptionValues = groupData[groupKey].map(
                      value => value.name,
                    );
                    const restValues = difference(
                      form.values[formKey],
                      allOptionValues,
                    );

                    if (event.target.checked) {
                      form.setFieldValue(formKey, [
                        ...restValues,
                        ...allOptionValues,
                      ]);
                    } else {
                      form.setFieldValue(formKey, restValues);
                    }
                  }}
                  onChange={event => {
                    if (event.target.checked) {
                      helpers.push(event.target.value);
                    } else {
                      const index = form.values[formKey].indexOf(
                        event.target.value,
                      );

                      if (index > -1) {
                        helpers.remove(index);
                      }
                    }
                  }}
                  options={options}
                />
              )}
            </FieldArray>
          </MenuDetails>
        );
      });

    return groups.length > 0
      ? groups
      : `No options matching '${this.state.searchTerm}'.`;
  }

  public render() {
    const { submitError } = this.state;

    const yearOptions = [
      { value: 201112, text: '2011/12' },
      { value: 201213, text: '2012/13' },
      { value: 201314, text: '2013/14' },
      { value: 201415, text: '2014/15' },
      { value: 201516, text: '2015/16' },
      { value: 201617, text: '2016/17' },
    ];

    const yearValues = yearOptions.map(({ value }) => value);

    return (
      <Formik
        initialValues={{
          attributes: [],
          characteristics: [],
          endYear: 201617,
          startYear: 201213,
        }}
        validationSchema={Yup.object({
          attributes: Yup.array().required('Select at least one option'),
          characteristics: Yup.array().required('Select at least one option'),
          endYear: Yup.number()
            .required('End year is required')
            .oneOf(yearValues, 'Must be one of provided years')
            .moreThanOrEqual(
              Yup.ref('startYear'),
              'Must be after or same as start year',
            ),
          startYear: Yup.number()
            .required('Start year is required')
            .oneOf(yearValues, 'Must be one of provided years')
            .lessThanOrEqual(
              Yup.ref('endYear'),
              'Must be before or same as end year',
            ),
        })}
        onSubmit={async ({ attributes, characteristics }, actions) => {
          try {
            await this.props.onSubmit({ attributes, characteristics });
          } catch (error) {
            this.setState({
              submitError: error.message,
            });
          }

          actions.setSubmitting(false);
        }}
        render={({
          errors,
          touched,
          values,
          ...form
        }: FormikProps<FormValues>) => {
          const getError = (value: keyof FormValues): string => {
            if (!touched[value]) {
              return '';
            }

            return typeof errors[value] === 'string'
              ? (errors[value] as any)
              : '';
          };

          return (
            <div ref={this.ref}>
              <ErrorSummary
                errors={this.getSummaryErrors(errors, touched)}
                id="filter-errors"
              />

              <Form>
                <FormFieldSet
                  id="years"
                  legend="Academic years"
                  hint="Filter statistics by a given start and end date"
                >
                  <Field name="startYear">
                    {({ field }: FieldProps) => (
                      <FormGroup hasError={!!getError('startYear')}>
                        <FormSelect
                          {...field}
                          error={getError('startYear')}
                          id="filter-startYear"
                          label="Start year"
                          options={yearOptions}
                        />
                      </FormGroup>
                    )}
                  </Field>
                  <Field name="endYear">
                    {({ field }: FieldProps) => (
                      <FormGroup hasError={!!getError('endYear')}>
                        <FormSelect
                          {...field}
                          error={getError('endYear')}
                          id="filter-endYear"
                          label="End year"
                          options={yearOptions}
                        />
                      </FormGroup>
                    )}
                  </Field>
                </FormFieldSet>

                <FormGroup>
                  <div className="govuk-grid-row">
                    <div className="govuk-grid-column-one-half">
                      <FormFieldSet
                        id="filter-attributes"
                        legend="Attributes"
                        hint="Filter by at least one statistical attribute from the publication"
                        error={getError('attributes')}
                      >
                        {this.renderGroupedOptions('attributes', values)}
                      </FormFieldSet>
                    </div>
                    <div className="govuk-grid-column-one-half">
                      <FormFieldSet
                        id="filter-characteristics"
                        legend="Characteristics"
                        hint="Filter by at least one pupil characteristic from the publication"
                        error={getError('characteristics')}
                      >
                        {this.renderGroupedOptions('characteristics', values)}
                      </FormFieldSet>
                    </div>
                  </div>
                </FormGroup>

                <FormGroup>
                  <h3>Can't find what you're looking for?</h3>

                  <FormTextInput
                    id="characteristic-search"
                    label="Search for an attribute or characteristic"
                    name="characteristicSearch"
                    onChange={event => {
                      event.persist();
                      this.setDebouncedFilterSearch(event);
                    }}
                    width={20}
                  />
                </FormGroup>

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

export default CharacteristicsFilterForm;
