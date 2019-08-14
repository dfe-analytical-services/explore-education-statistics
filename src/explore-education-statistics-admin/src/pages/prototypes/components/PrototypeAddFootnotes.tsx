import classNames from 'classnames';
import React, { useState } from 'react';
import Link from '@admin/components/Link';
import Details from '@common/components/Details';
import PrototypeFootnotes from './PrototypeFootnotes';

const PrototypeAddFootnotes = () => {
  const [addNewFootnote, setAddNewFootnote] = useState(false);
  const [addAnotherFootnote, setAddAnotherFootnote] = useState(false);

  return (
    <>
      {addNewFootnote && (
        <>
          <PrototypeFootnotes />
          <button
            className="govuk-button govuk-!-margin-right-3"
            type="submit"
            onClick={() => setAddAnotherFootnote(true)}
          >
            Add another footnote
          </button>

          <button
            className="govuk-button govuk-button--secondary govuk-!-margin-right-3"
            type="submit"
            onClick={() => setAddNewFootnote(false)}
          >
            Delete this footnote
          </button>
          <hr />
        </>
      )}
      {addAnotherFootnote && (
        <>
          <PrototypeFootnotes />
          <button
            className="govuk-button govuk-!-margin-right-3"
            type="submit"
            onClick={() => setAddNewFootnote(true)}
          >
            Add another footnote
          </button>
          <button
            className="govuk-button govuk-button--secondary govuk-!-margin-right-3"
            type="submit"
            onClick={() => setAddNewFootnote(false)}
          >
            Delete this footnote
          </button>
          <hr />
        </>
      )}
      {!addNewFootnote && (
        <button
          className="govuk-button govuk-!-margin-right-3"
          type="submit"
          onClick={() => {
            setAddNewFootnote(true);
          }}
        >
          Add footnote
        </button>
      )}
    </>
  );
};

export default PrototypeAddFootnotes;
