import React, { useState } from 'react';
import {
  FormGroup,
  FormFieldset,
  FormSelect,
  FormRadioGroup,
} from '@common/components/form';
import Link from '@admin/components/Link';

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
                Enter publication title
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
                legend="Choose a methodology for this publication"
                id="methodology"
                name="methodology"
                value={value}
                onChange={event => {
                  setValue(event.target.value);
                }}
                options={[
                  {
                    id: 'existing-methodology',
                    label: 'Add existing methodology',
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
                    id: 'new-methodology',
                    label: 'Create new methodology',
                    value: 'new-methodology',
                  },
                ]}
              />
            </FormGroup>

            <FormGroup>
              <FormFieldset
                id="lead-statisician"
                legend="Choose the contact for this publication
                "
              >
                <p className="govuk-hint">
                  They will be the main point of contact for data and
                  methodology enquiries for this publication and its releases.
                </p>
                <FormSelect
                  id="select-lead-statisician"
                  label="Publication and release contact"
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
                <dt className="govuk-summary-list__key">Email</dt>
                <dd className="govuk-summary-list__value">
                  example@email.co.uk
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Telephone</dt>
                <dd className="govuk-summary-list__value">07954 765423</dd>
              </div>
            </dl>

            <h2 className="govuk-heading-m govuk-!-margin-top-9">
              Production team
            </h2>
            <dl className="govuk-summary-list govuk-width-container govuk-!-margin-bottom-9">
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">
                  Responsible statistician
                </dt>
                <dd className="govuk-summary-list__value">
                  <a href="mailto: example@email.co.uk">Stephen Doherty</a>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Team lead</dt>
                <dd className="govuk-summary-list__value">
                  <a href="mailto: example@email.co.uk">John Smith</a>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Primary analyst</dt>
                <dd className="govuk-summary-list__value">
                  <a href="mailto: example@email.co.uk">Ann Evans</a>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Primary analyst</dt>
                <dd className="govuk-summary-list__value">
                  <a href="mailto: example@email.co.uk">Alex Miller</a>
                </dd>
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
                <dt className="govuk-summary-list__key">
                  Publication and release contact
                </dt>
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
              Continue
            </button>
            <div className="govuk-!-margin-top-6">
              <Link to="/prototypes/admin-dashboard">Cancel publication</Link>
            </div>
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
