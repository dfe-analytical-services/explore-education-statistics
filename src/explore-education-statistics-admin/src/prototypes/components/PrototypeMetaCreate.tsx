import React, { useState, Fragment } from 'react';
import CreateMetaForms from './PrototypeMetaForms';
import PreviewMeta from './PrototypeMetaPreview';

const CreateMeta = () => {
  const [createMeta, setCreateMeta] = useState(true);
  const [addNewMeta, setAddNewMeta] = useState(false);
  const [previewMeta, setPreviewMeta] = useState(false);
  const [editMeta, setEditMeta] = useState(false);

  const formDescription = `
      <section>
        <h2 className="govuk-heading-m">Description</h2>
        <p>
          This document describes the data included in the ‘Pupil absence in
          schools in England: 2018/19’ National Statistics release’s underlying
          data files. This data is released under the terms of the Open
          Government License and is intended to meet at least 3 stars for Open
          Data
        </p>
        <p>
          The Guide to absence statistics should be referenced alongside this
          data. It provides information on the data sources, their coverage and
          quality as well as explaining methodology used in producing the data.
        </p>
      </section>
      <section>
        <h2 className="govuk-heading-m">Coverage</h2>
        <p>
          This release provides information on the levels of overall, authorised
          and unauthorised absence in:
        </p>
        <ul className="govuk-list--bullet">
          <li>state-funded primary schools</li>
          <li>state-funded secondary schools</li>
          <li>state-funded special schools</li>
        </ul>
        <p>it includes information on:</p>

        <ul className="govuk-list--bullet">
          <li>reasons for absence</li>
          <li>persistent absentees</li>
          <li>pupil characteristics</li>
          <li>absence information for pupil referral units</li>
          <li>absence by term</li>
        </ul>

        <p>
          The information is based on pupil level absence data collected via the
          school census.
        </p>

        <p>
          A guide on how we produce pupil absence statistics is also available
          with further detail on the methods used. The underlying data files
          include national, regional, local authority, local authority district
          and school level absence information from 2006/07 to 2018/19 for
          schools in England.
        </p>
      </section>
      <section>
        <h2 className="govuk-heading-m">File formats and conventions</h2>
        <h3 className="govuk-heading-s">Rounding</h3>
        <p>This dataset has not had suppression applied.</p>
        <h3 className="govuk-heading-s">Conventions</h3>
        <p>The following convention is used throughout the underlying data</p>
      </section>
  `;

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
          <CreateMetaForms editing={editMeta} description={formDescription} />
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
          <PreviewMeta description={formDescription} />
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
