import React, { useState } from 'react';
import FormEditor from '@admin/components/form/FormEditor';
import PageTitle from '@admin/components/PageTitle';
import Button from '@common/components/Button';
import ContentHtml from '@common/components/ContentHtml';

const PrototypeCreatePreRelease = () => {
  // const query = new URLSearchParams(window.location.search);
  // const dialog = query.has('showDialog');

  const [createPreRelease, setCreatePreRelease] = useState(true);
  const [addNewPreRelease, setAddNewPreRelease] = useState(false);
  const [previewPreRelease, setPreviewPreRelease] = useState(false);
  const [editPreRelease, setEditPreRelease] = useState(false);

  const formIntro = `
    <p>Beside Department for Education (DfE) professional and production staff the 
    following post holders are given pre-release access up to 24 hours before release.</p>

    <ul>
        <li>ADD ROLES HERE</li>
    </ul>
`;

  const formText = `
    <p>Beside Department for Education (DfE) professional and production staff the 
    following post holders are given pre-release access up to 24 hours before release.</p>
    <ul>
        <li>
            Secretary of State, DfE
        </li>
        <li>
            Prime Minister
        </li>
    </ul>
`;

  return (
    <>
      {createPreRelease && (
        <>
          <div className="govuk-inset-text">
            <h2 className="govuk-heading-m">Before you start</h2>
            <ul className="govuk-list--bullet">
              <li>
                add a list of roles who have been granted pre-release access to
                this release
              </li>
              <li>
                this page will become publicly facing when the publication is
                released
              </li>
            </ul>
          </div>

          <Button
            onClick={() => {
              setAddNewPreRelease(true);
              setCreatePreRelease(false);
              setEditPreRelease(false);
            }}
          >
            Create public pre-release public access list
          </Button>
        </>
      )}
      {addNewPreRelease && (
        <form
          id="createPreRelease"
          className="govuk-!-margin-bottom-9 govuk-width-container govuk-!-margin-left-0"
        >
          <legend className="govuk-fieldset__legend govuk-fieldset__legend--l">
            <h2 className="govuk-fieldset__heading">
              {editPreRelease ? 'Edit' : 'Create'} public pre-release access
              list
            </h2>
          </legend>
          <div className="govuk-!-margin-bottom-7">
            <FormEditor
              id="description"
              label="Public access list details"
              value={editPreRelease ? formText : formIntro}
              onChange={() => setAddNewPreRelease(true)}
            />
          </div>
          <Button
            className="govuk-!-margin-right-3"
            onClick={() => {
              setPreviewPreRelease(true);
              setAddNewPreRelease(false);
            }}
          >
            Save
          </Button>
          <Button
            variant="secondary"
            onClick={() => {
              setAddNewPreRelease(false);
              setCreatePreRelease(true);
            }}
          >
            Cancel
          </Button>
        </form>
      )}
      {previewPreRelease && (
        <div className="govuk-width-container govuk-!-margin-left-0">
          <PageTitle
            title="An example publication"
            caption="Academic year 2018/19"
          />
          <h2 className="govuk-heading-m">Pre-release access list</h2>

          <h3 className="govuk-heading-s">Published 23 July 2020</h3>

          <div className="govuk-!-margin-top-9 govuk-!-margin-bottom-9">
            <ContentHtml html={formText || ''} />
          </div>
          <Button
            onClick={() => {
              setPreviewPreRelease(false);
              setEditPreRelease(true);
              setAddNewPreRelease(true);
            }}
          >
            Edit public metadata
          </Button>
        </div>
      )}
    </>
  );
};

export default PrototypeCreatePreRelease;
