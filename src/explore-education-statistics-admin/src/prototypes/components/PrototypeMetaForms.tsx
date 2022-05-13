import React, { useState } from 'react';
import Accordion from '@common/components/Accordion';
import Button from '@common/components/Button';
import AccordionSection from '@common/components/AccordionSection';
import FormEditor from '@admin/components/form/FormEditor';
import FormTextArea from '@common/components/form/FormTextArea';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import MetaVariables from './PrototypeMetaVariables';

interface Props {
  editing?: boolean;
  description?: string;
  subject1?: string;
  subject2?: string;
  subject3?: string;
}

const PrototypeCreateMetaForms = ({
  editing,
  description,
  subject1,
  subject2,
  subject3,
}: Props) => {
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
      <form id="createMetaForm" className="govuk-!-margin-bottom-9">
        <legend className="govuk-fieldset__legend govuk-fieldset__legend--l">
          <h2 className="govuk-fieldset__heading">
            <span className="govuk-caption-l">Academic year 2018/19</span>
            An example publication
          </h2>
        </legend>
        <div className="govuk-!-margin-bottom-7">
          <FormEditor
            id="description"
            label="Public metadata introduction"
            value={formExample.descriptionPlaceholder.text}
            onChange={() => {}}
          />
        </div>
        <Button className=" govuk-!-margin-right-3" onClick={() => {}}>
          Save
        </Button>
        <Button variant="secondary" onClick={() => {}}>
          Cancel
        </Button>
      </form>
      <h2 className="govuk-heading-m">Data files</h2>
      <Accordion id="dataFiles">
        <AccordionSection heading="Absence by geography" goToTop={false}>
          <SummaryList className="govuk-!-margin-bottom-9">
            <SummaryListItem term="Filename">
              Absence_3term201819_nat_reg_la_sch
            </SummaryListItem>
            <SummaryListItem term="Content">
              <form id="createMetaFormData1">
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
                <Button className=" govuk-!-margin-right-3" onClick={() => {}}>
                  Save
                </Button>
                <Button variant="secondary" onClick={() => {}}>
                  Cancel
                </Button>
              </form>
            </SummaryListItem>
            <SummaryListItem term="Geographical levels">
              National; Regional; Local authority; School
            </SummaryListItem>
            <SummaryListItem term="Years">2006/07 to 2018/19</SummaryListItem>
            <SummaryListItem term="Variable names and descriptions">
              <MetaVariables />
            </SummaryListItem>
          </SummaryList>
        </AccordionSection>
        <AccordionSection
          heading="Absence by Local Authority by characteristics"
          goToTop={false}
        >
          <SummaryList className="govuk-!-margin-bottom-9">
            <SummaryListItem term="Geographical levels">
              National; Regional; Local authority; School
            </SummaryListItem>
            <SummaryListItem term="Content">
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
              <Button className=" govuk-!-margin-right-3" onClick={() => {}}>
                Save
              </Button>
              <Button variant="secondary" onClick={() => {}}>
                Cancel
              </Button>
            </SummaryListItem>
            <SummaryListItem term="Years">2006/07 to 2018/19</SummaryListItem>
            <SummaryListItem term="Variable names and descriptions">
              <MetaVariables />
            </SummaryListItem>
          </SummaryList>
        </AccordionSection>
        <AccordionSection
          heading="Absence by Local Authority District by characteristics"
          goToTop={false}
        >
          <SummaryList className="govuk-!-margin-bottom-9">
            <SummaryListItem term="Geographical levels">
              Absence_3term201819_lad_characteristics
            </SummaryListItem>
            <SummaryListItem term="Content">
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
              <Button className=" govuk-!-margin-right-3" onClick={() => {}}>
                Save
              </Button>
              <Button variant="secondary" onClick={() => {}}>
                Cancel
              </Button>
            </SummaryListItem>
            <SummaryListItem term="Years">2012/13 to 2018/19</SummaryListItem>
            <SummaryListItem term="Variable names and descriptions">
              <MetaVariables />
            </SummaryListItem>
          </SummaryList>
        </AccordionSection>
      </Accordion>
    </>
  );
};

export default PrototypeCreateMetaForms;
