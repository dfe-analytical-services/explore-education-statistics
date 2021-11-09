import Button from '@common/components/Button';
import { BaseErrorSummary } from '@common/components/ErrorSummary';
import downloadService from '@common/services/downloadService';
import { Subject } from '@common/services/tableBuilderService';
import React, { useEffect, useRef } from 'react';

interface Props {
  id: string;
  releaseId?: string;
  showDownloadOption?: boolean;
  subject: Subject;
}

const TableSizeError = ({
  id,
  releaseId,
  showDownloadOption = true,
  subject,
}: Props) => {
  const ref = useRef<HTMLDivElement>(null);
  useEffect(() => {
    if (ref.current) {
      ref.current.focus();
    }
  }, []);

  return (
    <BaseErrorSummary id={id} ref={ref} title="There is a problem">
      <p>
        Could not create table as the filters chosen may exceed the maximum
        allowed table size.
      </p>
      {showDownloadOption ? (
        <>
          <p>Select different filters or download the subject data.</p>
          <Button
            className="govuk-!-margin-bottom-0"
            disabled={!releaseId}
            onClick={async () => {
              if (releaseId) {
                await downloadService.downloadFiles(releaseId, [
                  subject.file.id,
                ]);
              }
            }}
          >
            {`Download ${subject.name} (${subject.file.extension}, ${subject.file.size})`}
            {!releaseId && ` (available when the release is published)`}
          </Button>
        </>
      ) : (
        <p>Select different filters and try again.</p>
      )}
    </BaseErrorSummary>
  );
};

export default TableSizeError;
