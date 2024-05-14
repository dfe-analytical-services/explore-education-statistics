import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import Tag from '@common/components/Tag';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import { ReleaseSummary } from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import React, { ReactNode, useMemo } from 'react';

export interface ReleaseFormValues {
  releaseId: string;
}

export type ReleaseFormSubmitHandler = (values: {
  release: ReleaseSummary;
}) => void | Promise<void>;

const formId = 'releaseForm';

interface Props {
  legend?: ReactNode;
  legendSize?: FormFieldsetProps['legendSize'];
  legendHint?: string;
  initialValues?: { releaseId: string };
  onSubmit: ReleaseFormSubmitHandler;
  options: ReleaseSummary[];
  hideLatestDataTag?: boolean;
}

export default function ReleaseForm({
  legend,
  initialValues = {
    releaseId: '',
  },
  onSubmit,
  options,
  hideLatestDataTag,
  ...stepProps
}: Props & InjectedWizardProps) {
  const { isActive, currentStep, stepNumber, goToNextStep } = stepProps;

  const radioOptions = useMemo<RadioOption[]>(
    () =>
      options.map(option => {
        return {
          label: option.title,
          hint:
            option.latestRelease && !hideLatestDataTag ? (
              <Tag>This is the latest data</Tag>
            ) : undefined,
          inlineHint: true,
          value: option.id,
        };
      }),
    [options, hideLatestDataTag],
  );

  const handleSubmit = async ({ releaseId }: ReleaseFormValues) => {
    const release = options.find(r => r.id === releaseId);

    if (!release) {
      throw new Error('Selected release not found');
    }

    await goToNextStep(async () => {
      await onSubmit({ release });
    });
  };

  return (
    <FormProvider
      enableReinitialize
      initialValues={{
        releaseId:
          radioOptions.length === 1
            ? radioOptions[0].value
            : initialValues.releaseId,
      }}
      validationSchema={Yup.object<ReleaseFormValues>({
        releaseId: Yup.string().required('Choose a release'),
      })}
    >
      {({ formState, reset }) => {
        return isActive ? (
          <Form id={formId} onSubmit={handleSubmit}>
            <FormFieldRadioSearchGroup<ReleaseFormValues>
              name="releaseId"
              legend={legend}
              disabled={formState.isSubmitting}
              options={radioOptions}
              order={[]}
            />

            {options.length > 0 ? (
              <WizardStepFormActions
                {...stepProps}
                isSubmitting={formState.isSubmitting}
              />
            ) : (
              <p>No releases available.</p>
            )}
          </Form>
        ) : (
          <ResetFormOnPreviousStep
            currentStep={currentStep}
            stepNumber={stepNumber}
            onReset={reset}
          />
        );
      }}
    </FormProvider>
  );
}
