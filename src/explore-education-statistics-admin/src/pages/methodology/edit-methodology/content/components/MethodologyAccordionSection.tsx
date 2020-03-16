import { ContentSection } from '@common/services/publicationService';
import React from 'react';
import EditableAccordionSection from '@admin/components/EditableAccordionSection';
import { EditableContentBlock } from '@admin/services/publicationService';
import MethodologyContentSection from './MethodologyContentSection';

interface MethodologyAccordionSectionProps {
  isAnnex?: boolean;
  content: ContentSection<EditableContentBlock>;
}

const MethodologyAccordionSection = ({
  isAnnex = false,
  content,
}: MethodologyAccordionSectionProps) => {
  return (
    <EditableAccordionSection {...content}>
      <MethodologyContentSection
        content={content.content}
        isAnnex={isAnnex}
        sectionId={content.id}
        allowComments={false}
        onBlockContentChange={() => {}}
        onBlockDelete={() => {}}
        id={content.id}
      />
    </EditableAccordionSection>
  );
};

export default MethodologyAccordionSection;
