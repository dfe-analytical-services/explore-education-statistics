import Link from '@admin/components/Link';
import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import permissionService from '@admin/services/permissions/service';
import service from '@admin/services/release/edit-release/status/service';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import Button from '@common/components/Button';
import { Form, FormFieldRadioGroup, Formik } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import Yup from '@common/lib/validation/yup';
import { ReleaseStatus } from '@common/services/publicationService';
import { FormikProps } from 'formik';
import React, { useContext, useEffect, useState } from 'react';

interface FormValues {
  releaseStatus: ReleaseStatus;
  internalReleaseNote: string;
}

interface Model {
  releaseStatus: ReleaseStatus;
  statusOptions: RadioOption[];
}

const ReleaseStatusPage = ({ handleApiErrors }: ErrorControlProps) => {
  const [model, setModel] = useState<Model>();

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

  const formId = 'releaseStatusForm';

  return (
    <>
      <h2 className="govuk-heading-m">Release status</h2>

      {model && model.releaseStatus !== 'Approved' && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            releaseStatus: model.releaseStatus,
            internalReleaseNote: '',
          }}
          onSubmit={async (values: FormValues) => {
            await service
              .updateReleaseStatus(releaseId, values)
              .then(() => {
                setModel({
                  releaseStatus: values.releaseStatus,
                  statusOptions: model.statusOptions,
                });
              })
              .catch(handleApiErrors);
          }}
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
                <Button type="submit" className="govuk-!-margin-top-6">
                  Update
                </Button>
                <div className="govuk-!-margin-top-6">
                  <Link to="#" onClick={() => form.resetForm()}>
                    Cancel update
                  </Link>
                </div>
              </Form>
            );
          }}
        />
      )}
      {model && model.releaseStatus === 'Approved' && (
        <div>
          <p>This release has been approved</p>
          <ReleaseServiceStatus releaseId={releaseId} />
        </div>
      )}
    </>
  );
};

export default withErrorControl(ReleaseStatusPage);
