import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';
import StatusBlock from '@admin/components/StatusBlock';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import permissionService from '@admin/services/permissions/service';
import service from '@admin/services/release/edit-release/status/service';
import submitWithFormikValidation from '@admin/validation/formikSubmitHandler';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import Button from '@common/components/Button';
import { Form, FormFieldRadioGroup, Formik } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import Yup from '@common/lib/validation/yup';
import { ReleaseStatus } from '@common/services/publicationService';
import { FormikProps } from 'formik';
import React, { useContext, useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';

interface FormValues {
  releaseStatus: ReleaseStatus;
  internalReleaseNote: string;
}

interface Model {
  releaseStatus: ReleaseStatus;
  statusOptions: RadioOption[];
}

const statusMap: {
  [keyof: string]: string;
} = {
  Draft: 'In Draft',
  HigherLevelReview: 'Awaiting higher review',
  Approved: 'Approved',
};

const ReleaseStatusPage = ({
  history,
  handleApiErrors,
}: RouteComponentProps & ErrorControlProps) => {
  const [model, setModel] = useState<Model>();
  const [showForm, setShowForm] = useState<boolean>(false);

  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;

  useEffect(() => {
    Promise.all([
      service.getReleaseStatus(releaseId),
      permissionService.canSubmitReleaseForHigherLevelReview(releaseId),
      permissionService.canApproveRelease(releaseId),
    ])
      .then(([releaseStatus, canSubmit, canApprove]) => {
        const statusOptions: RadioOption[] = [
          {
            label: 'In draft',
            value: 'Draft',
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
        });
      })
      .catch(handleApiErrors);
  }, [releaseId, handleApiErrors]);

  if (!model) return null;

  const formId = 'releaseStatusForm';

  const errorCodeMappings = [
    errorCodeToFieldError(
      'APPROVED_RELEASE_MUST_HAVE_PUBLISH_SCHEDULED_DATE',
      'releaseStatus',
      'Enter a publish scheduled date before approving',
    ),
  ];

  const submitFormHandler = submitWithFormikValidation<FormValues>(
    async values => {
      await service
        .updateReleaseStatus(releaseId, values)
        .then(() => {
          setModel({
            releaseStatus: values.releaseStatus,
            statusOptions: model.statusOptions,
          });
        })
        .then(() => {
          setShowForm(false);
        });
    },
    handleApiErrors,
    ...errorCodeMappings,
  );

  return (
    <>
      <h2 className="govuk-heading-m">Release Status</h2>
      {!showForm ? (
        <div>
          <p>
            The current release status is:{' '}
            <StatusBlock text={statusMap[model.releaseStatus]} />
            {model.releaseStatus === 'Approved' && (
              <div className="govuk-!-margin-top-1">
                Release process status:{' '}
                <ReleaseServiceStatus releaseId={releaseId} />
              </div>
            )}
          </p>
          <Button onClick={() => setShowForm(true)}>
            Update release status
          </Button>
        </div>
      ) : (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            releaseStatus: model.releaseStatus,
            internalReleaseNote: '',
          }}
          onSubmit={submitFormHandler}
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
                  <Button
                    onClick={() => {
                      form.resetForm();
                      setShowForm(false);
                    }}
                    className="govuk-!-margin-left-1 govuk-button--secondary"
                  >
                    Cancel
                  </Button>
                  <Button type="submit">Update</Button>
                </div>
              </Form>
            );
          }}
        />
      )}
    </>
  );
};

export default withErrorControl(ReleaseStatusPage);
