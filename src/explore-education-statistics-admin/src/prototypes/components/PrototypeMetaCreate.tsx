import React, { useState } from 'react';
import CreateMetaForms from './PrototypeMetaForms';
import PreviewMeta from './PrototypeMetaPreview';

const CreateMeta = () => {
  const [createMeta, setCreateMeta] = useState(true);
  const [addNewMeta, setAddNewMeta] = useState(false);
  const [previewMeta, setPreviewMeta] = useState(false);
  const [editMeta, setEditMeta] = useState(false);

  return (
    <>
      {createMeta && (
        <>
          <div className="govuk-inset-text">
            <h2 className="govuk-heading-m">Before you start</h2>
            <ul className="govuk-list--bullet">
              <li>
                ensure you have uploaded all your subject files before creating
                the public metadata document
              </li>
              <li>
                you can create the document and change subject files later, but
                you may need to re-edit the content
              </li>
            </ul>
          </div>

          <button
            className="govuk-button"
            type="submit"
            onClick={() => {
              setAddNewMeta(true);
              setCreateMeta(false);
              setEditMeta(false);
            }}
          >
            Create public metadata
          </button>
        </>
      )}
      {addNewMeta && (
        <>
          <CreateMetaForms editing={editMeta} />
          <button
            className="govuk-button govuk-!-margin-right-3"
            type="submit"
            onClick={() => {
              setPreviewMeta(true);
              setAddNewMeta(false);
            }}
          >
            Save
          </button>
          <button
            className="govuk-button govuk-button--secondary"
            type="submit"
            onClick={() => {
              setAddNewMeta(false);
              setCreateMeta(true);
            }}
          >
            Cancel
          </button>
        </>
      )}
      {previewMeta && (
        <>
          <PreviewMeta />
          <button
            className="govuk-button govuk-!-margin-right-3"
            type="submit"
            onClick={() => {
              setPreviewMeta(false);
              setEditMeta(true);
              setAddNewMeta(true);
            }}
          >
            Edit public metadata
          </button>
        </>
      )}
    </>
  );
};

export default CreateMeta;
