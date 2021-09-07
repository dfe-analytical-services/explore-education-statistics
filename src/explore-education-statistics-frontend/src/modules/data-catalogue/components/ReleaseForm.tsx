import {
  Form,
  FormFieldRadioGroup,
  FormGroup,
  FormTextSearchInput,
  FormFieldset,
} from '@common/components/form';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import Yup from '@common/validation/yup';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import createErrorHelper from '@common/validation/createErrorHelper';
import { ReleaseSummary } from '@common/services/publicationService';
import Tag from '@common/components/Tag';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { format } from 'date-fns';
import { Formik } from 'formik';
import React, { ReactNode, useMemo, useState } from 'react';
import orderBy from 'lodash/orderBy';

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
}

const ReleaseForm = ({
  goToNextStep,
  legend,
  legendSize = 'l',
  legendHint,
  initialValues = {
    releaseId: '',
  },
  onSubmit,
  options,
  ...stepProps
}: Props & InjectedWizardProps) => {
  const [searchTerm, setSearchTerm] = useState('');
  const { isActive, currentStep, stepNumber } = stepProps;

  const radioOptions = useMemo<RadioOption[]>(
    () =>
      orderBy(options, 'published', 'desc')
        .filter(option => {
          if (!searchTerm) {
            return option;
          }
          if (option.title.toLowerCase().includes(searchTerm.toLowerCase())) {
            return option;
          }
          return null;
        })
        .map(option => {
          return {
            label: `${option.title} (${format(
              new Date(option.published ?? ''),
              'd MMMM yyyy',
            )})`,
            hint: option.latestRelease ? (
              <Tag strong>This is the latest data</Tag>
            ) : undefined,
            inlineHint: true,
            value: option.id,
          };
        }),
    [options, searchTerm],
  );

  const handleSubmit = useFormSubmit(
    async ({ releaseId }: ReleaseFormValues) => {
      const release = options.find(r => r.id === releaseId);

      if (!release) {
        throw new Error('Selected release not found');
      }

      await onSubmit({ release });
      goToNextStep();
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
        const { getError } = createErrorHelper(form);
        return isActive ? (
          <Form id={formId} showSubmitError>
            <FormFieldset
              error={getError('release')}
              id="release"
              legend={legend}
              legendSize={legendSize}
              hint={legendHint}
            >
              {options.length > 1 && (
                <FormGroup>
                  <FormTextSearchInput
                    id={`${formId}-releaseIdSearch`}
                    label="Search releases"
                    name="releaseSearch"
                    onChange={event => setSearchTerm(event.target.value)}
                    onKeyPress={event => {
                      if (event.key === 'Enter') {
                        event.preventDefault();
                      }
                    }}
                    width={20}
                  />
                </FormGroup>
              )}
              {options.length > 0 && (
                <FormFieldRadioGroup<ReleaseFormValues>
                  name="releaseId"
                  legendSize={legendSize}
                  legend="Choose a release from the list below"
                  legendHidden
                  disabled={form.isSubmitting}
                  options={radioOptions}
                  order={[]}
                />
              )}
            </FormFieldset>

            {options.length > 0 ? (
              <WizardStepFormActions {...stepProps} />
            ) : (
              <p>No releases available.</p>
            )}
          </Form>
        ) : (
          <ResetFormOnPreviousStep
            currentStep={currentStep}
            stepNumber={stepNumber}
          />
        );
      }}
    </Formik>
  );
};

export default ReleaseForm;
