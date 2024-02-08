import FormFieldThemeTopicSelect from '@admin/components/form/FormFieldThemeTopicSelect';
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
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';
import PublicationUpdateConfirmModal from '@admin/pages/publication/components/PublicationUpdateConfirmModal';

const id = 'publicationDetailsForm';

interface FormValues {
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
  publicationSlug: string;
  onCancel: () => void;
  onSubmit: () => void | Promise<void>;
}

export default function PublicationDetailsForm({
  canUpdatePublication = false,
  canUpdatePublicationSummary = false,
  initialValues,
  publicationId,
  publicationSlug,
  onCancel,
  onSubmit,
}: Props) {
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
        {form => {
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
                    <FormFieldThemeTopicSelect<FormValues>
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

              {showConfirmModal && (
                <PublicationUpdateConfirmModal
                  title={initialValues.title}
                  slug={publicationSlug}
                  newTitle={form.getValues().title}
                  onConfirm={async () => {
                    await form.handleSubmit(handleSubmit)();
                  }}
                  onExit={toggleConfirmModal.off}
                  onCancel={toggleConfirmModal.off}
                />
              )}
            </>
          );
        }}
      </FormProvider>
    </LoadingSpinner>
  );
}
