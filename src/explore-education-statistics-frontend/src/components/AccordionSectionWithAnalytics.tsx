import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import AccordionSection, {
  AccordionSectionProps,
} from '@common/components/AccordionSection';

const AccordionSectionWithAnalytics = (props: AccordionSectionProps) => {
  return (
    <AccordionSection
      {...props}
      onToggle={(open: boolean) =>
        open &&
        logEvent(
          'Contact Us',
          'Contact us accordion opened',
          window.location.pathname,
        )
      }
    />
  );
};

export default AccordionSectionWithAnalytics;
