import React, { useState } from 'react';
import {
  FormGroup,
  FormFieldset,
  FormTextInput,
  FormSelect,
  FormRadioGroup,
} from '@common/components/form';
import Link from '../../../components/Link';

interface Props {
  sectionId?: string;
  action?: string;
}

const PrototypePublicationConfig = ({ sectionId }: Props) => {
  const [value, setValue] = useState('academic-year');

  // const formAction = `publication-${sectionId}`;

  return (
    <>
      {sectionId === 'confirmPublication' && (
        <h2 className="govuk-heading-m">Confirm publication details</h2>
      )}
      {sectionId === 'editPublication' && (
        <h2 className="govuk-heading-m">Edit publication details</h2>
      )}

      <form method="get">
        {sectionId !== 'confirmPublication' && (
          <>
            <div className="govuk-form-group">
              <label htmlFor="title" className="govuk-label govuk-label--s">
                Publication title
              </label>
              <input
                className="govuk-input"
                id="title"
                name="title"
                type="text"
                defaultValue="Pupil absence statistics and data for schools in England: summer term"
              />
            </div>

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

            <FormGroup>
              <FormFieldset id="lead-statisician" legend="Lead statistician">
                <FormSelect
                  id="select-lead-statisician"
                  label="Select lead statistician"
                  name="select-lead-statisician"
                  options={[
                    { label: 'Mark Pearson', value: 'mark-pearson' },
                    { label: 'Alex Miller', value: 'alex-miller' },
                  ]}
                />
              </FormFieldset>
            </FormGroup>
            <dl className="govuk-summary-list govuk-width-container">
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Email:</dt>
                <dd className="govuk-summary-list__value">
                  example@email.co.uk
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Telephone:</dt>
                <dd className="govuk-summary-list__value">07954 765423</dd>
              </div>
            </dl>
          </>
        )}
        {sectionId === 'confirmPublication' && (
          <>
            <dl className="govuk-summary-list">
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Publication title</dt>
                <dd className="govuk-summary-list__value">
                  Pupil absence statistics and data for schools in England:
                  summer term
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Methodology</dt>
                <dd className="govuk-summary-list__value">
                  <a href="#">A guide to absence statistics</a>
                </dd>
              </div>

              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Lead statistician</dt>
                <dd className="govuk-summary-list__value">Alex Miller</dd>
              </div>

              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key" />
                <dd className="govuk-summary-list__actions">
                  <Link to="/prototypes/publication-edit-new">
                    Edit publication details
                  </Link>
                </dd>
              </div>
            </dl>
          </>
        )}
        {sectionId === 'newPublication' && (
          <div className="govuk-!-margin-top-6">
            <input type="hidden" name="status" value="confirmPublication" />

            <button
              type="submit"
              className="govuk-button govuk-!-margin-right-3"
              formAction="publication-confirm-new"
            >
              Create new publication
            </button>

            <Link to="/prototypes/admin-dashboard">Cancel publication</Link>
          </div>
        )}
        {sectionId === 'editPublication' && (
          <div className="govuk-!-margin-top-6">
            <input type="hidden" name="status" value="confirmPublication" />
            <button
              type="submit"
              className="govuk-button"
              formAction="publication-confirm-new"
            >
              Edit publication
            </button>

            <div className="govuk-!-margin-top-6">
              <Link to="/prototypes/admin-dashboard">Cancel update</Link>
            </div>
          </div>
        )}
        {sectionId === 'confirmPublication' && (
          <div className="govuk-!-margin-top-6">
            <input type="hidden" name="status" value="newPublication" />
            <button
              type="submit"
              className="govuk-button"
              formAction="admin-dashboard"
            >
              Confirm and create new publication
            </button>

            <div className="govuk-!-margin-top-6">
              <Link to="/prototypes/admin-dashboard">Cancel publication</Link>
            </div>
          </div>
        )}
      </form>
    </>
  );
};

export default PrototypePublicationConfig;
