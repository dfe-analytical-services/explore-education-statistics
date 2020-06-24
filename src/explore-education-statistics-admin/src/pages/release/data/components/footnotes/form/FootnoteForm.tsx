import FieldSubjectCheckbox from '@admin/pages/release/data/components/footnotes/form/FieldSubjectCheckbox';
import FilterGroupDetails from '@admin/pages/release/data/components/footnotes/form/FilterGroupDetails';
import styles from '@admin/pages/release/data/components/footnotes/form/FootnoteForm.module.scss';
import IndicatorDetails from '@admin/pages/release/data/components/footnotes/form/IndicatorDetails';
import { FootnoteMetaGetters } from '@admin/pages/release/data/utils/generateFootnoteMetaMap';
import {
  BaseFootnote,
  Footnote,
  FootnoteMeta,
  FootnoteSubjectMeta,
} from '@admin/services/footnoteService';
import footnoteToFlatFootnote from '@admin/services/utils/footnote/footnoteToFlatFootnote';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldset } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import createErrorHelper from '@common/validation/createErrorHelper';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import get from 'lodash/get';
import merge from 'lodash/merge';
import React from 'react';

const footnoteFormValidation = (footnote: BaseFootnote) => {
  const errors: { [key: string]: string } = {};
  if (footnote) {
    const {
      subjects,
      indicatorGroups,
      indicators,
      filters,
      filterGroups,
      filterItems,
    } = footnoteToFlatFootnote(footnote);

    const atLeastOneOption =
      [
        ...subjects,
        ...indicators,
        ...indicatorGroups,
        ...filters,
        ...filterGroups,
        ...filterItems,
      ].length === 0 &&
      'At least one Subject, Indicator or Filter must be selected';
    if (atLeastOneOption) {
      errors.subjects = atLeastOneOption;
    }
  }
  return errors;
};

export interface FootnoteFormConfig {
  state: 'create' | 'edit' | 'cancel';
  footnote?: Footnote;
}

interface Props extends FootnoteFormConfig {
  footnoteMeta: FootnoteMeta;
  footnoteMetaGetters: FootnoteMetaGetters;
  isFirst?: boolean;
  onOpen: () => void;
  onCancel: () => void;
  onSubmit?: (values: BaseFootnote, id?: string) => void;
}

export interface FootnoteFormControls {
  footnoteForm: FootnoteFormConfig;
  create: () => void;
  edit: (footnote: Footnote) => void;
  cancel: () => void;
  save: (footnote: BaseFootnote, footnoteId?: string) => void;
  delete: (footnote: Footnote) => void;
}

