import LoadingSpinner from '@common/components/LoadingSpinner';
import Button from '@common/components/Button';
import Form from '@common/components/form/Form';
import FormProvider from '@common/components/form/FormProvider';
import Panel from '@common/components/Panel';
import Yup from '@common/validation/yup';
import Page from '@frontend/components/Page';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import React from 'react';
import { GetServerSideProps, NextPage } from 'next';
import Head from 'next/head';
import { ReleasePublishingFeedbackResponse } from '@common/services/types/releasePublishingFeedback';
import releasePublishingFeedbackService from '@frontend/services/releasePublishingFeedbackService';
import {
  FormFieldRadioGroup,
  FormFieldTextArea,
} from '@common/components/form';
import ButtonGroup from '@common/components/ButtonGroup';

const feedbackMaxLength = 2000;

interface FormValues {
  response: ReleasePublishingFeedbackResponse;
  additionalFeedback?: string;
}

interface Props {
  emailToken: string;
  initialResponse: ReleasePublishingFeedbackResponse;
}

const ReleasePublishingFeedbackPage: NextPage<Props> = ({
  emailToken,
  initialResponse,
}) => {
  const handleFormSubmit = async ({
    response,
    additionalFeedback,
  }: FormValues) => {
    await releasePublishingFeedbackService.sendFeedback({
      emailToken,
      response,
      additionalFeedback: additionalFeedback?.length
        ? additionalFeedback
        : undefined,
    });
  };

  return (
    <Page
      title="Your recent publishing experience"
      caption="Feedback"
      breadcrumbLabel="Feedback"
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
      ]}
    >
      <Head>
        <meta name="robots" content="noindex,nofollow" />
        <meta name="googlebot" content="noindex,nofollow" />
      </Head>
      <FormProvider
        enableReinitialize
        initialValues={{
          response: initialResponse,
        }}
        validationSchema={Yup.object({
          response: Yup.string()
            .required('Select a response')
            .oneOf<ReleasePublishingFeedbackResponse>([
              'ExtremelySatisfied',
              'VerySatisfied',
              'Satisfied',
              'SlightlyDissatisfied',
              'NotSatisfiedAtAll',
            ]),
          additionalFeedback: Yup.string().max(
            feedbackMaxLength,
            `Additional feedback must be ${feedbackMaxLength} characters or fewer`,
          ),
        })}
      >
        {({ formState }) => (
          <LoadingSpinner
            loading={formState.isSubmitting || formState.isLoading}
            alert
            text="Submitting feedback"
          >
            {!formState.isSubmitSuccessful ? (
              <Form
                id="releasePublishingFeedbackForm"
                onSubmit={handleFormSubmit}
              >
                <FormFieldRadioGroup<FormValues>
                  legend="How satisfied were you with your recent publishing experience?"
                  name="response"
                  order={[]}
                  options={[
                    {
                      label: 'Extremely satisfied',
                      value: 'ExtremelySatisfied',
                    },
                    {
                      label: 'Very satisfied',
                      value: 'VerySatisfied',
                    },
                    {
                      label: 'Satisfied',
                      value: 'Satisfied',
                    },
                    {
                      label: 'Slightly dissatisfied',
                      value: 'SlightlyDissatisfied',
                    },
                    {
                      label: 'Not satisfied at all',
                      value: 'NotSatisfiedAtAll',
                    },
                  ]}
                />
                <FormFieldTextArea
                  name="additionalFeedback"
                  label="Additional feedback (optional)"
                  maxLength={feedbackMaxLength}
                />
                <ButtonGroup>
                  <Button type="submit" disabled={formState.isSubmitting}>
                    Send
                  </Button>
                </ButtonGroup>
              </Form>
            ) : (
              <div aria-live="polite">
                <Panel headingTag="h2" title="Feedback received">
                  <p>Thank you.</p>
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
    const { token, response } = query;
    return {
      props: {
        emailToken: token as string,
        initialResponse: response as ReleasePublishingFeedbackResponse,
      },
    };
  },
);

export default ReleasePublishingFeedbackPage;
