import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import PageSearchForm, {
  PageSearchFormProps,
} from '@common/components/PageSearchForm';

const PageSearchFormWithAnalytics = (props: PageSearchFormProps) => {
  const onSearch = (searchTerm: string) => {
    logEvent(
      window.location.pathname,
      props.id || 'PageSearchForm',
      searchTerm,
    );
    if (typeof props.onSearch === 'function') {
      props.onSearch(searchTerm);
    }
  };

  return (
    <PageSearchForm
      {...props}
      onSearch={(searchTerm: string) => onSearch(searchTerm)}
    />
  );
};

export default PageSearchFormWithAnalytics;
