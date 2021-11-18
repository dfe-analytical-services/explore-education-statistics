import Button from '@common/components/Button';
import { FormFieldTextInput, Form } from '@common/components/form';
import useMounted from '@common/hooks/useMounted';
import publicationService, {
  PublicationTitle,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import notificationService from '@frontend/services/notificationService';
import classNames from 'classnames';
import { Formik } from 'formik';
import { GetServerSideProps, NextPage } from 'next';
import React, { useState } from 'react';
import styles from './SubscriptionPage.module.scss';

interface FormValues {
  email: string;
}

const formId = 'subscriptionForm';

interface Props {
  slug: string;
  data: PublicationTitle;
  unsubscribed: string;
  verified: string;
}

const SubscriptionPage: NextPage<Props> = ({
  data,
  slug,
  unsubscribed,
  verified,
}) => {
  const [subscribed, setSubscribed] = useState(false);

  const { isMounted } = useMounted();

  const handleFormSubmit = async ({ email }: FormValues) => {
    if (email !== '') {
      const { id, title } = data;

      await notificationService.subscribeToPublication({
        email,
        id,
        slug,
        title,
      });

      setSubscribed(true);
    }
  };

  let message;
  let title;

  if (unsubscribed) {
    title = 'Unsubscribed';
    message = 'You have successfully unsubscribed from these updates.';
  } else if (verified) {
    title = 'Subscription verified';
    message = 'You have successfully subscribed to these updates.';
  } else if (subscribed) {
    title = 'Subscribed';
    message = 'Thank you. Check your email to confirm your subscription.';
  }

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
      {message ? (
        <div
          className={classNames(
            'govuk-panel',
            'govuk-panel--confirmation',
            styles.panelContainer,
          )}
        >
          <h1 className="govuk-panel__title">{title}</h1>
          <div className="govuk-panel__body">{message}</div>
          {verified && (
            <Link to={`/find-statistics/${slug}`}>View {data.title}</Link>
          )}
        </div>
      ) : (
        <>
          <p>Subscribe to receive updates when:</p>
          <ul className="govuk-list govuk-list--bullet">
            <li>new statistics and data are released</li>
            <li>existing statistics and data are changed or corrected</li>
          </ul>

          {isMounted && (
            <Formik<FormValues>
              initialValues={{
                email: '',
              }}
              validationSchema={Yup.object({
                email: Yup.string()
                  .required('Email is required')
                  .email('Enter a valid email'),
              })}
              onSubmit={handleFormSubmit}
            >
              {form => (
                <Form id={formId} showSubmitError>
                  <FormFieldTextInput<FormValues>
                    label="Enter your email address"
                    hint="This will only be used to subscribe you to updates. You can unsubscribe at any time"
                    name="email"
                    width={20}
                  />

                  <Button type="submit" disabled={form.isSubmitting}>
                    {form.isSubmitting && form.isValid
                      ? 'Submitting'
                      : 'Subscribe'}
                  </Button>
                </Form>
              )}
            </Formik>
          )}
        </>
      )}
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const { slug, unsubscribed = '', verified = '' } = query as Dictionary<
    string
  >;

  const data = await publicationService.getPublicationTitle(slug as string);

  return {
    props: {
      data,
      slug,
      unsubscribed,
      verified,
    },
  };
};

export default SubscriptionPage;
