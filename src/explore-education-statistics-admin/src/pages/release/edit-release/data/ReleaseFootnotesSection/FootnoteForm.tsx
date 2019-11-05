import {
  Footnote,
  FootnoteProps,
} from '@admin/services/release/edit-release/footnotes/types';
import Link from '@admin/components/Link';
import Button from '@common/components/Button';
import React from 'react';
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
  onSubmit?: (values: Footnote, id?: string) => void;
}

export interface FootnoteFormControls {
  footnoteForm: FootnoteFormConfig;
  create: () => void;
  edit: (footnote: Footnote) => void;
  cancel: () => void;
  save: (footnote: Footnote, footnoteId?: string | undefined) => void;
}

const FootnoteForm = ({
  isFirst = true,
  state,
  footnote,
  onOpen,
  onCancel,
  onSubmit,
}: Props) => {
  const renderNewForm = () => {
    return state !== 'create' ? (
      <Button type="button" className="govuk-button" onClick={onOpen}>
        Add {!isFirst && ` another `}footnote
      </Button>
    ) : (
      <div className={styles.container}>
        <h4>Create new footnote</h4>
        <Button
          type="submit"
          className="govuk-button govuk-!-margin-right-6"
          onClick={() => onSubmit && onSubmit(dummyFootnotes[0])}
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
      </div>
    );
  };

  const renderEditForm = () => {
    if (state !== 'edit') {
      return null;
    }
    return (
      <td colSpan={5} className="govuk-body-m">
        <h4>Edit footnote</h4>
        <Button
          type="submit"
          className="govuk-button govuk-!-margin-right-6"
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
      </td>
    );
  };

  return !footnote ? renderNewForm() : renderEditForm();
};

export default FootnoteForm;
