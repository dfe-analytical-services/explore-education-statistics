import FormFieldThemeTopicSelect from '@admin/components/form/FormFieldThemeTopicSelect';
import publicationService from '@admin/services/publicationService';
import themeService from '@admin/services/themeService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import {
  FormFieldset,
  FormFieldSelect,
  FormFieldTextArea,
} from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import useToggle from '@common/hooks/useToggle';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

const errorMappings = [
  mapFieldErrors<PublicationDetailsFormValues>({
    target: 'title',
    messages: {
      SlugNotUnique: 'Choose a unique title',
    },
  }),
];

const id = 'publicationDetailsForm';

export interface PublicationDetailsFormValues {
  supersededById?: string;
  title: string;
  summary: string;
  topicId: string;
}

interface Props {
  canUpdatePublication?: boolean;
  canUpdatePublicationSummary?: boolean;
  initialValues: PublicationDetailsFormValues;
  publicationId: string;
  onCancel: () => void;
  onSubmit: (values: PublicationDetailsFormValues) => void;
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

  const { themes, publications } = value ?? {};

  return (
    <LoadingSpinner loading={isLoading}>
      <Formik<PublicationDetailsFormValues>
        enableReinitialize
        initialValues={{
          ...(initialValues ?? {
            theme: '',
            title: '',
            summary: '',
            topicId: '',
          }),
        }}
        validationSchema={Yup.object<PublicationDetailsFormValues>({
          title: Yup.string().required('Enter a title'),
          summary: Yup.string()
            .required('Enter a summary')
            .max(160, 'Summary must be 160 characters or less'),
          topicId: Yup.string().required('Choose a topic'),
        })}
        onSubmit={useFormSubmit(onSubmit, errorMappings)}
      >
        {form => (
          <>
            <Form id={id}>
              <FormFieldset id="details" legend="Publication details">
                {canUpdatePublication && (
                  <FormFieldTextInput<PublicationDetailsFormValues>
                    name="title"
                    label="Publication title"
                    className="govuk-!-width-one-half"
                  />
                )}

                {canUpdatePublicationSummary && (
                  <FormFieldTextArea<PublicationDetailsFormValues>
                    name="summary"
                    label="Publication summary"
                    className="govuk-!-width-one-half"
                    maxLength={160}
                  />
                )}
                {canUpdatePublication && themes && initialValues?.topicId && (
                  <FormFieldThemeTopicSelect<PublicationDetailsFormValues>
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
                  <FormFieldSelect<PublicationDetailsFormValues>
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
                <Button
                  type="submit"
                  onClick={e => {
                    e.preventDefault();
                    if (form.isValid) {
                      toggleConfirmModal.on();
                    } else {
                      form.submitForm();
                    }
                  }}
                >
                  Update publication details
                </Button>
                <ButtonText onClick={onCancel}>Cancel</ButtonText>
              </ButtonGroup>
            </Form>

            <ModalConfirm
              title="Confirm publication changes"
              onConfirm={form.submitForm}
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
        )}
      </Formik>
    </LoadingSpinner>
  );
};

export default PublicationDetailsForm;
