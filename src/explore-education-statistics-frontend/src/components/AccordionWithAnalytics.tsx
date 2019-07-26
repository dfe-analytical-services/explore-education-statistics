import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import Accordion, { AccordionProps } from '@common/components/Accordion';

const AccordionWithAnalytics = (props: AccordionProps) => {
  return (
    <Accordion
      {...props}
      analytics={(accordionHeading: { id: string; title: string }) => {
        logEvent(
          'Accordion',
          `${accordionHeading.title} accordion opened`,
          `URL: ${window.location.pathname} | Accordion ID: ${accordionHeading.id}`,
        );
      }}
    />
  );
};

export default AccordionWithAnalytics;
