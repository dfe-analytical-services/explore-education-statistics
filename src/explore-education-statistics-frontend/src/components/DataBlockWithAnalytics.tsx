import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import DataBlock, {
  DataBlockProps,
} from '@common/modules/find-statistics/components/DataBlock';

const DataBlockWithAnalytics = (props: DataBlockProps) => {
  return (
    <DataBlock
      {...props}
      onToggle={(tabTitle: string) => {
        logEvent(
          'Statistics tabs',
          `${tabTitle} tab opened`,
          window.location.pathname,
        );
      }}
    />
  );
};

DataBlockWithAnalytics.defaultProps = DataBlock.defaultProps;

export default DataBlockWithAnalytics;
