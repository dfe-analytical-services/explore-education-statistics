import Page from '@frontend/components/Page';
import React from 'react';
import Link from '@frontend/components/Link';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldset,
  FormGroup,
  Formik,
} from '@common/components/form';
import { FormikProps } from 'formik';
import Yup from '@common/lib/validation/yup';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import * as CookiesService from '@frontend/services/cookiesService';

interface FormValues {
  googleAnalytics: string;
}

function CookiesPage() {
  return (
    <Page
      title="Cookies on Explore education statistics"
      pageMeta={{ title: 'Cookies' }}
      breadcrumbLabel="Cookies"
    >
      <p>
        Cookies are files saved on your phone, tablet or computer when you visit
        a website.
      </p>
      <p>
        We use cookies to store information about how you use the GOV.UK
        website, such as the pages you visit.
      </p>
      <h2 className="govuk-!-margin-top-6">Cookie settings</h2>
      <p>
        We use 2 types of cookie. You can choose which cookies you're happy for
        us to use.
      </p>
      <Formik<FormValues>
        enableReinitialize
        initialValues={{
          googleAnalytics: 'on',
        }}
        onSubmit={async cookiesSettings => {
          CookiesService.setCookies([
            {
              name: CookiesService.cookieSettingsCookieName,
              value: cookiesSettings,
            },
          ]);
          CookiesService.acceptCookies();
        }}
        validationSchema={Yup.object<FormValues>({
          googleAnalytics: Yup.string().required('Select'),
        })}
        render={(form: FormikProps<FormValues>) => {
          const { getError } = createErrorHelper(form);

          return (
            <Form id="cookieSettingsForm">
              <section className="govuk-!-margin-bottom-6">
                <h3>Cookies that measure website use</h3>
                <p>
                  We use Google Analytics to measure how you use the website so
                  we can improve it based on user needs. Google Analytics sets
                  cookies that store anonymised information about:
                </p>
                <ul>
                  <li>how you got to the site</li>
                  <li>
                    the pages you visit on GOV.UK and how long you spend on each
                    page
                  </li>
                  <li>what you click on while you're visiting the site</li>
                </ul>
                <p>
                  We do not allow Google to use or share the data about how you
                  use this site.
                </p>
                <p>
                  <FormFieldset
                    error={getError('googleAnalytics')}
                    id="cookieSettingsForm-googleAnalytics"
                    legend=""
                  >
                    <FormGroup>
                      <FormFieldRadioGroup
                        legend="Google analytics and cookies"
                        legendHidden
                        inline
                        orderDirection={['desc']}
                        showError={false}
                        name="googleAnalytics"
                        id="cookieSettingsForm-googleAnalytics"
                        options={[
                          {
                            id: 'googleAnalytics-on',
                            label: 'On',
                            value: 'on',
                          },
                          {
                            id: 'googleAnalytics-off',
                            label: 'Off',
                            value: 'off',
                          },
                        ]}
                      />
                    </FormGroup>
                  </FormFieldset>
                </p>
              </section>
              <section className="govuk-!-margin-bottom-6">
                <h3>Strictly necessary cookies</h3>
                <p>These essential cookies do things like:</p>
                <ul>
                  <li>
                    remember the notifications you've seen so we do not show
                    them to you again
                  </li>
                  <li>remember your cookie settings</li>
                </ul>
                <p>They always need to be on.</p>
              </section>
              <div className="govuk-!-margin-bottom-6">
                <Link to="/cookies/details">
                  Find out more about cookies on Explore education statistics
                </Link>
              </div>
              <button type="submit" className="govuk-button">
                Save changes
              </button>
            </Form>
          );
        }}
      />
    </Page>
  );
}

export default CookiesPage;
