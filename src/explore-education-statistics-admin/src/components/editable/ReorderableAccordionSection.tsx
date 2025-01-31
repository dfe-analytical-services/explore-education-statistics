import AccordionSection, {
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import React from 'react';

export interface ReorderableAccordionSectionProps
  extends AccordionSectionProps {
  id: string;
  isReordering?: boolean;
}

export default function ReorderableAccordionSection({
  heading,
  isReordering,
  ...restProps
}: ReorderableAccordionSectionProps) {
  if (isReordering) {
    return heading;
  }

  return <AccordionSection {...restProps} heading={heading} />;
}
