import { AccordionProps } from '@common/components/Accordion';
import { useState } from 'react';

interface Section {
  id: string;
  open: boolean;
}

export default function useMappingAccordionControls(sections: Section[]) {
  const [overrides, setOverrides] = useState<Record<string, boolean>>({});
  const [defaultOpen, setDefaultOpen] = useState<boolean>();
  const isOpen = (section: Section) =>
    overrides[section.id] ?? defaultOpen ?? section.open;
  const allExpanded = sections.length > 0 && sections.every(isOpen);

  return {
    hasSections: sections.length > 0,
    allExpanded,
    toggleAll: () => {
      setDefaultOpen(!allExpanded);
      setOverrides({});
    },
    getAccordionProps: (
      ids: string[],
    ): Pick<AccordionProps, 'openSectionIds' | 'onOpenSectionIdsChange'> => ({
      openSectionIds: sections
        .filter(section => ids.includes(section.id) && isOpen(section))
        .map(section => section.id),
      onOpenSectionIdsChange: openIds => {
        setOverrides(current => {
          const next = { ...current };
          ids.forEach(id => {
            next[id] = openIds.includes(id);
          });
          return next;
        });
      },
    }),
  };
}
