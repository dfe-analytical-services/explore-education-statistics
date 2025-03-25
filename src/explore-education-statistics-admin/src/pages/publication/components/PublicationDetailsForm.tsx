import publicationService from '@admin/services/publicationService';
import themeService from '@admin/services/themeService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import React, { useEffect, useMemo, useRef } from 'react';
import { ObjectSchema } from 'yup';
import PublicationUpdateConfirmModal from '@admin/pages/publication/components/PublicationUpdateConfirmModal';

const id = 'publicationDetailsForm';

interface FormValues {
  summary: string;
  supersededById?: string;
  title: string;
  themeId: string;
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
  const submitButtonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (showConfirmModal === false) {
      submitButtonRef.current?.focus();
    }
  }, [showConfirmModal]);

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
      themeId: Yup.string().required('Choose a theme'),
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
            themeId: '',
          }),
        }}
        validationSchema={validationSchema}
      >
        {form => {
          return (
            <>
              <Form id={id} onSubmit={async () => toggleConfirmModal.on()}>
                <FormFieldset id="details" legend="Publication details">
                  {canUpdatePublication && (
                    <FormFieldTextInput<FormValues>
                      name="title"
                      label="Publication title"
                      className="govuk-!-width-one-half"
                    />
                  )}

                  {canUpdatePublicationSummary && (
                    <FormFieldTextArea<FormValues>
                      name="summary"
                      label="Publication summary"
                      className="govuk-!-width-one-half"
                      maxLength={160}
                    />
                  )}

                  {canUpdatePublication && themes && (
                    <FormFieldSelect<FormValues>
                      className="govuk-!-width-one-half"
                      id={id}
                      inline={false}
                      label="Select theme"
                      name="themeId"
                      options={themes.map(theme => {
                        return {
                          label: theme.title,
                          value: theme.id,
                        };
                      })}
                    />
                  )}
                </FormFieldset>

                {canUpdatePublication && (
                  <FormFieldset
                    id="supersede"
                    legend="Archive this publication"
                    legendSize="s"
                  >
                    <FormFieldSelect<FormValues>
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
                  <Button type="submit" ref={submitButtonRef}>
                    Update publication details
                  </Button>
                  <ButtonText onClick={onCancel}>Cancel</ButtonText>
                </ButtonGroup>
              </Form>

              {showConfirmModal && (
                <PublicationUpdateConfirmModal
                  initialPublicationTitle={initialValues.title}
                  initialPublicationSlug={publicationSlug}
                  newPublicationTitle={form.getValues().title}
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
