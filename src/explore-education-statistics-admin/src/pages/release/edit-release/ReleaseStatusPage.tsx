import Link from "@admin/components/Link";
import ManageReleaseContext, {ManageRelease,} from '@admin/pages/release/ManageReleaseContext';
import dashboardRoutes from "@admin/routes/dashboard/routes";
import service from '@admin/services/release/edit-release/status/service';
import Button from "@common/components/Button";
import {Form, FormFieldRadioGroup, Formik} from "@common/components/form";
import FormFieldTextArea from "@common/components/form/FormFieldTextArea";
import Yup from "@common/lib/validation/yup";
import {FormikProps} from "formik";
import React, {useContext, useEffect, useState} from 'react';
import {RouteComponentProps} from "react-router";

interface FormValues {
  releaseStatus: string;
  releaseNotes: string;
}

const ReleaseStatusPage = ({history}: RouteComponentProps) => {
  const [releaseStatus, setReleaseStatus] = useState('');

  const { releaseId } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  useEffect(
    () => {
      service.getReleaseStatus(releaseId).then(setReleaseStatus);
    }, [releaseId]
  );

  const formId = 'releaseStatusForm';

  return (
    <>
      <h2 className="govuk-heading-m">Update release status</h2>
      <p>Select and update the release status.</p>

      {releaseStatus && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            releaseStatus,
            releaseNotes: '',
          }}
          onSubmit={async (values: FormValues, actions) => {
            await service.updateReleaseStatus(releaseId, values);
            history.push(dashboardRoutes.adminDashboard);
          }}
          validationSchema={Yup.object<FormValues>({
            releaseStatus: Yup.string().required('Choose a status'),
            releaseNotes: Yup.string().required('Provide some release notes')
          })}
          render={(form: FormikProps<FormValues>) => {
            return (
              <Form id={formId}>
                <FormFieldRadioGroup<FormValues>
                  legend='Status'
                  name='releaseStatus'
                  id={`${formId}-releaseStatus`}
                  options={[
                    {
                      label: 'In draft',
                      value: 'Draft',
                    },
                    {
                      label: 'Ready for higher review',
                      value: 'HigherLevelReview',
                    },
                  ]}
                />
                <FormFieldTextArea
                  name='releaseNotes'
                  id={`${formId}-releaseNotes`}
                  label='Release notes'
                  rows={2}
                  additionalClass='govuk-!-width-one-half'
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
