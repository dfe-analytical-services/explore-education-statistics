import StatusBlock from '@admin/components/StatusBlock';
import { MethodologyRouteParams } from '@admin/routes/methodologyRoutes';
import methodologyService, {
  MethodologyStatus,
} from '@admin/services/methodologyService';
import permissionService from '@admin/services/permissionService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';
import { RouteComponentProps } from 'react-router';

interface FormValues {
  status: MethodologyStatus;
  internalReleaseNote: string;
}

const statusMap: Dictionary<string> = {
  Draft: 'In Draft',
  Approved: 'Approved',
};

const formId = 'methodologyStatusForm';

const MethodologyStatusPage = ({
  match,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const [showForm, toggleForm] = useToggle(false);

  const {
    value: model,
    setState: setModel,
    isLoading,
  } = useAsyncRetry(async () => {
    const [summary, canApprove, canMarkAsDraft] = await Promise.all([
      methodologyService.getMethodology(methodologyId),
      permissionService.canApproveMethodology(methodologyId),
      permissionService.canMarkMethodologyAsDraft(methodologyId),
    ]);

    return {
      summary,
      canApprove,
      canMarkAsDraft,
    };
  }, [methodologyId]);

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    if (!model) {
      return;
    }

    const nextSummary = await methodologyService.updateMethodology(
      methodologyId,
      {
        ...model.summary,
        ...values,
      },
    );

    setModel({
      isLoading: false,
      value: {
        ...model,
        summary: {
          ...nextSummary,
        },
      },
    });

    toggleForm.off();
  });

  const isEditable = model?.canApprove || model?.canMarkAsDraft;
  const isPublished = model?.summary.published;

  return (
    <>
      <LoadingSpinner loading={isLoading}>
        {model ? (
          <>
            {!showForm ? (
              <>
                <h2>Methodology status</h2>

                <div className="govuk-!-margin-bottom-6">
                  The current methodology status is:{' '}
                  <StatusBlock text={statusMap[model.summary.status]} />
                </div>

                {isEditable && (
                  <Button
                    className="govuk-!-margin-top-2"
                    onClick={toggleForm.on}
                  >
                    Edit status
                  </Button>
                )}
              </>
            ) : (
              <Formik<FormValues>
                initialValues={{
                  status: model.summary.status,
                  internalReleaseNote: '',
                }}
                onSubmit={handleSubmit}
                validationSchema={Yup.object<FormValues>({
                  status: Yup.mixed().required('Choose a status'),
                  internalReleaseNote: Yup.string().when('status', {
                    is: 'Approved',
                    then: Yup.string().required(
                      'Enter an internal release note',
                    ),
                  }),
                })}
              >
                {form => {
                  return (
                    <Form id={formId}>
                      <h2>Edit methodology status</h2>

                      <FormFieldRadioGroup<FormValues, MethodologyStatus>
                        legend="Status"
                        hint={
                          isPublished &&
                          'Once approved, changes will be available to the public immediately.'
                        }
                        name="status"
                        options={[
                          {
                            label: 'In draft',
                            value: 'Draft',
                          },
                          {
                            label: 'Approved for publication',
                            value: 'Approved',
                            conditional: (
                              <FormFieldTextArea<FormValues>
                                name="internalReleaseNote"
                                className="govuk-!-width-one-half"
                                label="Internal release note"
                                rows={2}
                              />
                            ),
                          },
                        ]}
                        orderDirection={[]}
                      />

                      <ButtonGroup>
                        <Button type="submit">Update status</Button>
                        <ButtonText
                          onClick={() => {
                            form.resetForm();
                            toggleForm.off();
                          }}
                        >
                          Cancel
                        </ButtonText>
                      </ButtonGroup>
                    </Form>
                  );
                }}
              </Formik>
            )}
          </>
        ) : (
          <WarningMessage>Could not load methodology status</WarningMessage>
        )}
      </LoadingSpinner>
    </>
  );
};

export default MethodologyStatusPage;
