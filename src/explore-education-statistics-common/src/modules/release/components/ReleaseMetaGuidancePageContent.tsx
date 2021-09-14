import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import FormattedDate from '@common/components/FormattedDate';
import ContentHtml from '@common/components/ContentHtml';
import ReleaseMetaGuidanceDataFile from '@common/modules/release/components/ReleaseMetaGuidanceDataFile';
import { SubjectMetaGuidance } from '@common/services/releaseMetaGuidanceService';
import React, { ReactNode } from 'react';

interface Props {
  published?: string;
  metaGuidance: string;
  renderDataCatalogueLink?: ReactNode;
  subjects: SubjectMetaGuidance[];
}

const ReleaseMetaGuidancePageContent = ({
  published,
  metaGuidance,
  renderDataCatalogueLink,
  subjects,
}: Props) => {
  return (
    <>
      {published && (
        <p className="govuk-!-margin-bottom-8" data-testid="published-date">
          <strong>
            Published <FormattedDate>{published}</FormattedDate>
          </strong>
        </p>
      )}

      {metaGuidance && (
        <ContentHtml html={metaGuidance} testId="metaGuidance-content" />
      )}

      {subjects.length > 0 && (
        <>
          <h3 className="govuk-!-margin-top-6">Data files</h3>

          {renderDataCatalogueLink && (
            <p>
              All data files associated with this releases are listed below with
              guidance on their content. To download any of these files, please
              visit our {renderDataCatalogueLink}.
            </p>
          )}

          <Accordion id="dataFiles">
            {subjects.map(subject => (
              <AccordionSection heading={subject.name} key={subject.id}>
                <ReleaseMetaGuidanceDataFile
                  key={subject.id}
                  subject={subject}
                />
              </AccordionSection>
            ))}
          </Accordion>
        </>
      )}
    </>
  );
};

export default ReleaseMetaGuidancePageContent;
