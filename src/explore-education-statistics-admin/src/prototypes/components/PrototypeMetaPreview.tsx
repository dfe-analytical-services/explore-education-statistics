import PageTitle from '@admin/components/PageTitle';
import React from 'react';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ContentHtml from '@common/components/ContentHtml';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WarningMessage from '@common/components/WarningMessage';
import MetaVariables from './PrototypeMetaVariables';

interface Props {
  description?: string;
  showDialog?: boolean;
}

const PrototypeMetaPreview = ({ description, showDialog }: Props) => {
  return (
    <>
      <div>
        {showDialog && (
          <WarningMessage>
            Some data files have been added or changed since this page was
            created. <br />
            Please check all the details of this page and edit where necessary.
          </WarningMessage>
        )}

        <PageTitle
          title="An example publiction"
          caption="Academic year 2018/19"
        />
        <h2 className="govuk-heading-m">Meta guidance document</h2>

        <h3 className="govuk-heading-s">Published 23 July 2020</h3>

        <div className="govuk-!-margin-top-9 govuk-!-margin-bottom-9">
          <ContentHtml html={description || ''} />
        </div>

        <h2 className="govuk-heading-m govuk-!-margin-top-9">Data files</h2>
        <Accordion id="dataFiles">
          <AccordionSection heading="Absence by geography" goToTop={false}>
            <SummaryList className="govuk-!-margin-bottom-9">
              <SummaryListItem term="Filename">
                Absence_3term201819_nat_reg_la_sch
              </SummaryListItem>
              <SummaryListItem term="Content">
                Absence information for all enrolments in state-funded primary,
                secondary and special schools including information on overall
                absence, persistent absence and reason for absence for pupils
                aged 5-15, based on all 5 half terms data from 2006/07 to
                2011/12 inclusive and based on 6 half term data from 2012/13
                onwards
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
                Absence information by pupil characteristics such as age, gender
                and ethnicity by local authority district.
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
                Absence information by pupil characteristics such as age, gender
                and ethnicity by local authority district.
              </SummaryListItem>
              <SummaryListItem term="Years">2012/13 to 2018/19</SummaryListItem>
              <SummaryListItem term="Variable names and descriptions">
                <MetaVariables />
              </SummaryListItem>
            </SummaryList>
          </AccordionSection>
        </Accordion>
      </div>
    </>
  );
};

export default PrototypeMetaPreview;
