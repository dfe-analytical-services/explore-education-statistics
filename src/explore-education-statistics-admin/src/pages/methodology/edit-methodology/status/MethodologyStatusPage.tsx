import StatusBlock from '@admin/components/StatusBlock';
import { MethodologyStatus } from '@admin/services/common/types';
import service from '@admin/services/methodology/service';
import permissionService from '@admin/services/permissions/service';
import submitWithFormikValidation from '@admin/validation/formikSubmitHandler';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup, Formik } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import Yup from '@common/lib/validation/yup';
import { FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

interface FormValues {
  status: MethodologyStatus;
  internalReleaseNote: string;
}

interface Model {
  methodologyStatus: MethodologyStatus;
  statusOptions: RadioOption[];
  editable: boolean;
}

const statusMap: {
  [keyof: string]: string;
} = {
  Draft: 'In Draft',
  Approved: 'Approved',
};

const MethodologyStatusPage = ({
  match,
  handleApiErrors,
}: RouteComponentProps<{ methodologyId: string }> & ErrorControlProps) => {
  const { methodologyId } = match.params;

  const [model, setModel] = useState<Model>();
  const [showForm, setShowForm] = useState(false);

  useEffect(() => {
    Promise.all([
      service.getMethodologyStatus(methodologyId),
      permissionService.canMarkMethodologyAsDraft(methodologyId),
      permissionService.canApproveMethodology(methodologyId),
    ])
      .then(([methodologyStatus, canMarkAsDraft, canApprove]) => {
        const statusOptions: RadioOption[] = [
          {
            label: 'In draft',
            value: 'Draft',
            disabled: !canMarkAsDraft,
          },
          {
            label: 'Approved for publication',
            value: 'Approved',
            disabled: !canApprove,
          },
        ];

        setModel({
          methodologyStatus,
          statusOptions,
          editable: statusOptions.some(option => !option.disabled),
        });
      })
      .catch(handleApiErrors);
  }, [methodologyId, handleApiErrors, showForm]);

  if (!model) return null;

  const formId = 'methodologyStatusForm';

  const submitFormHandler = submitWithFormikValidation<FormValues>(
    async values => {
      await service.updateMethodologyStatus(methodologyId, values).then(() => {
        setModel({
          ...model,
          methodologyStatus: values.status,
        });

        setShowForm(false);
      });
    },
    handleApiErrors,
  );

  return (
    <>
      <h2 className="govuk-heading-m">Methodology Status</h2>
      {!showForm ? (
        <>
          <div className="govuk-!-margin-bottom-6">
            The current methodology status is:{' '}
            <StatusBlock text={statusMap[model.methodologyStatus]} />
          </div>

          {model.editable && (
            <Button
              className="govuk-!-margin-top-2"
              onClick={() => setShowForm(true)}
            >
              Update methodology status
            </Button>
          )}
        </>
      ) : (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            status: model.methodologyStatus,
            internalReleaseNote: '',
          }}
          onSubmit={submitFormHandler}
          validationSchema={Yup.object<FormValues>({
            status: Yup.mixed().required('Choose a status'),
            internalReleaseNote: Yup.string().required(
              'Provide an internal release note',
            ),
          })}
          render={(form: FormikProps<FormValues>) => {
            return (
              <Form id={formId}>
                <p>Select and update the methodology status.</p>
                <FormFieldRadioGroup<FormValues>
                  legend="Status"
                  name="status"
                  id={`${formId}-status`}
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

export default withErrorControl(MethodologyStatusPage);
