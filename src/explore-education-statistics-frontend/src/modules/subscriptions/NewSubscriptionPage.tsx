import React from 'react';
import { GetServerSideProps, NextPage } from 'next';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Button from '@common/components/Button';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import useMounted from '@common/hooks/useMounted';
import publicationService, {
  PublicationTitle,
} from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import Page from '@frontend/components/Page';
import notificationService from '@frontend/services/notificationService';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import SubscriptionStatusMessage from '@frontend/modules/subscriptions/components/SubscriptionStatusMessage';

interface FormValues {
  email: string;
}

interface Props {
  slug: string;
  data: PublicationTitle;
}

const SubscriptionPage: NextPage<Props> = ({ data, slug }) => {
  const { isMounted } = useMounted();

  if (!isMounted) {
    return null;
  }

  const handleFormSubmit = async ({ email }: FormValues) => {
    if (email !== '') {
      const { id, title } = data;

      await notificationService.requestPendingSubscription({
        email,
        id,
        slug,
        title,
      });
    }
  };

  return (
    <Page
      title={data.title}
      caption="Notify me"
      breadcrumbLabel="Notify me"
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        { name: data.title, link: `/find-statistics/${slug}` },
      ]}
    >
      <FormProvider
        enableReinitialize
        initialValues={{
          email: '',
        }}
        validationSchema={Yup.object({
          email: Yup.string()
            .required('Email is required')
            .email('Enter a valid email'),
        })}
      >
        {({ formState }) => (
          <LoadingSpinner
            loading={formState.isSubmitting || formState.isLoading}
          >
            {!formState.isSubmitSuccessful && (
              <>
                <p>Subscribe to receive updates when:</p>
                <ul className="govuk-list govuk-list--bullet">
                  <li>new statistics and data are released</li>
                  <li>existing statistics and data are changed or corrected</li>
                </ul>

                <Form id="subscriptionForm" onSubmit={handleFormSubmit}>
                  <FormFieldTextInput<FormValues>
                    label="Enter your email address"
                    hint="This will only be used to subscribe you to updates. You can unsubscribe at any time"
                    name="email"
                    width={20}
                  />

                  <Button type="submit" disabled={formState.isSubmitting}>
                    {formState.isSubmitting && formState.isValid
                      ? 'Submitting'
                      : 'Subscribe'}
                  </Button>
                </Form>
              </>
            )}
            {formState.isSubmitSuccessful && (
              <SubscriptionStatusMessage
                title="Subscribed"
                message="Thank you. Check your email to confirm your subscription."
              />
            )}
          </LoadingSpinner>
        )}
      </FormProvider>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async ({ query }) => {
    const { publicationSlug } = query;

    const data = await publicationService.getPublicationTitle(
      publicationSlug as string,
    );

    return {
      props: {
        data,
        slug: publicationSlug as string,
      },
    };
  },
);

export default SubscriptionPage;
