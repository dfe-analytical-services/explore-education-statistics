import Button from '@common/components/Button';
import { BaseErrorSummary } from '@common/components/ErrorSummary';
import downloadService from '@common/services/downloadService';
import { Subject } from '@common/services/tableBuilderService';
import React, { useEffect, useMemo, useRef } from 'react';
import {
  FormValues,
  TableQueryErrorCode,
} from '@common/modules/table-tool/components/FiltersForm';
import { useWatch } from 'react-hook-form';
import isEqual from 'lodash/isEqual';

interface Props {
  id: string;
  releaseId?: string;
  showDownloadOption?: boolean;
  subject: Subject;
  errorCode: TableQueryErrorCode;
  previousValues?: FormValues;
}

interface ErrorMessageText {
  errorMessage: string;
  downloadOptionMessage: string;
  nonDownloadOptionMessage: string;
}

const TableQueryError = ({
  id,
  releaseId,
  showDownloadOption = true,
  subject,
  errorCode,
  previousValues,
}: Props) => {
  const ref = useRef<HTMLDivElement>(null);
  useEffect(() => {
    if (ref.current) {
      ref.current.focus();
    }
  }, []);

  const values = useWatch();
  const {
    errorMessage,
    downloadOptionMessage,
    nonDownloadOptionMessage,
  } = useMemo<ErrorMessageText>(() => {
    switch (errorCode) {
      case 'QueryExceedsMaxAllowableTableSize':
        return {
          errorMessage:
            'Could not create table as the filters chosen may exceed the maximum allowed table size.',
          downloadOptionMessage:
            'Select different filters or download the subject data.',
          nonDownloadOptionMessage: 'Select different filters and try again.',
        };
      case 'RequestCancelled':
        return {
          errorMessage:
            'Could not create table as the filters chosen took too long to respond.',
          downloadOptionMessage:
            'Select different filters, try again later or download the subject data.',
          nonDownloadOptionMessage:
            'Select different filters or try again later.',
        };
      default:
        return {
          errorMessage: 'Could not create table.',
          downloadOptionMessage:
            'Try again later or download the subject data.',
          nonDownloadOptionMessage: 'Try again later.',
        };
    }
  }, [errorCode]);

  if (!isEqual(values, previousValues)) {
    return null;
  }

  return (
    <BaseErrorSummary id={id} ref={ref} title="There is a problem">
      <p>{errorMessage}</p>
      {showDownloadOption ? (
        <>
          <p>{downloadOptionMessage}</p>
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
        <p>{nonDownloadOptionMessage}</p>
      )}
    </BaseErrorSummary>
  );
};

export default TableQueryError;
