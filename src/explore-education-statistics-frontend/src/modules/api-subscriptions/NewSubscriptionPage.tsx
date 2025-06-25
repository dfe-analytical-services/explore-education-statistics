import LoadingSpinner from '@common/components/LoadingSpinner';
import Button from '@common/components/Button';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import Form from '@common/components/form/Form';
import FormProvider from '@common/components/form/FormProvider';
import Panel from '@common/components/Panel';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Dictionary } from '@common/types';
import Page from '@frontend/components/Page';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import dataSetFileQueries from '@frontend/queries/dataSetFileQueries';
import { DataSetFile } from '@frontend/services/dataSetFileService';
import apiNotificationService from '@frontend/services/apiNotificationService';
import React from 'react';
import { GetServerSideProps, NextPage } from 'next';
import Head from 'next/head';
import { QueryClient } from '@tanstack/react-query';

interface FormValues {
  dataSetId: string;
  email: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'dataSetId',
    messages: {
      ApiPendingSubscriptionAlreadyExists:
        'The user is already subscribed to the API data set.',
      ApiVerifiedSubscriptionAlreadyExists:
        'The user already has a pending subscription for the API data set. They must verify their subscription.',
    },
  }),
];

interface Props {
  dataSetFile: DataSetFile;
}

const NewSubscriptionPage: NextPage<Props> = ({ dataSetFile }) => {
  const { id: dataSetFileId, title } = dataSetFile;
  const dataSetId = dataSetFile.api!.id;

  const handleFormSubmit = async ({ email }: FormValues) => {
    await apiNotificationService.requestPendingSubscription({
      email,
      dataSetId,
      dataSetTitle: title,
    });
  };

  return (
    <Page
      title={title}
      caption="Notify me"
      breadcrumbLabel="Notify me"
      breadcrumbs={[
        { name: 'Data catalogue', link: '/data-catalogue' },
        {
          name: title,
          link: `/data-catalogue/data-set/${dataSetFileId}`,
        },
      ]}
    >
      <Head>
        <meta name="robots" content="noindex,nofollow" />
        <meta name="googlebot" content="noindex,nofollow" />
      </Head>
      <FormProvider
        enableReinitialize
        errorMappings={errorMappings}
        initialValues={{
          dataSetId,
          email: '',
        }}
        validationSchema={Yup.object({
          dataSetId: Yup.string().required(),
          email: Yup.string()
            .required('Email is required')
            .email('Enter a valid email'),
        })}
      >
        {({ formState }) => (
          <LoadingSpinner
            loading={formState.isSubmitting || formState.isLoading}
          >
            {!formState.isSubmitSuccessful ? (
              <>
                <p>
                  Subscribe to receive updates when new versions of this data
                  set are published.
                </p>

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
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async ({ query }) => {
    const { dataSetFileId } = query as Dictionary<string>;

    const queryClient = new QueryClient();

    const dataSetFile = await queryClient.fetchQuery(
      dataSetFileQueries.get(dataSetFileId),
    );

    return {
      props: {
        dataSetFile,
      },
    };
  },
);

export default NewSubscriptionPage;
