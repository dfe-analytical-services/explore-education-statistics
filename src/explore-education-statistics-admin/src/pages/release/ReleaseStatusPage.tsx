import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';
import StatusBlock from '@admin/components/StatusBlock';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import permissionService from '@admin/services/permissionService';
import releaseService from '@admin/services/releaseService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import { ReleaseApprovalStatus } from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useEffect, useState } from 'react';

const errorCodeMappings = [
  errorCodeToFieldError(
    'APPROVED_RELEASE_MUST_HAVE_PUBLISH_SCHEDULED_DATE',
    'releaseStatus',
    'Enter a publish scheduled date before approving',
  ),
  errorCodeToFieldError(
    'ALL_DATAFILES_UPLOADED_MUST_BE_COMPLETE',
    'releaseStatus',
    'Check all uploaded datafiles are complete before approving',
  ),
];

interface FormValues {
  releaseStatus: ReleaseApprovalStatus;
  internalReleaseNote: string;
}

interface Model {
  releaseStatus: ReleaseApprovalStatus;
  statusOptions: RadioOption[];
  editable: boolean;
}

const statusMap: {
  [keyof: string]: string;
} = {
  Draft: 'In Draft',
  HigherLevelReview: 'Awaiting higher review',
  Approved: 'Approved',
};

const ReleaseStatusPage = () => {
  const [model, setModel] = useState<Model>();
  const [showForm, setShowForm] = useState(false);

  const { releaseId, onChangeReleaseStatus } = useManageReleaseContext();

  useEffect(() => {
    Promise.all([
      releaseService.getReleaseSummary(releaseId),
      permissionService.canMarkReleaseAsDraft(releaseId),
      permissionService.canSubmitReleaseForHigherLevelReview(releaseId),
      permissionService.canApproveRelease(releaseId),
    ]).then(([releaseSummary, canMarkAsDraft, canSubmit, canApprove]) => {
      const statusOptions: RadioOption[] = [
        {
          label: 'In draft',
          value: 'Draft',
          disabled: !canMarkAsDraft,
        },
        {
          label: 'Ready for higher review',
          value: 'HigherLevelReview',
          disabled: !canSubmit,
        },
        {
          label: 'Approved for publication',
          value: 'Approved',
          disabled: !canApprove,
        },
      ];

      setModel({
        releaseStatus: releaseSummary.status,
        statusOptions,
        editable: statusOptions.some(option => !option.disabled),
      });
    });
  }, [releaseId, showForm]);

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    await releaseService.updateReleaseStatus(releaseId, values);

    if (model) {
      setModel({
        ...model,
        releaseStatus: values.releaseStatus,
        statusOptions: model.statusOptions,
      });
    }

    setShowForm(false);

    onChangeReleaseStatus();
  }, errorCodeMappings);

  if (!model) return null;

  const formId = 'releaseStatusForm';

  return (
    <>
      <h2 className="govuk-heading-m">Release Status</h2>
      {!showForm ? (
        <>
          <div className="govuk-!-margin-bottom-6">
            The current release status is:{' '}
            <StatusBlock
              text={statusMap[model.releaseStatus]}
              id={`CurrentReleaseStatus-${statusMap[model.releaseStatus]}`}
            />
            {model.releaseStatus === 'Approved' && (
              <div className="govuk-!-margin-top-1">
                Release process status:{' '}
                <ReleaseServiceStatus releaseId={releaseId} />
              </div>
            )}
          </div>

          {model.editable && (
            <Button
              className="govuk-!-margin-top-2"
              onClick={() => setShowForm(true)}
            >
              Update release status
            </Button>
          )}
        </>
      ) : (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            releaseStatus: model.releaseStatus,
            internalReleaseNote: '',
          }}
          onSubmit={handleSubmit}
          validationSchema={Yup.object<FormValues>({
            releaseStatus: Yup.mixed().required('Choose a status'),
            internalReleaseNote: Yup.string().required(
              'Provide an internal release note',
            ),
          })}
        >
          {form => {
            return (
              <Form id={formId}>
                <p>Select and update the release status.</p>
                <FormFieldRadioGroup<FormValues>
                  legend="Status"
                  name="releaseStatus"
                  id={`${formId}-releaseStatus`}
                  options={model.statusOptions}
                  orderDirection={[]}
                />
                <FormFieldTextArea
                  name="internalReleaseNote"
                  className="govuk-!-width-one-half"
                  id={`${formId}-internalReleaseNote`}
                  label="Internal release note"
                  rows={2}
                />
                <div className="govuk-!-margin-top-6">
                  <Button type="submit" className="govuk-!-margin-right-6">
                    Update
                  </Button>
                  <ButtonText
                    onClick={() => {
                      form.resetForm();
                      setShowForm(false);
                    }}
                    className="govuk-button govuk-button--secondary"
                  >
                    Cancel
                  </ButtonText>
                </div>
              </Form>
            );
          }}
        </Formik>
      )}
    </>
  );
};

export default ReleaseStatusPage;
