import React from 'react';
import { Footnote } from '.';
import { dummyFootnotes } from './dummyFootnoteData';

export interface FootnoteFormConfig {
  state: 'create' | 'edit' | 'cancel';
  footnote?: Footnote;
}

interface Props extends FootnoteFormConfig {
  onOpen: () => void;
  onCancel: () => void;
  onSubmit?: (values: Footnote, id?: string) => void;
}

const FootnoteForm = ({
  state,
  footnote,
  onOpen,
  onCancel,
  onSubmit,
}: Props) => {
  const renderNewForm = () => {
    return state === 'cancel' ? (
      <button type="button" onClick={onOpen}>
        Add new footnote{' '}
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
    return state === 'edit' ? <>edittfootnote form</> : null;
  };

  return !footnote ? renderNewForm() : renderEditForm();
};

export default FootnoteForm;
