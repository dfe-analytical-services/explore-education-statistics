import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import FormattedDate from '@common/components/FormattedDate';
import SanitizeHtml from '@common/components/SanitizeHtml';
import ReleaseMetaGuidanceDataFile from '@common/modules/release/components/ReleaseMetaGuidanceDataFile';
import { SubjectMetaGuidance } from '@common/services/releaseMetaGuidanceService';
import React from 'react';

interface Props {
  published?: string;
  metaGuidance: string;
  subjects: SubjectMetaGuidance[];
}

const ReleaseMetaGuidancePageContent = ({
  published,
  metaGuidance,
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

      <SanitizeHtml dirtyHtml={metaGuidance} testId="metaGuidance-content" />

      {subjects.length > 0 && (
        <>
          <h3 className="govuk-!-margin-top-6">Data files</h3>

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
