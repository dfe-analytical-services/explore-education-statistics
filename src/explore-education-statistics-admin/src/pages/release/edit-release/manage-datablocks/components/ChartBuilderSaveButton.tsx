import Button from '@common/components/Button';
import ErrorSummary from '@common/components/ErrorSummary';
import React, { MouseEventHandler } from 'react';

interface Props {
  formId: string;
  showSubmitError: boolean;
  onClick?: MouseEventHandler<HTMLButtonElement>;
}

const ChartBuilderSaveButton = ({
  formId,
  showSubmitError,
  onClick,
}: Props) => {
  return (
    <>
      {showSubmitError && (
        <ErrorSummary
          title="Cannot save chart"
          id={`${formId}-errorSummary`}
          errors={[
            {
              id: `${formId}-submit`,
              message: 'Ensure that all other tabs are valid first',
            },
          ]}
        />
      )}

      <Button type="submit" id={`${formId}-submit`} onClick={onClick}>
        Save chart options
      </Button>
    </>
  );
};

export default ChartBuilderSaveButton;
