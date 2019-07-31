import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import Accordion, { AccordionProps } from '@common/components/Accordion';

const AccordionWithAnalytics = (props: AccordionProps) => {
  return (
    <Accordion
      {...props}
      onToggle={accordionSection => {
        logEvent(
          'Accordion',
          `${accordionSection.title} accordion opened`,
          `URL: ${window.location.pathname} | Accordion ID: ${accordionSection.id}`,
        );
      }}
    />
  );
};

export default AccordionWithAnalytics;
