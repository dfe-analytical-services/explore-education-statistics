import React, { useState } from 'react';
import {
  FormGroup,
  FormTextInput,
  FormSelect,
  FormRadioGroup,
} from '@common/components/form';
import Link from '@admin/components/Link';
import PrototypePage from './components/PrototypePage';

const PublicationPage = () => {
  const [value, setValue] = useState('academic-year');
  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard',
          text: 'Administrator dashboard',
        },
        { text: 'Assign methodology', link: '#' },
      ]}
    >
      <h1 className="govuk-heading-xl">
        Pupil absence statistics and data for schools in England
        <span className="govuk-caption-l">Assign methodology</span>
      </h1>
      <form method="get">
        <FormGroup>
          <FormRadioGroup
            legend="Methodology"
            id="methodology"
            name="methodology"
            value={value}
            onChange={event => {
              setValue(event.target.value);
            }}
            options={[
              {
                id: 'existing-methodology',
                label: 'Add existing methodology to this publication',
                value: 'existing-methodlogy',
                conditional: (
                  <FormSelect
                    id="methodology"
                    label="Select methodology"
                    name="methodology"
                    options={[
                      {
                        label: 'A guide to absence statistics',
                        value: 'absence-statistics',
                      },
                      {
                        label: 'Children missing education',
                        value: 'children-missing-education',
                      },
                      {
                        label: 'School attendance',
                        value: 'school-attendance',
                      },
                      {
                        label:
                          'School attendance parental responsibility measures',
                        value: 'responsibility-measures',
                      },
                    ]}
                  />
                ),
              },
              {
                id: 'url-methodology',
                label: 'Link to externally hosted methodology',
                value: 'url-methodology',
                conditional: (
                  <>
                    <FormGroup>
                      <FormTextInput
                        id="external-url"
                        name="external-url"
                        label="URL"
                        value="http://"
                      />
                    </FormGroup>
                    <FormGroup>
                      <FormTextInput
                        id="external-title"
                        name="external-title"
                        label="Link title"
                      />
                    </FormGroup>
                  </>
                ),
              },
              {
                id: 'new-methodology',
                label: 'This publication requires new methodology creating',
                value: 'new-methodology',
              },
            ]}
          />
        </FormGroup>
        <button
          className="govuk-button"
          formAction="/prototypes/admin-dashboard"
          type="submit"
        >
          Update methodology
        </button>
      </form>
      <div className="govuk-!-margin-top-6">
        <Link to="/prototypes/admin-dashboard">Cancel </Link>
      </div>
    </PrototypePage>
  );
};

export default PublicationPage;
