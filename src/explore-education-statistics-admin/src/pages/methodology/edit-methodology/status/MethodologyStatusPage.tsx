import StatusBlock from '@admin/components/StatusBlock';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import { MethodologyStatus } from '@admin/services/common/types';
import methodologyService from '@admin/services/methodology/methodologyService';
import permissionService from '@admin/services/permissions/permissionService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';

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
}: RouteComponentProps<{ methodologyId: string }>) => {
  const { methodologyId } = match.params;

  const [model, setModel] = useState<Model>();
  const [showForm, setShowForm] = useState(false);

  useEffect(() => {
    Promise.all([
      methodologyService.getMethodologyStatus(methodologyId),
      permissionService.canMarkMethodologyAsDraft(methodologyId),
      permissionService.canApproveMethodology(methodologyId),
    ]).then(([methodologyStatus, canMarkAsDraft, canApprove]) => {
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
    });
  }, [methodologyId, showForm]);

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    await methodologyService
      .updateMethodologyStatus(methodologyId, values)
      .then(() => {
        if (model) {
          setModel({
            ...model,
            methodologyStatus: values.status,
          });
        }

        setShowForm(false);
      });
  });

  if (!model) return null;

  const formId = 'methodologyStatusForm';

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
          onSubmit={handleSubmit}
          validationSchema={Yup.object<FormValues>({
            status: Yup.mixed().required('Choose a status'),
            internalReleaseNote: Yup.string().required(
              'Provide an internal release note',
            ),
          })}
        >
          {form => {
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

export default MethodologyStatusPage;
