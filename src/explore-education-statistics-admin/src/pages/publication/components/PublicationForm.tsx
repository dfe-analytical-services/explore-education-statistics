import FormFieldThemeTopicSelect from '@admin/components/form/FormFieldThemeTopicSelect';
import themeService from '@admin/services/themeService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormFieldset } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { ReactNode, useMemo, useState } from 'react';
import ModalConfirm from '@common/components/ModalConfirm';

export interface FormValues {
  title: string;
  topicId?: string;
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'title',
    messages: {
      SlugNotUnique: 'Choose a unique title',
    },
  }),
];
interface Props {
  cancelButton?: ReactNode;
  id?: string;
  initialValues?: FormValues;
  onSubmit: (values: FormValues) => void;
  confirmOnSubmit?: boolean;
  showTitleInput?: boolean;
}

const PublicationForm = ({
  cancelButton,
  id = 'publicationForm',
  initialValues,
  confirmOnSubmit = false,
  showTitleInput = true,
  onSubmit,
}: Props) => {
  const {
    value: themes = [],
    isLoading: isThemesLoading,
  } = useAsyncHandledRetry(themeService.getThemes);

  const [showConfirmSubmitModal, setShowConfirmSubmitModal] = useState<boolean>(
    false,
  );

  const validationSchema = useMemo(() => {
    const schema = Yup.object<FormValues>({
      title: Yup.string().required('Enter a publication title'),
      teamName: Yup.string().required('Enter a team name'),
      teamEmail: Yup.string()
        .required('Enter a team email address')
        .email('Enter a valid team email address'),
      contactName: Yup.string().required('Enter a contact name'),
      contactTelNo: Yup.string().required('Enter a contact telephone number'),
    });

    if (initialValues?.topicId) {
      return schema.shape({
        topicId: Yup.string().required('Choose a topic'),
      });
    }

    return schema;
  }, [initialValues?.topicId]);

  const handleSubmit = useFormSubmit(async (values: FormValues) => {
    await onSubmit(values);
  }, errorMappings);

  if (isThemesLoading) {
    return <LoadingSpinner />;
  }

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        ...(initialValues ?? {
          title: '',
          teamName: '',
          teamEmail: '',
          contactName: '',
          contactTelNo: '',
        }),
      }}
      validationSchema={validationSchema}
      onSubmit={handleSubmit}
    >
      {form => (
        <>
          <Form id={id}>
            {showTitleInput && (
              <FormFieldTextInput<FormValues>
                label="Publication title"
                name="title"
                className="govuk-!-width-two-thirds"
              />
            )}

            {initialValues?.topicId && (
              <FormFieldThemeTopicSelect<FormValues>
                name="topicId"
                legend="Choose a topic for this publication"
                legendSize="m"
                id={id}
                themes={themes}
              />
            )}

            <FormFieldset
              id="contact"
              legend="Contact for this publication"
              legendSize="m"
              hint="They will be the main point of contact for data and methodology enquiries for this publication and its releases."
            >
              <FormFieldTextInput<FormValues>
                name="teamName"
                label="Team name"
                className="govuk-!-width-one-half"
              />

              <FormFieldTextInput<FormValues>
                name="teamEmail"
                label="Team email address"
                className="govuk-!-width-one-half"
              />

              <FormFieldTextInput<FormValues>
                name="contactName"
                label="Contact name"
                className="govuk-!-width-one-half"
              />

              <FormFieldTextInput<FormValues>
                name="contactTelNo"
                label="Contact telephone number"
                width={10}
              />
            </FormFieldset>

            <ButtonGroup>
              <Button
                type="submit"
                onClick={e => {
                  e.preventDefault();
                  if (confirmOnSubmit && form.isValid) {
                    setShowConfirmSubmitModal(true);
                  } else {
                    form.submitForm();
                  }
                }}
              >
                Save publication
              </Button>
              {cancelButton}
            </ButtonGroup>
          </Form>
          <ModalConfirm
            title="Confirm publication changes"
            onConfirm={() => {
              form.submitForm();
              setShowConfirmSubmitModal(false);
            }}
            onExit={() => setShowConfirmSubmitModal(false)}
            onCancel={() => setShowConfirmSubmitModal(false)}
            open={showConfirmSubmitModal}
          >
            <p>
              Any changes made here will appear on the public site immediately.
            </p>
            <p>Are you sure you want to save the changes?</p>
          </ModalConfirm>
        </>
      )}
    </Formik>
  );
};

export default PublicationForm;
