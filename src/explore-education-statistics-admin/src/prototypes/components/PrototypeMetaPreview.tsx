import React from 'react';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import MetaVariables from './PrototypeMetaVariables';

interface Props {
  description?: string;
  showDialog?: boolean;
}

const CreateMetaForms = ({ description, showDialog }: Props) => {
  return (
    <>
      {showDialog && (
        <div className="govuk-warning-text govuk-!-margin-bottom-9">
          <span className="govuk-warning-text__icon" aria-hidden="true">
            !
          </span>
          <strong className="govuk-warning-text__text">
            <span className="govuk-warning-text__assistive">Warning</span>
            Some data files have been added or changed since this page was
            created. <br />
            Please check all the details of this page and edit where necessary.
          </strong>
        </div>
      )}

      <h2 className="govuk-heading-l">An example publication, 2018/19</h2>
      <h3 className="govuk-heading-m">Meta guidance document</h3>

      <p className="govuk-!-margin-top-9 govuk-!-margin-bottom-9">July 2020</p>

      <div
        dangerouslySetInnerHTML={{
          __html: `${description}`,
        }}
      />

      <h2 className="govuk-heading-m govuk-!-margin-top-9">Data files</h2>
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
          <dl className="govuk-summary-list  govuk-!-margin-bottom-9">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Filename</dt>
              <dd className="govuk-summary-list__value">
                Absence_3term201819_lad_characteristics
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Content</dt>
              <dd className="govuk-summary-list__value">
                Absence information by pupil characteristics such as age, gender
                and ethnicity by local authority district.
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
