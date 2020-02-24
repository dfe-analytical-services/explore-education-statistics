import { EditableAccordionProps } from '@admin/components/EditableAccordion';
import methodologyService from '@admin/services/methodology/service';
import { MethodologyContent } from '@admin/services/methodology/types';
import { EditableAccordionSectionProps } from 'src/components/EditableAccordionSection';

function accordionAddNewSection(
  methodology: MethodologyContent,
  setMethodology: React.Dispatch<
    React.SetStateAction<MethodologyContent | undefined>
  >,
): EditableAccordionProps['onAddSection'] {
  return async function onAddSection() {
    const newSection = await methodologyService.addContentSection(
      methodology.id,
      {
        order: methodology.content.length,
      },
    );
    setMethodology({
      ...methodology,
      content: [...methodology.content, newSection],
    });
  };
}

function accordionUpdateSectionsOrder(
  methodology: MethodologyContent,
  setMethodology: React.Dispatch<
    React.SetStateAction<MethodologyContent | undefined>
  >,
): EditableAccordionProps['onSaveOrder'] {
  return async function onSaveOrder(order) {
    const sectionsOrdered = await methodologyService.updateContentSectionsOrder(
      methodology.id,
      order,
    );
    setMethodology({
      ...methodology,
      content: sectionsOrdered,
    });
  };
}

function accordionSectionUpdateHeading(
  methodology: MethodologyContent,
  setMethodology: React.Dispatch<
    React.SetStateAction<MethodologyContent | undefined>
  >,
  sectionId: string,
): EditableAccordionSectionProps['onHeadingChange'] {
  return async function(heading) {
    const newSection = await methodologyService.updateContentSectionHeading(
      methodology.id,
      sectionId,
      heading,
    );

    setMethodology({
      ...methodology,
      content: methodology.content.map(section => {
        if (section.id === sectionId) {
          return newSection;
        }
        return section;
      }),
    });
  };
}

function accordionSectionRemoveSection(
  methodology: MethodologyContent,
  setMethodology: React.Dispatch<
    React.SetStateAction<MethodologyContent | undefined>
  >,
  sectionId: string,
): EditableAccordionSectionProps['onRemoveSection'] {
  return async function onRemoveSection() {
    await methodologyService.removeContentSection(methodology.id, sectionId);
    setMethodology({
      ...methodology,
      content: methodology.content.filter(section => section.id !== sectionId),
    });
  };
}

export default {
  accordionAddNewSection,
  accordionUpdateSectionsOrder,
  accordionSectionUpdateHeading,
  accordionSectionRemoveSection,
};
