import React, { useState } from 'react';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import FormEditor, { FormEditorProps } from '@admin/components/form/FormEditor';
import {
  Form,
  FormFieldTextInput,
  FormGroup,
  FormTextInput,
} from '@common/components/form';

import FormTextArea from '@common/components/form/FormTextArea';
import MetaVariables from './PrototypeMetaVariables';

interface Props {
  editing?: boolean;
}

const CreateMetaForms = (editing: Props) => {
  const [addNewMeta, setAddNewMeta] = useState(false);

  const descriptionPlaceholder = `
    <p>
      This document describes the data included in the ‘Pupil absence in schools in England: 2018/19’ 
      National Statistics release’s underlying data files. This data is released under the terms of the 
      Open Government License and is intended to meet at least 3 stars for Open Data
    </p>
  
    <p>
      The Guide to absence statistics should be referenced alongside this
      data. It provides information on the data sources, their coverage and
      quality as well as explaining methodology used in producing the data.
    </p>`;

  const coveragePlaceholder = `
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
  `;
  const fileFormatPlaceholder = `
    <h3 className="govuk-heading-s">Rounding</h3>
    <p>This dataset has not had suppression applied.</p>
    <h3 className="govuk-heading-s">Conventions</h3>
    <p>The following convention is used throughout the underlying data</p>
  `;

  const test = editing;

  return (
    <>
      <form id="createMetaForm" className="govuk-!-marin-bottom-9">
        <legend className="govuk-fieldset__legend govuk-fieldset__legend--l">
          <h2 className="govuk-fieldset__heading">
            Example publication, 2018/19
          </h2>
        </legend>
        <div className="govuk-!-margin-bottom-7">
          <FormEditor
            id="description"
            name="description"
            label="Description"
            value={test.editing ? descriptionPlaceholder : ''}
            onChange={() => setAddNewMeta(true)}
          />
        </div>
        <div className="govuk-!-margin-bottom-7">
          <FormEditor
            id="coverage"
            name="coverage"
            label="Coverage"
            value={test.editing ? coveragePlaceholder : ''}
            onChange={() => setAddNewMeta(true)}
          />
        </div>
        <div className="govuk-!-margin-bottom-7">
          <FormEditor
            id="fileFormat"
            name="fileFormat"
            label="File Format"
            value={test.editing ? fileFormatPlaceholder : ''}
            onChange={() => setAddNewMeta(true)}
          />
        </div>
      </form>
      <h2 className="govuk-heading-m">Data files</h2>
      <Accordion id="dataFiles">
        <AccordionSection heading="Absence by geography" goToTop={false}>
          <dl className="govuk-summary-list">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Filename</dt>
              <dd className="govuk-summary-list__value">
                Absence_3term201819_nat_reg_la_sch
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Content</dt>
              <dd className="govuk-summary-list__value">
                Absence information for all enrolments in state-funded primary,
                secondary and special schools including information on overall
                absence, persistent absence and reason for absence for pupils
                aged 5-15, based on all 5 half terms data from 2006/07 to
                2011/12 inclusive and based on 6 half term data from 2012/13
                onwards
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Geographical levels</dt>
              <dd className="govuk-summary-list__value">
                National; Regional; Local authority; School
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Years</dt>
              <dd className="govuk-summary-list__value">2006/07 to 2018/19</dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Optional information</dt>
              <dd className="govuk-summary-list__value">
                <FormTextArea id="data1" name="data1" label="Details" />
              </dd>
            </div>
          </dl>
        </AccordionSection>
        <AccordionSection
          heading="Absence by Local Authority by characteristics"
          goToTop={false}
        >
          <dl className="govuk-summary-list">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Filename</dt>
              <dd className="govuk-summary-list__value">
                Absence_3term201819_la_characteristics
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Content</dt>
              <dd className="govuk-summary-list__value">
                Absence information by pupil characteristics such as age, gender
                and ethnicity by local authority.
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Geographical levels</dt>
              <dd className="govuk-summary-list__value">Local authority</dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Years</dt>
              <dd className="govuk-summary-list__value">2012/13 to 2018/19</dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Optional information</dt>
              <dd className="govuk-summary-list__value">
                <FormTextArea id="data2" name="data2" label="Details" />
              </dd>
            </div>
          </dl>
        </AccordionSection>
        <AccordionSection
          heading="Absence by Local Authority District by characteristics"
          goToTop={false}
        >
          <dl className="govuk-summary-list">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Filename</dt>
              <dd className="govuk-summary-list__value">
                Absence_3term201819_la_characteristics
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Content</dt>
              <dd className="govuk-summary-list__value">
                Absence information by pupil characteristics such as age, gender
                and ethnicity by local authority.
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Geographical levels</dt>
              <dd className="govuk-summary-list__value">Local authority</dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Years</dt>
              <dd className="govuk-summary-list__value">2012/13 to 2018/19</dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Optional information</dt>
              <dd className="govuk-summary-list__value">
                <FormTextArea id="data3" name="data3" label="Details" />
              </dd>
            </div>
          </dl>
        </AccordionSection>
        <MetaVariables />
      </Accordion>
    </>
  );
};

export default CreateMetaForms;
