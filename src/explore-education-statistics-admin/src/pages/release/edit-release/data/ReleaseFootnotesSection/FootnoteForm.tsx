import {
  Footnote,
  FootnoteProps,
  FootnoteMeta,
  FootnoteMetaGetters,
} from '@admin/services/release/edit-release/footnotes/types';
// import footnoteFormValidation from '@admin/services/release/edit-release/footnotes/util';
import Link from '@admin/components/Link';
import Button from '@common/components/Button';
import { Formik, Form, FormFieldset } from '@common/components/form';
import Yup from '@common/lib/validation/yup';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import React from 'react';
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
  save: (footnote: FootnoteProps, footnoteId?: number | undefined) => void;
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
        initialValues={footnote || { content: '' }}
        validationSchema={Yup.object<FootnoteProps>({
          content: Yup.string().required('Footnote content must be added.'),
        })}
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
                legendHidden
              >
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
