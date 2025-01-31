import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormGroup, FormTextInput } from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React from 'react';

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
          <Button variant="secondary">Invite new user</Button>
        </div>
      </form>

      <SummaryList>
        <SummaryListItem
          term="Pre-release access"
          actions={<ButtonText>Remove</ButtonText>}
        >
          Test1@example.com
        </SummaryListItem>
        <SummaryListItem
          term="Pre-release access"
          actions={<ButtonText>Remove</ButtonText>}
        >
          Test2@example.com
        </SummaryListItem>
      </SummaryList>
    </>
  );
};

export default PrototypeManagePreRelease;
