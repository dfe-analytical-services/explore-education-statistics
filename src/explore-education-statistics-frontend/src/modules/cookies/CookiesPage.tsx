import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup, FormGroup } from '@common/components/form';
import useMounted from '@common/hooks/useMounted';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import { useCookies } from '@frontend/hooks/useCookies';
import { Formik } from 'formik';
import { GetServerSideProps, NextPage } from 'next';
import { useRouter } from 'next/router';
import { parseCookies } from 'nookies';
import React, { useState } from 'react';
import { StringSchema } from 'yup';
import styles from './CookiesPage.module.scss';

interface FormValues {
  googleAnalytics: 'on' | 'off';
}

interface Props {
  cookies: Dictionary<string>;
}

const CookiesPage: NextPage<Props> = ({ cookies }) => {
  const [submitted, setSubmitted] = useState(false);
  const { back } = useRouter();

  const { getCookie, setBannerSeenCookie, setGADisabledCookie } =
    useCookies(cookies);

  const { isMounted } = useMounted();

  return (
    <Page
      title="Cookies on Explore education statistics"
      pageMeta={{ title: 'Cookies' }}
      breadcrumbLabel="Cookies"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          {submitted && (
            <div
              id="submit-notification"
              className={styles.submitNotification}
              role="alert"
            >
              <h2>Your cookie settings were saved</h2>
              <p>We have stored your cookie settings.</p>
              <p>
                <ButtonText onClick={() => back()}>
                  Go back to the page you were looking at
                </ButtonText>
              </p>
            </div>
          )}

          <p>
            Cookies are files saved on your phone, tablet or computer when you
            visit a website.
          </p>
          <p>
            We use cookies to store information about how you use the GOV.UK
            website, such as the pages you visit.
          </p>

          {isMounted ? (
            <Formik<FormValues>
              enableReinitialize
              initialValues={{
                googleAnalytics:
                  getCookie('disableGA') === 'true' ? 'off' : 'on',
              }}
              onSubmit={values => {
                setSubmitted(true);
                window.scrollTo(0, 0);

                setBannerSeenCookie(true);
                setGADisabledCookie(values.googleAnalytics !== 'on');
              }}
              validationSchema={Yup.object<FormValues>({
                googleAnalytics: Yup.string()
                  .required('Select an option for Google analytics and cookies')
                  .oneOf(['on', 'off']) as StringSchema<'on' | 'off'>,
              })}
            >
              {() => {
                return (
                  <Form id="cookieSettingsForm">
                    <h2 className="govuk-!-margin-top-6">Cookie settings</h2>

                    <p>
                      We use 2 types of cookie. You can choose which cookies
                      you're happy for us to use.
                    </p>

                    <section className="govuk-!-margin-bottom-6">
                      <h3>Cookies that measure website use</h3>
                      <p>
                        We use Google Analytics to measure how you use the
                        website so we can improve it based on user needs. Google
                        Analytics sets cookies that store anonymised information
                        about:
                      </p>
                      <ul>
                        <li>how you got to the site</li>
                        <li>
                          the pages you visit on GOV.UK and how long you spend
                          on each page
                        </li>
                        <li>
                          what you click on while you're visiting the site
                        </li>
                      </ul>
                      <p>
                        We do not allow Google to use or share the data about
                        how you use this site.
                      </p>

                      <FormGroup>
                        <FormFieldRadioGroup<FormValues>
                          legend="Allow Google Analytics cookies"
                          legendHidden
                          inline
                          orderDirection={['desc']}
                          showError={false}
                          name="googleAnalytics"
                          id="googleAnalytics"
                          options={[
                            {
                              label: 'On',
                              value: 'on',
                            },
                            {
                              label: 'Off',
                              value: 'off',
                            },
                          ]}
                        />
                      </FormGroup>
                    </section>
                    <section className="govuk-!-margin-bottom-6">
                      <h3>Strictly necessary cookies</h3>
                      <p>These essential cookies do things like:</p>
                      <ul>
                        <li>
                          remember the notifications you've seen so we do not
                          show them to you again
                        </li>
                        <li>remember your cookie settings</li>
                      </ul>
                      <p>They always need to be on.</p>
                    </section>

                    <p>
                      <Link to="/cookies/details">
                        Find out more about cookies on Explore education
                        statistics
                      </Link>
                    </p>

                    <Button type="submit">Save changes</Button>
                  </Form>
                );
              }}
            </Formik>
          ) : (
            <p>
              <Link to="/cookies/details">
                Find out more about cookies on Explore education statistics
              </Link>
            </p>
          )}
        </div>
      </div>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async context => {
  return {
    props: {
      cookies: parseCookies(context),
    },
  };
};

export default CookiesPage;
