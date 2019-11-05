import React from 'react';
import { Footnote } from '.';
import { dummyFootnotes, dummyFootnoteMeta } from './dummyFootnoteData';

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
      <button type="button" onClick={onOpen}>
        Add {!isFirst && ` another `}footnote
      </button>
    ) : (
      <>
        newfootnote form
        <button type="button" onClick={onCancel}>
          cancel
        </button>
        {onSubmit && (
          <button type="button" onClick={() => onSubmit(dummyFootnotes[0])}>
            submit
          </button>
        )}
      </>
    );
  };

  const renderEditForm = () => {
    if (state !== 'edit') {
      return null;
    }
    return (
      <>
        edittfootnote form
        <button type="button" onClick={onCancel}>
          cancel
        </button>
        {onSubmit && (
          <button
            type="button"
            onClick={() => onSubmit(dummyFootnotes[0], dummyFootnotes[0].id)}
          >
            Update
          </button>
        )}
      </>
    );
  };

  return !footnote ? renderNewForm() : renderEditForm();
};

export default FootnoteForm;
