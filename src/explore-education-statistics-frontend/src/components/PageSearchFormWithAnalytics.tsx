import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import PageSearchForm, {
  PageSearchFormProps,
} from '@common/components/PageSearchForm';

const PageSearchFormWithAnalytics = (props: PageSearchFormProps) => {
  return (
    <PageSearchForm
      {...props}
      onSearch={(searchTerm: string) => {
        logEvent(
          window.location.pathname,
          props.id || 'PageSearchForm',
          searchTerm,
        );
        if (typeof props.onSearch === 'function') {
          props.onSearch(searchTerm);
        }
      }}
    />
  );
};

PageSearchFormWithAnalytics.defaultProps = PageSearchForm.defaultProps;

export default PageSearchFormWithAnalytics;
