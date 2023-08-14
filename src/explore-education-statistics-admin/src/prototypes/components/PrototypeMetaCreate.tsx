import React, { useState } from 'react';
import Button from '@common/components/Button';
import CreateMetaForms from './PrototypeMetaForms';
import PreviewMeta from './PrototypeMetaPreview';

interface Props {
  publicView?: boolean;
}

const PrototypeMetaCreate = ({ publicView }: Props) => {
  const query = new URLSearchParams(window.location.search);
  const dialog = query.has('showDialog');

  const switchPublicView = publicView;

  const [createMeta, setCreateMeta] = useState(!switchPublicView);
  const [addNewMeta, setAddNewMeta] = useState(false);
  const [previewMeta, setPreviewMeta] = useState(switchPublicView);
  const [editMeta, setEditMeta] = useState(false);

  const formText = {
    descriptionSaved: {
      text: `
        <h2 class="govuk-heading-m">Description</h2>
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
        <p></p>
        <hr />
        <h2>Coverage</h2>
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
        <p></p>
        <hr />
        <h2>File format and conventions</h2>
        <h3 className="govuk-heading-s">Rounding</h3>
        <p>This dataset has not had suppression applied.</p>
        <h3 className="govuk-heading-s">Conventions</h3>
        <p>The following convention is used throughout the underlying data</p>
        <hr />
      `,
    },
    descriptionPlaceholder: {
      text: `
      <h2 class="govuk-heading-m">Description</h2>
      <p>---</p>
      <h2 class="govuk-heading-m">Coverage</h2>
      <p>---</p>
      <h2 class="govuk-heading-m">File format and conventions</h2>
      <p>---</p>
      `,
    },
    subject1: {
      text: 'Absence information for all enrolments in state-funded primary, secondary and special schools including information on overall absence, persistent absence and reason for absence for pupils aged 5-15, based on all 5 half terms data from 2006/07 to 2011/12 inclusive and based on 6 half term data from 2012/13 onwards',
    },
    subject2: {
      text: 'Absence information by pupil characteristics such as age, gender and ethnicity by local authority.',
    },
    subject3: {
      text: 'Absence information by pupil characteristics such as age, gender and ethnicity by local authority district',
    },
  };

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

          <Button
            onClick={() => {
              setAddNewMeta(true);
              setCreateMeta(false);
              setEditMeta(false);
            }}
          >
            Create public metadata
          </Button>
        </>
      )}
      {addNewMeta && (
        <>
          <CreateMetaForms
            editing={editMeta}
            description={
              editMeta
                ? formText.descriptionSaved.text
                : formText.descriptionPlaceholder.text
            }
            subject1={formText.subject1.text}
            subject2={formText.subject2.text}
            subject3={formText.subject3.text}
          />
          <Button
            className=" govuk-!-margin-right-3"
            onClick={() => {
              setPreviewMeta(true);
              setAddNewMeta(false);
            }}
          >
            Preview
          </Button>
        </>
      )}
      {previewMeta && (
        <>
          <PreviewMeta
            description={formText.descriptionSaved.text}
            showDialog={dialog}
          />
          {!switchPublicView && (
            <Button
              className="govuk-!-margin-right-3"
              onClick={() => {
                setPreviewMeta(false);
                setEditMeta(true);
                setAddNewMeta(true);
              }}
            >
              Edit public metadata
            </Button>
          )}
        </>
      )}
    </>
  );
};

export default PrototypeMetaCreate;
