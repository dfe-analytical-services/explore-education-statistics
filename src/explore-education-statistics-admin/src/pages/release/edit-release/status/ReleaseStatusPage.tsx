import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';
import StatusBlock from '@admin/components/StatusBlock';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import ManageReleaseContext from '@admin/pages/release/ManageReleaseContext';
import permissionService from '@admin/services/permissions/service';
import service from '@admin/services/release/edit-release/status/service';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import {Form, FormFieldRadioGroup, Formik} from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import {RadioOption} from '@common/components/form/FormRadioGroup';
import {errorCodeToFieldError} from '@common/components/form/util/serverValidationHandler';
import Yup from '@common/lib/validation/yup';
import {ReleaseStatus} from '@common/services/publicationService';
import {FormikProps} from 'formik';
import React, {useContext, useEffect, useState} from 'react';

const errorCodeMappings = [
  errorCodeToFieldError(
    'APPROVED_RELEASE_MUST_HAVE_PUBLISH_SCHEDULED_DATE',
    'releaseStatus',
    'Enter a publish scheduled date before approving',
  ),
];

interface FormValues {
  releaseStatus: ReleaseStatus;
  internalReleaseNote: string;
}

interface Model {
  releaseStatus: ReleaseStatus;
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

  const { releaseId, onChangeReleaseStatus } = useContext(ManageReleaseContext);

  useEffect(() => {
    Promise.all([
      service.getReleaseStatus(releaseId),
      permissionService.canMarkReleaseAsDraft(releaseId),
      permissionService.canSubmitReleaseForHigherLevelReview(releaseId),
      permissionService.canApproveRelease(releaseId),
    ]).then(([releaseStatus, canMarkAsDraft, canSubmit, canApprove]) => {
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
        releaseStatus,
        statusOptions,
        editable: statusOptions.some(option => !option.disabled),
      });
    });
  }, [releaseId, showForm]);

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    await service.updateReleaseStatus(releaseId, values);

    if (model) {
      setModel({
        ...model,
        releaseStatus: values.releaseStatus,
        statusOptions: model.statusOptions,
      });
    }

    setShowForm(false);

    onChangeReleaseStatus(values.releaseStatus);

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
            <StatusBlock text={statusMap[model.releaseStatus]} />
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
          render={(form: FormikProps<FormValues>) => {
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
                  id={`${formId}-internalReleaseNote`}
                  label="Internal release note"
                  rows={2}
                  additionalClass="govuk-!-width-one-half"
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
        />
      )}
    </>
  );
};

export default ReleaseStatusPage;
