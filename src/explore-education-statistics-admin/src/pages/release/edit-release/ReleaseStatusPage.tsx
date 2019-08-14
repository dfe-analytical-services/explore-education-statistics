import Link from "@admin/components/Link";
import ManageReleaseContext, {ManageRelease,} from '@admin/pages/release/ManageReleaseContext';
import dashboardRoutes from "@admin/routes/dashboard/routes";
import service from '@admin/services/release/edit-release/status/service';
import Button from "@common/components/Button";
import {Form, FormFieldRadioGroup, Formik} from "@common/components/form";
import FormFieldTextInput from "@common/components/form/FormFieldTextInput";
import Yup from "@common/lib/validation/yup";
import {FormikProps} from "formik";
import React, {useContext, useEffect, useState} from 'react';
import {RouteComponentProps, withRouter} from "react-router";

interface FormValues {
  releaseStatusId: string;
  releaseNotes: string;
}

const ReleaseStatusPage = ({history}: RouteComponentProps) => {
  const [releaseStatusId, setReleaseStatusId] = useState('');

  const { releaseId } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  useEffect(
    () => {
      service.getReleaseStatus(releaseId).then(setReleaseStatusId);
    }, [releaseId]
  );

  const formId = 'releaseStatusForm';

  return (
    <>
      <h2 className="govuk-heading-m">Update release status</h2>
      <p>Select and update the release status.</p>

      {releaseStatusId && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            releaseStatusId,
            releaseNotes: '',
          }}
          onSubmit={async (values: FormValues, actions) => {
            await service.updateReleaseStatus(releaseId, values);
            history.push(dashboardRoutes.adminDashboard);
          }}
          validationSchema={Yup.object<FormValues>({
            releaseStatusId: Yup.string().required('Choose a status'),
            releaseNotes: Yup.string().required('Provide some release notes')
          })}
          render={(form: FormikProps<FormValues>) => {
            return (
              <Form id={formId}>
                <FormFieldRadioGroup<FormValues>
                  legend='Status'
                  name='releaseStatusId'
                  id={`${formId}-releaseStatusId`}
                  options={[
                    {
                      label: 'In draft',
                      value: 'draft',
                    },
                    {
                      label: 'Ready for higher review',
                      value: 'higher-review',
                    },
                  ]}
                />
                <FormFieldTextInput
                  name='releaseNotes'
                  id={`${formId}-releaseNotes`}
                  label='Release notes'
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
            )
          }}
        />
      )}
    </>
  );
};

export default ReleaseStatusPage;
