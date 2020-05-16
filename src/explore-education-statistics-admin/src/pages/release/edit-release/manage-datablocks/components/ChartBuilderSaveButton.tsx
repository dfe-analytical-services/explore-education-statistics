import { ChartBuilderForm } from '@admin/pages/release/edit-release/manage-datablocks/components/ChartBuilder';
import Button from '@common/components/Button';
import ErrorSummary from '@common/components/ErrorSummary';
import { Dictionary } from '@common/types';
import React, { MouseEventHandler } from 'react';

interface Props {
  formId: string;
  forms: Dictionary<ChartBuilderForm>;
  showSubmitError: boolean;
  onClick?: MouseEventHandler<HTMLButtonElement>;
}

const ChartBuilderSaveButton = ({
  formId,
  forms,
  showSubmitError,
  onClick,
}: Props) => {
  return (
    <>
      {showSubmitError && (
        <ErrorSummary
          title="Cannot save chart"
          id={`${formId}-errorSummary`}
          errors={Object.values(forms)
            .filter(form => !form.isValid)
            .map(form => ({
              id: `${formId}-submit`,
              message: `${form.title} tab is invalid`,
            }))}
        />
      )}

      <Button type="submit" id={`${formId}-submit`} onClick={onClick}>
        Save chart options
      </Button>
    </>
  );
};

export default ChartBuilderSaveButton;
