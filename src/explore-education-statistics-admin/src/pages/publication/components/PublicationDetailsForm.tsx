import RHFFormFieldThemeTopicSelect from '@admin/components/form/RHFFormFieldThemeTopicSelect';
import publicationService from '@admin/services/publicationService';
import themeService from '@admin/services/themeService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldSelect from '@common/components/form/rhf/RHFFormFieldSelect';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

const id = 'publicationDetailsForm';

export interface FormValues {
  summary: string;
  supersededById?: string;
  title: string;
  topicId: string;
}

interface Props {
  canUpdatePublication?: boolean;
  canUpdatePublicationSummary?: boolean;
  initialValues: FormValues;
  publicationId: string;
  onCancel: () => void;
  onSubmit: () => void;
}

const PublicationDetailsForm = ({
  canUpdatePublication = false,
  canUpdatePublicationSummary = false,
  initialValues,
  publicationId,
  onCancel,
  onSubmit,
}: Props) => {
  const [showConfirmModal, toggleConfirmModal] = useToggle(false);

  const { value, isLoading } = useAsyncHandledRetry(async () => {
    const themes = await themeService.getThemes();
    if (!canUpdatePublication) {
      return { themes };
    }

    const allPublications = await publicationService.getPublicationSummaries();
    const publications = allPublications.filter(
      publication => publication.id !== publicationId,
    );

    return {
      themes,
      publications,
    };
  });

  const handleSubmit = async (values: FormValues) => {
    await publicationService.updatePublication(publicationId, {
      ...values,
    });
    onSubmit();
  };

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      summary: Yup.string()
        .required('Enter a summary')
        .max(160, 'Summary must be 160 characters or less'),
      supersededById: Yup.string(),
      title: Yup.string().required('Enter a title'),
      topicId: Yup.string().required('Choose a topic'),
    });
  }, []);

  const { themes, publications } = value ?? {};

  return (
    <LoadingSpinner loading={isLoading}>
      <FormProvider
        enableReinitialize
        initialValues={{
          ...(initialValues ?? {
            theme: '',
            title: '',
            summary: '',
            topicId: '',
          }),
        }}
        validationSchema={validationSchema}
      >
        {({ getValues }) => {
          return (
            <>
              <RHFForm id={id} onSubmit={async () => toggleConfirmModal.on()}>
                <FormFieldset id="details" legend="Publication details">
                  {canUpdatePublication && (
                    <RHFFormFieldTextInput<FormValues>
                      name="title"
                      label="Publication title"
                      className="govuk-!-width-one-half"
                    />
                  )}

                  {canUpdatePublicationSummary && (
                    <RHFFormFieldTextArea<FormValues>
                      name="summary"
                      label="Publication summary"
                      className="govuk-!-width-one-half"
                      maxLength={160}
                    />
                  )}
                  {canUpdatePublication && themes && initialValues?.topicId && (
                    <RHFFormFieldThemeTopicSelect<FormValues>
                      id={id}
                      inline={false}
                      legend="Choose a topic for this publication"
                      legendHidden
                      name="topicId"
                      themes={themes}
                    />
                  )}
                </FormFieldset>
                {canUpdatePublication && (
                  <FormFieldset
                    id="supersede"
                    legend="Archive this publication"
                    legendSize="s"
                  >
                    <RHFFormFieldSelect<FormValues>
                      className="govuk-!-width-one-half"
                      hint="If superseded by a publication with a live release, this will archive the current publication immediately"
                      label="Superseding publication"
                      name="supersededById"
                      options={publications?.map(publication => ({
                        label: publication.title,
                        value: publication.id,
                      }))}
                      placeholder="None selected"
                    />
                  </FormFieldset>
                )}

                <ButtonGroup>
                  <Button type="submit">Update publication details</Button>
                  <ButtonText onClick={onCancel}>Cancel</ButtonText>
                </ButtonGroup>
              </RHFForm>
              <ModalConfirm
                title="Confirm publication changes"
                onConfirm={async () => {
                  const values = getValues();
                  await handleSubmit(values);
                }}
                onExit={toggleConfirmModal.off}
                onCancel={toggleConfirmModal.off}
                open={showConfirmModal}
              >
                <p>
                  Any changes made here will appear on the public site
                  immediately.
                </p>
                <p>Are you sure you want to save the changes?</p>
              </ModalConfirm>
            </>
          );
        }}
      </FormProvider>
    </LoadingSpinner>
  );
};

export default PublicationDetailsForm;
