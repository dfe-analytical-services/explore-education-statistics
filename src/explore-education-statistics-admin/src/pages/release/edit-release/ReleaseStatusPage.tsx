import Link from '@admin/components/Link';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import appRouteList from '@admin/routes/dashboard/routes';
import service from '@admin/services/release/edit-release/status/service';
import permissionService from '@admin/services/permissions/service';
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
import { RouteComponentProps } from 'react-router';

interface FormValues {
  releaseStatus: ReleaseStatus;
  internalReleaseNote: string;
}

interface Model {
  releaseStatus: ReleaseStatus;
  statusOptions: RadioOption[];
}

const ReleaseStatusPage = ({
  history,
  handleApiErrors,
}: RouteComponentProps & ErrorControlProps) => {
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
      <h2 className="govuk-heading-m">Update release status</h2>
      <p>Select and update the release status.</p>

      {model && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            releaseStatus: model.releaseStatus,
            internalReleaseNote: '',
          }}
          onSubmit={async (values: FormValues) => {
            await service
              .updateReleaseStatus(releaseId, values)
              .catch(handleApiErrors);

            history.push(appRouteList.adminDashboard.path as string);
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
    </>
  );
};

export default withErrorControl(ReleaseStatusPage);
