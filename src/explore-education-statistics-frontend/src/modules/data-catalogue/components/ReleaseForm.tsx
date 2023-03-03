import { Form } from '@common/components/form';
import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import Tag from '@common/components/Tag';
import useFormSubmit from '@common/hooks/useFormSubmit';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import { ReleaseSummary } from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { ReactNode, useMemo } from 'react';

export interface ReleaseFormValues {
  releaseId: string;
}

export type ReleaseFormSubmitHandler = (values: {
  release: ReleaseSummary;
}) => void;

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

const ReleaseForm = ({
  legend,
  initialValues = {
    releaseId: '',
  },
  onSubmit,
  options,
  hideLatestDataTag,
  ...stepProps
}: Props & InjectedWizardProps) => {
  const { isActive, currentStep, stepNumber, goToNextStep } = stepProps;

  const radioOptions = useMemo<RadioOption[]>(
    () =>
      options.map(option => {
        return {
          label: option.title,
          hint:
            option.latestRelease && !hideLatestDataTag ? (
              <Tag strong>This is the latest data</Tag>
            ) : undefined,
          inlineHint: true,
          value: option.id,
        };
      }),
    [options, hideLatestDataTag],
  );

  const handleSubmit = useFormSubmit(
    async ({ releaseId }: ReleaseFormValues) => {
      const release = options.find(r => r.id === releaseId);

      if (!release) {
        throw new Error('Selected release not found');
      }

      await goToNextStep(async () => {
        await onSubmit({ release });
      });
    },
  );

  return (
    <Formik<ReleaseFormValues>
      enableReinitialize
      initialValues={{
        releaseId:
          radioOptions.length === 1
            ? radioOptions[0].value
            : initialValues.releaseId,
      }}
      validateOnBlur={false}
      validationSchema={Yup.object<ReleaseFormValues>({
        releaseId: Yup.string().required('Choose a release'),
      })}
      onSubmit={handleSubmit}
    >
      {form => {
        return isActive ? (
          <Form id={formId} showSubmitError>
            <FormFieldRadioSearchGroup<ReleaseFormValues>
              name="releaseId"
              legend={legend}
              disabled={form.isSubmitting}
              options={radioOptions}
              order={[]}
            />

            {options.length > 0 ? (
              <WizardStepFormActions
                {...stepProps}
                isSubmitting={form.isSubmitting}
              />
            ) : (
              <p>No releases available.</p>
            )}
          </Form>
        ) : (
          <ResetFormOnPreviousStep
            currentStep={currentStep}
            stepNumber={stepNumber}
            onReset={form.resetForm}
          />
        );
      }}
    </Formik>
  );
};

export default ReleaseForm;
