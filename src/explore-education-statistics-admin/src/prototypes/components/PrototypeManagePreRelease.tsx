import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React, { useState } from 'react';
import { FormGroup, FormTextInput } from '@common/components/form';

const PrototypeManagePreRelease = () => {
  return (
    <>
      <form id="createMetaForm" className="govuk-!-marin-bottom-9">
        <legend className="govuk-fieldset__legend govuk-fieldset__legend--l">
          <h2 className="govuk-heading-m">Manage pre-release access</h2>
        </legend>
        <FormGroup>
          <FormTextInput
            id="name"
            name="name"
            label="Invite a new user"
            className="govuk-!-width-three-quarters"
          />
        </FormGroup>

        <div className="govuk-!-margin-top-3">
          <button
            className="govuk-button govuk-button--secondary"
            type="submit"
          >
            Invite new user
          </button>
        </div>
      </form>

      <dl className="govuk-summary-list govuk-!-margin-bottom-9">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Pre-release access</dt>
          <dd className="govuk-summary-list__value">Test1@example.com</dd>
          <dd className="govuk-summary-list__actions">
            <a href="#">Remove</a>
          </dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Pre-release access</dt>
          <dd className="govuk-summary-list__value">Test2@example.com</dd>
          <dd className="govuk-summary-list__actions">
            <a href="#">Remove</a>
          </dd>
        </div>
      </dl>
    </>
  );
};

export default PrototypeManagePreRelease;
