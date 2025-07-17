import LoadingSpinner from '@common/components/LoadingSpinner';
import Button from '@common/components/Button';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import Form from '@common/components/form/Form';
import FormProvider from '@common/components/form/FormProvider';
import Panel from '@common/components/Panel';
import useMounted from '@common/hooks/useMounted';
import publicationService from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import Page from '@frontend/components/Page';
import notificationService from '@frontend/services/notificationService';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import React from 'react';
import { GetServerSideProps, NextPage } from 'next';
import Head from 'next/head';

interface FormValues {
  email: string;
}

interface Props {
  publicationSlug: string;
  publicationTitle: string;
  publicationId: string;
}

const SubscriptionPage: NextPage<Props> = ({
  publicationSlug,
  publicationTitle,
  publicationId,
}) => {
  const { isMounted } = useMounted();

  if (!isMounted) {
    return null;
  }

  const handleFormSubmit = async ({ email }: FormValues) => {
    if (email !== '') {
      await notificationService.requestPendingSubscription({
        email,
        id: publicationId,
        slug: publicationSlug,
        title: publicationTitle,
      });
    }
  };

  return (
    <Page
      title={publicationTitle}
      caption="Notify me"
      breadcrumbLabel="Notify me"
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        {
          name: publicationTitle,
          link: `/find-statistics/${publicationSlug}`,
        },
      ]}
    >
      <Head>
        <meta name="robots" content="noindex,nofollow" />
        <meta name="googlebot" content="noindex,nofollow" />
      </Head>
      <div role="status">
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
              alert
              text="Subscribing"
            >
              {!formState.isSubmitSuccessful ? (
                <>
                  <p>Subscribe to receive updates when:</p>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>new statistics and data are released</li>
                    <li>
                      existing statistics and data are changed or corrected
                    </li>
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
              ) : (
                <div aria-live="polite">
                  <Panel headingTag="h2" title="Subscribed">
                    <p>
                      Thank you. Check your email to confirm your subscription.
                    </p>
                  </Panel>
                </div>
              )}
            </LoadingSpinner>
          )}
        </FormProvider>
      </div>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async ({ query }) => {
    const { publicationSlug } = query;

    const publication = await publicationService.getPublicationTitle(
      publicationSlug as string,
    );

    return {
      props: {
        publicationSlug: publicationSlug as string,
        publicationId: publication.id,
        publicationTitle: publication.title,
      },
    };
  },
);

export default SubscriptionPage;
