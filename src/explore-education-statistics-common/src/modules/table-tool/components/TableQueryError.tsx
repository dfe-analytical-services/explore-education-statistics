import Button from '@common/components/Button';
import { BaseErrorSummary } from '@common/components/ErrorSummary';
import downloadService from '@common/services/downloadService';
import { Subject } from '@common/services/tableBuilderService';
import React, { useEffect, useRef } from 'react';
import { TableQueryErrorCode } from '@common/modules/table-tool/components/FiltersForm';

interface Props {
  id: string;
  releaseId?: string;
  showDownloadOption?: boolean;
  subject: Subject;
  errorCode: TableQueryErrorCode;
}

const TableQueryError = ({
  id,
  releaseId,
  showDownloadOption = true,
  subject,
  errorCode,
}: Props) => {
  const ref = useRef<HTMLDivElement>(null);
  useEffect(() => {
    if (ref.current) {
      ref.current.focus();
    }
  }, []);

  return (
    <BaseErrorSummary id={id} ref={ref} title="There is a problem">
      {errorCode === 'QUERY_EXCEEDS_MAX_ALLOWABLE_TABLE_SIZE' && (
        <p>
          Could not create table as the filters chosen may exceed the maximum
          allowed table size.
        </p>
      )}
      {errorCode === 'REQUEST_CANCELLED' && (
        <p>
          Could not create table as the filters chosen took too long to respond.
        </p>
      )}
      {showDownloadOption ? (
        <>
          {errorCode === 'QUERY_EXCEEDS_MAX_ALLOWABLE_TABLE_SIZE' && (
            <p>Select different filters or download the subject data.</p>
          )}
          {errorCode === 'REQUEST_CANCELLED' && (
            <p>
              Select different filters, try again later or download the subject
              data.
            </p>
          )}
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
        <>
          {errorCode === 'QUERY_EXCEEDS_MAX_ALLOWABLE_TABLE_SIZE' && (
            <p>Select different filters and try again.</p>
          )}
          {errorCode === 'REQUEST_CANCELLED' && (
            <p>Select different filters or try again later.</p>
          )}
        </>
      )}
    </BaseErrorSummary>
  );
};

export default TableQueryError;
