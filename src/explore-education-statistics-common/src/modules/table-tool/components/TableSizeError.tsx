import Button from '@common/components/Button';
import ErrorPrefixPageTitle from '@common/components//ErrorPrefixPageTitle';
import downloadService from '@common/services/downloadService';
import { Subject } from '@common/services/tableBuilderService';
import React, { useEffect, useRef } from 'react';

interface Props {
  focus: boolean;
  releaseId?: string;
  showDownloadOption?: boolean;
  subject: Subject;
}

const TableSizeError = ({
  focus = false,
  releaseId,
  showDownloadOption = true,
  subject,
}: Props) => {
  const handleDownloadFile = async (fileId: string) => {
    if (releaseId) {
      await downloadService.downloadFiles(releaseId, [fileId]);
    }
  };

  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (focus && ref.current) {
      ref.current.focus();
    }
  }, [focus]);

  return (
    <div
      aria-labelledby="tableSizeError"
      className="govuk-error-summary"
      id="filtersForm-tableSizeError"
      ref={ref}
      role="alert"
      tabIndex={-1}
    >
      <h2 className="govuk-error-summary__title" id="tableSizeError">
        There is a problem
      </h2>

      <ErrorPrefixPageTitle />
      <div className="govuk-error-summary__body">
        <p>
          A table cannot be returned as the filters chosen can exceed the
          maximum allowable table size.
        </p>
        {showDownloadOption ? (
          <>
            <p>Select different filters or download the subject data.</p>
            {releaseId ? (
              <Button
                className="govuk-!-margin-bottom-0"
                onClick={() => handleDownloadFile(subject.file.id)}
              >
                {`Download ${subject.name} (${subject.file.extension}, ${subject.file.size})`}
              </Button>
            ) : (
              <Button className="govuk-!-margin-bottom-0" disabled>
                Download subject file (available when the release is published)
              </Button>
            )}
          </>
        ) : (
          <p>Select different filters and try again.</p>
        )}
      </div>
    </div>
  );
};

export default TableSizeError;
