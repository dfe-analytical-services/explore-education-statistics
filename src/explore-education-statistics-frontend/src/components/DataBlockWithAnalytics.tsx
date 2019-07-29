import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import DataBlock, {
  DataBlockProps,
} from '@common/modules/find-statistics/components/DataBlock';

const DataBlockWithAnalytics = (props: DataBlockProps) => {
  return (
    <DataBlock
      {...props}
      onSummaryDetailsToggle={(isOpened, event) => {
        logEvent(
          'Summary Define Dropdowns',
          event.currentTarget.title,
          window.location.pathname,
        );
      }}
      onToggle={(section: { id: string; title: string }) => {
        logEvent(
          'Publication Release Data Tabs',
          `${section.title} (${section.id}) tab opened`,
          window.location.pathname,
        );
      }}
    />
  );
};

DataBlockWithAnalytics.defaultProps = DataBlock.defaultProps;

export default DataBlockWithAnalytics;