const FootnoteForm = ({
  isFirst = true,
  state,
  footnote,
  onOpen,
  onCancel,
  onSubmit,
  footnoteMeta,
}: Props) => {
  const generateInitialValues = () => {
    /**
     * This function can be removed once the id's of
     * filters, filterItems etc. aren't numbers
     * We can just use the initialValue below.
     */
    const initialValue: BaseFootnote = {
      content: '',
      subjects: {},
    };

    Object.entries(footnoteMeta).map(([subjectId, subject]) => {
      initialValue.subjects[subjectId] = {
        indicatorGroups: {},
        filters: {},
        selected: false,
      };
      Object.keys(subject.indicators).map(indicatorId => {
        initialValue.subjects[subjectId].indicatorGroups[`${indicatorId}`] = {
          selected: false,
          indicators: [],
        };
        return null;
      });
      return Object.entries(subject.filters).map(([filterId, filter]) => {
        initialValue.subjects[subjectId].filters[`${filterId}`] = {
          selected: false,
          filterGroups: {},
        };
        return Object.keys(filter.options).map(filterGroupId => {
          initialValue.subjects[subjectId].filters[`${filterId}`].filterGroups[
            `${filterGroupId}`
          ] = {
            selected: false,
            filterItems: [],
          };
          return null;
        });
      });
    });

    return merge(initialValue, footnote);
  };

  const renderForm = () => {
    const formId = `${(footnote && footnote.id) || 'create'}-footnote-form`;

    return (
      <Formik<BaseFootnote>
        // @ts-ignore
        initialValues={generateInitialValues()}
        validate={footnoteFormValidation}
        validationSchema={Yup.object({
          content: Yup.string().required('Footnote content must be added.'),
        })}
        onSubmit={values => {
          return (
            onSubmit && onSubmit(values, footnote ? footnote.id : undefined)
          );
        }}
      >
        {form => {
          const { getError } = createErrorHelper(form);
          return (
            <div className={styles.container}>
              <Form {...form} id={formId}>
                <FormFieldset
                  id={`${formId}-allFieldsFieldset`}
                  legend={!footnote ? 'Create new footnote' : 'Edit footnote'}
                  error={getError('subjects')}
                >
                  <p>
                    Select filters and indicators from either one or multiple
                    subject areas and then add your footnote (shown at the
                    bottom of the page)
                  </p>
                  {Object.entries(footnoteMeta)
                    .sort((a, b) => {
                      const textA = a[1].subjectName.toUpperCase();
                      const textB = b[1].subjectName.toUpperCase();
                      return textA < textB ? -1 : 1;
                    })
                    .map(
                      ([subjectMetaId, subjectMeta]: [
                        string,
                        FootnoteSubjectMeta,
                      ]) => {
                        const subjectSelected =
                          get(
                            form.values,
                            `subjects.${subjectMetaId}.selected`,
                          ) || false;
                        return (
                          <fieldset
                            key={subjectMetaId}
                            className="govuk-fieldset"
                            datatest-id={`footnote-subject ${subjectMeta.subjectName}`}
                          >
                            <legend className="govuk-heading-m">
                              Subject: {subjectMeta.subjectName}
                            </legend>
                            <div>
                              <FieldSubjectCheckbox
                                id={`subject-${subjectMetaId}`}
                                label="Select all indicators and filters for this subject"
                                name={`subjects.${subjectMetaId}.selected`}
                                className="govuk-checkboxes--small"
                              />
                            </div>
                            <div
                              key={subjectMetaId}
                              className="govuk-grid-row govuk-!-margin-top-3 "
                            >
                              <div className="govuk-grid-column-one-half">
                                <h3 className="govuk-heading-s govuk-!-margin-bottom-2 govuk-!-margin-top-0">
                                  Indicators
                                </h3>
                                <IndicatorDetails
                                  summary="Indicators"
                                  parentSelected={subjectSelected}
                                  valuePath={`subjects.${subjectMetaId}`}
                                  indicator={subjectMeta.indicators}
                                  form={form}
                                />
                              </div>
                              <div className="govuk-grid-column-one-half">
                                <h3 className="govuk-heading-s govuk-!-margin-bottom-2 govuk-!-margin-top-0">
                                  Filters
                                </h3>
                                {Object.entries(subjectMeta.filters).map(
                                  ([filterId, filter]: [
                                    string,
                                    FootnoteSubjectMeta['filters']['0'],
                                  ]) => (
                                    <FilterGroupDetails
                                      key={filterId}
                                      summary={filter.legend}
                                      parentSelected={subjectSelected}
                                      valuePath={`subjects.${subjectMetaId}`}
                                      groupId={filterId}
                                      filter={filter}
                                      selectAll
                                      value={get(
                                        form.values,
                                        `subjects.${subjectMetaId}.filters.${filterId}.selected`,
                                      )}
                                      form={form}
                                    />
                                  ),
                                )}
                              </div>
                            </div>
                            <hr className="govuk-!-margin-bottom-2" />
                          </fieldset>
                        );
                      },
                    )}

                  <FormFieldTextArea<BaseFootnote>
                    id={`${formId}-content`}
                    name="content"
                    label="Footnote"
                  />
                  <Button
                    type="submit"
                    className="govuk-button govuk-!-margin-right-6"
                  >
                    {`${!footnote ? 'Create' : 'Update'} footnote`}
                  </Button>
                  <ButtonText
                    className="govuk-button govuk-button--secondary"
                    onClick={onCancel}
                  >
                    Cancel
                  </ButtonText>
                </FormFieldset>
              </Form>
            </div>
          );
        }}
      </Formik>
    );
  };

  const renderNewForm = () => {
    return state !== 'create' ? (
      <Button
        type="button"
        id="add-footnote-button"
        className="govuk-button"
        onClick={onOpen}
      >
        Add {!isFirst && `another `}footnote
      </Button>
    ) : (
      renderForm()
    );
  };

  const renderEditForm = () => {
    if (state !== 'edit') {
      return null;
    }
    return renderForm();
  };

  return !footnote ? renderNewForm() : renderEditForm();
};

export default FootnoteForm;
