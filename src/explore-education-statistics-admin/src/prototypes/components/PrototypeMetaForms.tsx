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
  description?: string;
  subject1?: string;
  subject2?: string;
  subject3?: string;
}

const CreateMetaForms = ({
  editing,
  description,
  subject1,
  subject2,
  subject3,
}: Props) => {
  const [addNewMeta, setAddNewMeta] = useState(false);
  const [valueSubject1, setValueSubject1] = useState('');
  const [valueSubject2, setValueSubject2] = useState('');
  const [valueSubject3, setValueSubject3] = useState('');

  const formExample = {
    descriptionPlaceholder: {
      text: `
      ${description}
      `,
    },
    subject1: {
      text: `
      ${subject1}
      `,
    },
    subject2: {
      text: `
      ${subject2}
      `,
    },
    subject3: {
      text: `
      ${subject3}
      `,
    },
  };

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
            label="Public metadata introduction"
            value={editing ? formExample.descriptionPlaceholder.text : ''}
            onChange={() => setAddNewMeta(true)}
          />
        </div>
      </form>
      <h2 className="govuk-heading-m">Data files</h2>
      <Accordion id="dataFiles">
        <AccordionSection heading="Absence by geography" goToTop={false}>
          <dl className="govuk-summary-list govuk-!-margin-bottom-9">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Filename</dt>
              <dd className="govuk-summary-list__value">
                Absence_3term201819_nat_reg_la_sch
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Content</dt>
              <dd className="govuk-summary-list__value">
                <FormTextArea
                  id="data1"
                  name="data1"
                  label="Details"
                  value={
                    editing ? formExample.subject1.text.trim() : valueSubject1
                  }
                  onChange={event => {
                    setValueSubject1(event.target.value);
                  }}
                />
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
              <dt className="govuk-summary-list__key">
                Variable names and descriptions
              </dt>
              <dd className="govuk-summary-list__value">
                <MetaVariables />
              </dd>
            </div>
          </dl>
        </AccordionSection>
        <AccordionSection
          heading="Absence by Local Authority by characteristics"
          goToTop={false}
        >
          <dl className="govuk-summary-list govuk-!-margin-bottom-9">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Filename</dt>
              <dd className="govuk-summary-list__value">
                Absence_3term201819_la_characteristics
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Content</dt>
              <dd className="govuk-summary-list__value">
                <FormTextArea
                  id="data2"
                  name="data2"
                  label="Details"
                  value={
                    editing ? formExample.subject2.text.trim() : valueSubject2
                  }
                  onChange={event => {
                    setValueSubject2(event.target.value);
                  }}
                />
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
              <dt className="govuk-summary-list__key">
                Variable names and descriptions
              </dt>
              <dd className="govuk-summary-list__value">
                <MetaVariables />
              </dd>
            </div>
          </dl>
        </AccordionSection>
        <AccordionSection
          heading="Absence by Local Authority District by characteristics"
          goToTop={false}
        >
          <dl className="govuk-summary-list govuk-!-margin-bottom-9">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Filename</dt>
              <dd className="govuk-summary-list__value">
                Absence_3term201819_lad_characteristics
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Content</dt>
              <dd className="govuk-summary-list__value">
                <FormTextArea
                  id="data3"
                  name="data3"
                  label="Details"
                  value={
                    editing ? formExample.subject3.text.trim() : valueSubject3
                  }
                  onChange={event => {
                    setValueSubject3(event.target.value);
                  }}
                />
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
              <dt className="govuk-summary-list__key">
                Variable names and descriptions
              </dt>
              <dd className="govuk-summary-list__value">
                <MetaVariables />
              </dd>
            </div>
          </dl>
        </AccordionSection>
      </Accordion>
    </>
  );
};

export default CreateMetaForms;
