import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import PageSearchForm, {
  PageSearchFormProps,
} from '@common/components/PageSearchForm';

const PageSearchFormWithAnalytics = (props: PageSearchFormProps) => {
  const { id, onSearch } = props;
  return (
    <PageSearchForm
      {...props}
      onSearch={(searchTerm: string) => {
        logEvent({
          category: window.location.pathname,
          action: id || 'PageSearchForm',
          label: searchTerm,
        });

        if (typeof onSearch === 'function') {
          onSearch(searchTerm);
        }
      }}
    />
  );
};

export default PageSearchFormWithAnalytics;
