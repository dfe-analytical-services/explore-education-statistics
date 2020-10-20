import React from 'react';
import DownloadExcelButton, {
  DownloadExcelButtonProps,
} from '@common/modules/table-tool/components/DownloadExcelButton';
import { logEvent } from '@frontend/services/googleAnalyticsService';

const DownloadExcelButtonWithAnalytics = (props: DownloadExcelButtonProps) => {
  return (
    <DownloadExcelButton
      {...props}
      onClick={() => {
        logEvent(
          'Excel download',
          'Excel download button clicked',
          `${props.subjectMeta.publicationName} between ${
            props.subjectMeta.timePeriodRange[0].label
          } and ${
            props.subjectMeta.timePeriodRange[
              props.subjectMeta.timePeriodRange.length - 1
            ].label
          }`,
        );
      }}
    />
  );
};

export default DownloadExcelButtonWithAnalytics;
