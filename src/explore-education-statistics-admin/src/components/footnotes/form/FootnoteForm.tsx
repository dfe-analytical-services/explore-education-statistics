import Link from '@admin/components/Link';
import {
  Footnote,
  FootnoteMeta,
  FootnoteMetaGetters,
  FootnoteProps,
  FootnoteSubjectMeta,
} from '@admin/services/release/edit-release/footnotes/types';
import footnoteFormValidation from '@admin/services/release/edit-release/footnotes/util';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import get from 'lodash/get';
import { FormikProps } from 'formik';
import React, { createContext } from 'react';
import FieldSubjectCheckbox from './FieldSubjectCheckbox';
import styles from './FootnoteForm.module.scss';

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
  onSubmit?: (values: FootnoteProps, id?: number) => void;
}

export interface FootnoteFormControls {
  footnoteForm: FootnoteFormConfig;
  create: () => void;
  edit: (footnote: Footnote) => void;
  cancel: () => void;
  save: (footnote: FootnoteProps, footnoteId?: number) => void;
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
  const renderForm = () => {
    const formId = `${(footnote && footnote.id) || 'create'}-footnote-form`;

    return (
      <Formik<FootnoteProps>
        // @ts-ignore
        initialValues={
          footnote || {
            content: '',
            subjects: {},
          }
        }
        validate={footnoteFormValidation}
        validationSchema={Yup.object({
          content: Yup.string().required('Footnote content must be added.'),
        })}
        onSubmit={values => {
          return (
            onSubmit && onSubmit(values, footnote ? footnote.id : undefined)
          );
        }}
        render={(form: FormikProps<FootnoteProps>) => {
          const { getError } = createErrorHelper(form);
          return (
            <div className={styles.container}>
              <Form {...form} id={formId}>
                <p>Select either one or multiple subject areas from below</p>
                <hr className="govuk-!-margin-top-1 govuk-!-margin-bottom-2" />
                <FormFieldset
                  id={`${formId}-allFieldsFieldset`}
                  legend={!footnote ? 'Create new footnote' : 'Edit footnote'}
                  legendHidden
                  error={getError('subjects')}
                >
                  {Object.entries(footnoteMeta).map(
                    ([subjectMetaId, subjectMeta]: [
                      string,
                      FootnoteSubjectMeta,
                    ]) => (
                      <div key={subjectMetaId}>
                        <div key={subjectMetaId} className="govuk-grid-row">
                          <h4 className="govuk-visually-hidden">
                            {subjectMeta.subjectName} footnote matching criteria
                          </h4>
                          <div className="govuk-grid-column-one-third">
                            <h5 className="govuk-!-margin-bottom-2 govuk-!-margin-top-0">
                              Subject
                            </h5>
                            <FieldSubjectCheckbox
                              id={subjectMetaId}
                              label={subjectMeta.subjectName}
                              name={`subjects.${subjectMetaId}.selected`}
                            />
                          </div>
                          <div className="govuk-grid-column-one-third">
                            <h5 className="govuk-!-margin-bottom-2 govuk-!-margin-top-0">
                              Indicators
                            </h5>
                            <Details
                              summary={`Indicators ${null ? '(All)' : ''}`}
                            >
                              IndicatorGroups here..
                              {/* <FormFieldCheckboxGroupsMenu<FormValues>
                                        name="indicators"
                                        id="indicators"
                                        legend="Indicators"
                                        legendHidden
                                        error={getError('indicators')}
                                        selectAll
                                        options={Object.values(
                                          subjectMeta.indicators,
                                        ).map(indicatorGroup => {
                                          return {
                                            legend: indicatorGroup.label,
                                            options: Object.values(
                                              indicatorGroup.options,
                                            ),
                                          };
                                        })}
                                      />
                                      */}
                            </Details>
                          </div>
                          <div className="govuk-grid-column-one-third">
                            <h5 className="govuk-!-margin-bottom-2 govuk-!-margin-top-0">
                              Filters
                            </h5>
                            {/*
                          <FormFieldset
                            id={`${formId}-filters`}
                            legend="Categories"
                            legendHidden
                            error={getError('filters')}
                          >
                            <CollapsibleList collapseAfter={5}>
                              {Object.entries(subjectMeta.filters).map(
                                ([filterId, filter]) => {
                                  const filterName = `filterItems`;

                                  return (
                                    <FormFieldCheckboxGroupsMenu<FormValues>
                                      key={filterId}
                                      name={filterName}
                                      id={`${formId}-${camelCase(filterName)}`}
                                      legend={filter.legend}
                                      hint={filter.hint}
                                      error={getError(filterName)}
                                      selectAll
                                      options={Object.values(
                                        filter.options,
                                      ).map(filterGroup => {
                                        return {
                                          legend: filterGroup.label,
                                          options: Object.values(
                                            filterGroup.options,
                                          ),
                                        };
                                      })}
                                    />
                                  );
                                },
                              )}
                            </CollapsibleList>
                          </FormFieldset>
                        */}
                          </div>
                        </div>
                        <hr className="govuk-!-margin-0 govuk-!-margin-bottom-2" />
                      </div>
                    ),
                  )}
                </FormFieldset>
                <FormFieldTextArea<FootnoteProps>
                  id={`${formId}-content`}
                  name="content"
                  label="Footnote"
                />
                <Button
                  type="submit"
                  className="govuk-button govuk-!-margin-right-3"
                >
                  {!footnote ? 'Create' : 'Update'} Footnote
                </Button>
                <Link
                  to="#"
                  className="govuk-button govuk-button--secondary"
                  onClick={onCancel}
                >
                  Cancel
                </Link>
              </Form>
            </div>
          );
        }}
      />
    );
  };

  const renderNewForm = () => {
    return state !== 'create' ? (
      <Button type="button" className="govuk-button" onClick={onOpen}>
        Add {!isFirst && ` another `}footnote
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
