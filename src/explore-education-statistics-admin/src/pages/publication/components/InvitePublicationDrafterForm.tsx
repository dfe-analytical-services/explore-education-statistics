import Button from '@common/components/Button';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';
import LoadingSpinner from '@common/components/LoadingSpinner';

interface InviteDrafterFormValues {
  email: string;
}

export const errorMappings = [
  mapFieldErrors<InviteDrafterFormValues>({
    target: 'email',
    messages: {
      UserAlreadyHasResourceRole:
        'This user has already been invited with these permissions',
      UserAlreadyHasMorePowerfulRole:
        'This user has already been invited with more powerful permissions than a publication drafter',
    },
  }),
];

interface Props {
  isLoading: boolean;
  onInviteDrafter: (email: string) => Promise<void>;
}

const InvitePublicationDrafterForm = ({
  isLoading,
  onInviteDrafter,
}: Props) => {
  const inviteDrafterInitialValues: InviteDrafterFormValues = {
    email: '',
  };

  const inviteDrafterValidationSchema = useMemo<
    ObjectSchema<InviteDrafterFormValues>
  >(() => {
    return Yup.object({
      email: Yup.string()
        .required('Enter an email address')
        .email('Enter a valid email address'),
    });
  }, []);

  const handleInviteDrafter = async (values: InviteDrafterFormValues) => {
    await onInviteDrafter(values.email);
  };

  return (
    <LoadingSpinner loading={isLoading}>
      <FormProvider
        errorMappings={errorMappings}
        initialValues={inviteDrafterInitialValues}
        validationSchema={inviteDrafterValidationSchema}
        reValidateMode="onSubmit"
        resetAfterSubmit
      >
        {({ formState }) => {
          return (
            <Form id="inviteDrafterForm" onSubmit={handleInviteDrafter}>
              <FormFieldTextInput
                className="govuk-!-width-one-third"
                name="email"
                label="Enter an email address"
              />
              <Button type="submit" disabled={formState.isSubmitting}>
                Invite drafter
              </Button>
            </Form>
          );
        }}
      </FormProvider>
    </LoadingSpinner>
  );
};

export default InvitePublicationDrafterForm;
