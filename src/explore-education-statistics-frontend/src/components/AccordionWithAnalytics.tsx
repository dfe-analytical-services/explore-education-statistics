import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import Accordion, { AccordionProps } from '@common/components/Accordion';

interface Props extends AccordionProps {
  publicationTitle: string;
}

const AccordionWithAnalytics = (props: Props) => {
  return (
    <Accordion
      {...props}
      onToggle={accordionSection => {
        logEvent(
          'Accordion',
          `${accordionSection.title} accordion opened`,
          props.publicationTitle,
        );
      }}
    />
  );
};

export default AccordionWithAnalytics;
