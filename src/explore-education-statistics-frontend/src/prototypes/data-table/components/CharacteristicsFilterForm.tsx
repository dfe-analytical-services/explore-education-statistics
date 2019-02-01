import { FieldArray, Form, Formik, FormikProps } from 'formik';
import debounce from 'lodash/debounce';
import difference from 'lodash/difference';
import sortBy from 'lodash/sortBy';
import React, { ChangeEvent, Component, createRef } from 'react';
import * as Yup from 'yup';
import Button from '../../../components/Button';
import ErrorSummary, {
  ErrorSummaryMessage,
} from '../../../components/ErrorSummary';
import {
  FormCheckboxGroup,
  FormGroup,
  FormTextInput,
} from '../../../components/form';
import FormFieldSet from '../../../components/form/FormFieldSet';
import { PublicationMeta } from '../../../services/dataTableService';
import MenuDetails from './MenuDetails';

interface FormValues {
  attributes: string[];
  characteristics: string[];
}

export type CharacteristicsFilterFormSubmitHandler = (
  values: {
    attributes: string[];
    characteristics: string[];
  },
) => void;

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

  private renderGroupedOptions(
    groupData: {
      [group: string]: {
        name: string;
        label: string;
      }[];
    },
    fieldKey: keyof FormValues,
    values: string[],
  ) {
    const containSearchTerm = (value: string) =>
      value.search(new RegExp(this.state.searchTerm, 'i')) > -1;

    const groups = Object.entries(groupData)
      .filter(
        ([groupKey]) =>
          this.state.searchTerm === '' ||
          groupData[groupKey].some(
            item =>
              containSearchTerm(item.label) || values.indexOf(item.name) > -1,
          ),
      )
      .map(([groupKey, items]) => {
        const isMenuOpen = groupData[groupKey].some(
          item =>
            (this.state.searchTerm !== '' && containSearchTerm(item.label)) ||
            values.indexOf(item.name) > -1,
        );

        const options = sortBy(
          items
            .filter(
              item =>
                this.state.searchTerm === '' ||
                containSearchTerm(item.label) ||
                values.indexOf(item.name) > -1,
            )
            .map(item => {
              return {
                id: item.name,
                label: item.label,
                value: item.name,
              };
            }),
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
            <FieldArray name={fieldKey}>
              {({ form, ...helpers }) => (
                <FormCheckboxGroup
                  checkedValues={checkedState}
                  name={fieldKey}
                  id={`${fieldKey}-${groupKey}`}
                  onAllChange={event => {
                    if (this.state.searchTerm !== '') {
                      return;
                    }

                    const currentValues = (form.values as FormValues)[fieldKey];
                    const allValues = groupData[groupKey].map(
                      value => value.name,
                    );
                    const diff = difference(currentValues, allValues);

                    if (event.target.checked) {
                      form.setFieldValue(fieldKey, [...diff, ...allValues]);
                    } else {
                      form.setFieldValue(fieldKey, diff);
                    }
                  }}
                  onChange={event => {
                    const currentValues = (form.values as FormValues)[fieldKey];

                    if (event.target.checked) {
                      helpers.push(event.target.value);
                    } else {
                      const index = currentValues.indexOf(event.target.value);

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
    const { publicationMeta } = this.props;
    const { submitError } = this.state;

    return (
      <Formik
        initialValues={{
          attributes: [],
          characteristics: [],
        }}
        validationSchema={Yup.object({
          attributes: Yup.array().required('Select at least one option'),
          characteristics: Yup.array().required('Select at least one option'),
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
          const summaryErrors: ErrorSummaryMessage[] = Object.entries(errors)
            .filter(([errorName]) => touched[errorName as keyof FormValues])
            .map(([errorName, message]) => ({
              id: `${errorName}-filters`,
              message: typeof message === 'string' ? message : '',
            }));

          if (submitError) {
            summaryErrors.push({
              id: 'submit-button',
              message: 'Could not submit filters. Please try again later.',
            });
          }

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
              <ErrorSummary errors={summaryErrors} id="filter-errors" />

              <Form>
                <FormGroup>
                  <FormTextInput
                    id="characteristic-search"
                    label="Search for a characteristic or attribute"
                    name="characteristicSearch"
                    onChange={event => {
                      event.persist();
                      this.setDebouncedFilterSearch(event);
                    }}
                    width={20}
                  />
                </FormGroup>

                <div className="govuk-grid-row">
                  <div className="govuk-grid-column-one-half">
                    <FormFieldSet
                      id="attributes-filters"
                      legend="Attributes"
                      error={getError('attributes')}
                    >
                      {this.renderGroupedOptions(
                        publicationMeta.attributes,
                        'attributes',
                        values.attributes,
                      )}
                    </FormFieldSet>
                  </div>
                  <div className="govuk-grid-column-one-half">
                    <FormFieldSet
                      id="characteristics-filters"
                      legend="Characteristics"
                      error={getError('characteristics')}
                    >
                      {this.renderGroupedOptions(
                        publicationMeta.characteristics,
                        'characteristics',
                        values.characteristics,
                      )}
                    </FormFieldSet>
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

export default CharacteristicsFilterForm;
