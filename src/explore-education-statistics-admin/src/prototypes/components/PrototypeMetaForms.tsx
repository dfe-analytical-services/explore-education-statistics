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
  coverage?: string;
  fileFormat?: string;
}

const CreateMetaForms = ({ editing, description }: Props) => {
  const [addNewMeta, setAddNewMeta] = useState(false);

  const formExample = {
    descriptionPlaceholder: {
      text: `
      ${description}
      `,
    },
    coveragePlaceholder: {
      text: `
      <p>test2</p>
      `,
    },
    fileFormatPlaceholder: {
      text: `
      <p>test3</p>`,
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
            label="Description"
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
