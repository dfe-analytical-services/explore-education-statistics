import {
  ChartBuilderFormKeys,
  useChartBuilderFormsContext,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ErrorSummary from '@common/components/ErrorSummary';
import React, { MouseEventHandler, ReactNode } from 'react';

interface Props {
  children?: ReactNode;
  disabled?: boolean;
  formId: string;
  formKey: ChartBuilderFormKeys;
  onClick?: MouseEventHandler<HTMLButtonElement>;
}

const ChartBuilderSaveActions = ({
  children,
  disabled,
  formId,
  formKey,
  onClick,
}: Props) => {
  const { forms, isSubmitting } = useChartBuilderFormsContext();

  const currentForm = forms[formKey];

  if (!currentForm) {
    return null;
  }

  const showSubmitError = currentForm.isValid && currentForm.submitCount > 0;

  return (
    <>
      <ErrorSummary
        title="Cannot save chart"
        id={`${formId}-errorSummary`}
        errors={
          showSubmitError
            ? Object.values(forms)
                .filter(form => !form.isValid)
                .map(form => ({
                  id: form.id,
                  message: `${form.title} tab is invalid`,
                }))
            : []
        }
        onErrorClick={event => {
          event.preventDefault();

          const tab = document.querySelector<HTMLAnchorElement>(
            `${event.currentTarget.getAttribute('href')}-tab`,
          );

          if (tab) {
            tab.click();

            const tabs =
              document.querySelector<HTMLDivElement>('#chartBuilder-tabs');

            if (tabs) {
              tabs.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
              });
            }
          }
        }}
      />

      <ButtonGroup>
        <Button
          type="submit"
          id={`${formId}-submit`}
          disabled={disabled || isSubmitting}
          onClick={onClick}
        >
          Save chart options
        </Button>

        {children}
      </ButtonGroup>
    </>
  );
};

export default ChartBuilderSaveActions;
