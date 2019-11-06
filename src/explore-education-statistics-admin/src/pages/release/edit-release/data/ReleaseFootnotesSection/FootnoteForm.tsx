import {
  Footnote,
  FootnoteProps,
} from '@admin/services/release/edit-release/footnotes/types';
// import footnoteFormValidation from '@admin/services/release/edit-release/footnotes/util';
import Link from '@admin/components/Link';
import Button from '@common/components/Button';
import { Formik, Form, FormFieldset } from '@common/components/form';
import Yup from '@common/lib/validation/yup';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import React, { ReactNode } from 'react';
import { FormikProps } from 'formik';
import { dummyFootnotes } from './dummyFootnoteData';
import styles from './FootnoteForm.module.scss';

export interface FootnoteFormConfig {
  state: 'create' | 'edit' | 'cancel';
  footnote?: Footnote;
}

interface Props extends FootnoteFormConfig {
  isFirst?: boolean;
  onOpen: () => void;
  onCancel: () => void;
  onSubmit?: (values: FootnoteProps, id?: string) => void;
}

export interface FootnoteFormControls {
  footnoteForm: FootnoteFormConfig;
  create: () => void;
  edit: (footnote: Footnote) => void;
  cancel: () => void;
  save: (footnote: FootnoteProps, footnoteId?: string | undefined) => void;
}

const FootnoteForm = ({
  isFirst = true,
  state,
  footnote,
  onOpen,
  onCancel,
  onSubmit,
}: Props) => {
  const renderForm = (confirmControls: ReactNode) => {
    const formId = (footnote && footnote.id) || 'new-footnote';
    return (
      <Formik<FootnoteProps>
        initialValues={footnote || { content: '' }}
        validationSchema={{
          content: Yup.string().required('Footnote content must be added.'),
        }}
        onSubmit={values => {
          return (
            onSubmit && onSubmit(values, footnote ? footnote.id : undefined)
          );
        }}
        render={(form: FormikProps<FootnoteProps>) => {
          return (
            <Form id={formId}>
              <FormFieldset
                id={`${formId}-allFieldsFieldset`}
                legend={!footnote ? 'Create new footnote' : 'Edit footnote'}
              >
                <FormFieldTextArea<FootnoteProps>
                  id={`${formId}-content`}
                  name="content"
                  label="Footnote"
                />
              </FormFieldset>
              {confirmControls}
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
      <div className={styles.container}>
        {renderForm(
          <>
            <Button
              type="submit"
              className="govuk-button govuk-!-margin-right-3"
            >
              Create Footnote
            </Button>
            <Link
              to="#"
              className="govuk-button govuk-button--secondary"
              onClick={onCancel}
            >
              Cancel
            </Link>
          </>,
        )}
      </div>
    );
  };

  const renderEditForm = () => {
    if (state !== 'edit') {
      return null;
    }
    return (
      <td colSpan={5} className="govuk-body-m">
        {renderForm(
          <>
            <Button
              type="submit"
              className="govuk-button govuk-!-margin-right-3"
              onClick={() =>
                onSubmit && onSubmit(dummyFootnotes[0], dummyFootnotes[0].id)
              }
            >
              Update Footnote
            </Button>
            <Link
              to="#"
              className="govuk-button govuk-button--secondary"
              onClick={onCancel}
            >
              Cancel
            </Link>
          </>,
        )}
      </td>
    );
  };

  return !footnote ? renderNewForm() : renderEditForm();
};

export default FootnoteForm;
