import { FieldArray, Form, Formik, FormikProps } from 'formik';
import debounce from 'lodash/debounce';
import difference from 'lodash/difference';
import sortBy from 'lodash/sortBy';
import React, { ChangeEvent, Component, createRef } from 'react';
import * as Yup from 'yup';
import Button from '../../../components/Button';
import {
  FormCheckboxGroup,
  FormGroup,
  FormTextInput,
} from '../../../components/form';
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
}

class CharacteristicsFilterForm extends Component<Props, State> {
  public state = {
    errors: {
      attributes: '',
      characteristics: '',
    },
    isSubmitted: false,
    searchTerm: '',
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
    fieldKey: 'characteristics' | 'attributes',
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
          await this.props.onSubmit({ attributes, characteristics });

          actions.setSubmitting(false);
        }}
        render={({
          errors,
          touched,
          values,
          ...form
        }: FormikProps<FormValues>) => (
          <div ref={this.ref}>
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
                  <FormGroup
                    hasError={
                      errors.attributes !== undefined &&
                      touched.attributes !== undefined
                    }
                  >
                    <h3>Attributes</h3>

                    {typeof errors.attributes === 'string' &&
                      touched.attributes !== undefined && (
                        <span className="govuk-error-message">
                          {errors.attributes}
                        </span>
                      )}

                    {this.renderGroupedOptions(
                      publicationMeta.attributes,
                      'attributes',
                      values.attributes,
                    )}
                  </FormGroup>
                </div>
                <div className="govuk-grid-column-one-half">
                  <FormGroup
                    hasError={
                      errors.characteristics !== undefined &&
                      touched.characteristics !== undefined
                    }
                  >
                    <h3>Characteristics</h3>

                    {typeof errors.characteristics === 'string' &&
                      touched.characteristics !== undefined && (
                        <span className="govuk-error-message">
                          {errors.characteristics}
                        </span>
                      )}

                    {this.renderGroupedOptions(
                      publicationMeta.characteristics,
                      'characteristics',
                      values.characteristics,
                    )}
                  </FormGroup>
                </div>
              </div>

              <Button
                disabled={form.isSubmitting}
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
        )}
      />
    );
  }
}

export default CharacteristicsFilterForm;
