import { EinContentBlock } from '@common/services/types/einBlocks';
import SectionBreak from '@common/components/SectionBreak';
import React from 'react';
import EducationInNumbersSectionBlocks from './EducationInNumbersSectionBlocks';

interface EducationInNumbersSectionProps {
  content: EinContentBlock[];
  heading?: string;
  isFirstSection?: boolean;
}

const EducationInNumbersContentSection = ({
  content,
  heading,
  isFirstSection = false,
}: EducationInNumbersSectionProps) => {
  return (
    <div data-testid="ein-content-section">
      {!isFirstSection && <SectionBreak size="l" />}
      {heading && (
        <h2
          className={`govuk-heading-l govuk-!-margin-bottom-4 ${
            !isFirstSection && 'govuk-!-margin-top-8'
          }`}
        >
          {heading}
        </h2>
      )}
      <EducationInNumbersSectionBlocks blocks={content} />
    </div>
  );
};

export default EducationInNumbersContentSection;
