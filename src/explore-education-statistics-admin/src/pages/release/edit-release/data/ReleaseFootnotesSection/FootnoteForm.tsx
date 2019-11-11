import {
  Footnote,
  FootnoteProps,
  FootnoteMeta,
  FootnoteMetaGetters,
  FootnoteSubjectMeta,
} from '@admin/services/release/edit-release/footnotes/types';
// import footnoteFormValidation from '@admin/services/release/edit-release/footnotes/util';
import Link from '@admin/components/Link';
import Button from '@common/components/Button';
import {
  Formik,
  Form,
  FormFieldset,
  FormFieldCheckboxGroup,
} from '@common/components/form';
import Yup from '@common/lib/validation/yup';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import FormFieldCheckboxGroupsMenu from '@common/modules/table-tool/components/FormFieldCheckboxGroupsMenu';
import { FormValues } from '@common/modules/table-tool/components/FiltersForm';
import CollapsibleList from '@common/components/CollapsibleList';
import React from 'react';
import camelCase from 'lodash/camelCase';
import { FormikProps } from 'formik';
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
  delete: (footnoteId: number) => void;
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
        initialValues={
          footnote || {
            content: '',
            subjects: [],
            indicators: [],
            filters: [],
            filterGroups: [],
            filterItems: [],
          }
        }
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
            <Form id={formId}>
              <p>Select either one or multiple subject areas from below</p>
              <div className="govuk-grid-row govuk-heading-s govuk-!-margin-bottom-0">
                <div className="govuk-grid-column-one-third">Subject</div>
                <div className="govuk-grid-column-one-third">Indicator</div>
                <div className="govuk-grid-column-one-third">Filter</div>
              </div>
              <hr className="govuk-!-margin-top-1 govuk-!-margin-bottom-2" />
              <FormFieldset
                id={`${formId}-allFieldsFieldset`}
                legend={!footnote ? 'Create new footnote' : 'Edit footnote'}
                legendHidden
              >
                {Object.entries(footnoteMeta).map(
                  ([subjectMetaId, subjectMeta]: [
                    string,
                    FootnoteSubjectMeta,
                  ]) => (
                    <div key={subjectMetaId}>
                      <div key={subjectMetaId} className="govuk-grid-row">
                        <div className="govuk-grid-column-one-third">
                          <FormFieldCheckboxGroup
                            name="subjects"
                            id={`${formId}-subjects-formFieldSet`}
                            legend="Subject"
                            legendHidden
                            error={getError('subjects')}
                            options={[
                              {
                                label: subjectMeta.subjectName,
                                value: `${subjectMeta.subjectId}`,
                              },
                            ]}
                          />
                        </div>
                        <div className="govuk-grid-column-one-third">
                          <FormFieldCheckboxGroupsMenu<FormValues>
                            name="indicators"
                            id={`${formId}-indicators`}
                            legend="Indicators"
                            legendHidden
                            error={getError('indicators')}
                            selectAll
                            options={Object.entries(subjectMeta.indicators).map(
                              ([indicatorGroupId, indicatorGroup]) => {
                                return {
                                  legend: indicatorGroup.label,
                                  options: Object.values(
                                    indicatorGroup.options,
                                  ),
                                };
                              },
                            )}
                          />
                        </div>
                        <div className="govuk-grid-column-one-third">
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
                                      options={Object.entries(
                                        filter.options,
                                      ).map(([filterGroupId, filterGroup]) => {
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
                        </div>
                      </div>
                      <hr className="govuk-!-margin-0 govuk-!-margin-bottom-2" />
                    </div>
                  ),
                )}
                <FormFieldTextArea<FootnoteProps>
                  id={`${formId}-content`}
                  name="content"
                  label="Footnote"
                />
              </FormFieldset>
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
      <div className={styles.container}>{renderForm()}</div>
    );
  };

  const renderEditForm = () => {
    if (state !== 'edit') {
      return null;
    }
    return (
      <td colSpan={5} className="govuk-body-m">
        {renderForm()}
      </td>
    );
  };

  return !footnote ? renderNewForm() : renderEditForm();
};

export default FootnoteForm;
