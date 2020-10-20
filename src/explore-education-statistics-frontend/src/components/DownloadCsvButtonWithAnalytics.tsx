import React from 'react';
import DownloadCsvButton, {
  DownloadCsvButtonProps,
} from '@common/modules/table-tool/components/DownloadCsvButton';
import { logEvent } from '@frontend/services/googleAnalyticsService';

const DownloadCsvButtonWithAnalytics = (props: DownloadCsvButtonProps) => {
  return (
    <DownloadCsvButton
      {...props}
      onClick={() =>
        logEvent(
          'CSV download',
          'CSV download button clicked',
          `${props.fullTable.subjectMeta.publicationName} between ${
            props.fullTable.subjectMeta.timePeriodRange[0].label
          } and ${
            props.fullTable.subjectMeta.timePeriodRange[
              props.fullTable.subjectMeta.timePeriodRange.length - 1
            ].label
          }`,
        )
      }
    />
  );
};

export default DownloadCsvButtonWithAnalytics;
