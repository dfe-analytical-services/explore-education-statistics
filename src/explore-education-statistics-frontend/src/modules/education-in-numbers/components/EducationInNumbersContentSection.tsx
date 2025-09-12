import { ContentBlock } from '@common/services/types/blocks';
import SectionBreak from '@common/components/SectionBreak';
import React from 'react';
import EducationInNumbersSectionBlocks from './EducationInNumbersSectionBlocks';

interface EducationInNumbersSectionProps {
  content: ContentBlock[];
  heading?: string;
  isLastSection?: boolean;
}

const EducationInNumbersContentSection = ({
  content,
  heading,
  isLastSection = false,
}: EducationInNumbersSectionProps) => {
  return (
    <div data-testid="ein-content-section">
      {heading && (
        <h2 className="govuk-heading-l govuk-!-margin-bottom-2">{heading}</h2>
      )}
      <EducationInNumbersSectionBlocks blocks={content} />
      {!isLastSection && <SectionBreak size="l" />}
    </div>
  );
};

export default EducationInNumbersContentSection;
