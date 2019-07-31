import Page from "@admin/components/Page";
import {ContactDetails, IdLabelPair} from "@admin/services/common/types";
import service from '@admin/services/edit-publication/service';
import React, {useEffect, useState} from 'react';
import {RouteComponentProps} from 'react-router';

interface MatchProps {
  topicId: string;
}

const CreatePublicationPage = ({ match }: RouteComponentProps<MatchProps>) => {

  const { topicId } = match.params;

  const [methodologies, setMethodologies] = useState<IdLabelPair[]>();
  const [contacts, setContacts] = useState<ContactDetails[]>();

  useEffect(() => {
    const methodologyPromise = service.getMethodologies();
    const contactsPromise = service.getPublicationAndReleaseContacts();
    Promise.all([methodologyPromise, contactsPromise]).
      then(([methodologiesResult, contactsResult]) => {
        setMethodologies(methodologiesResult);
        setContacts(contactsResult);
      }
    );
  }, []);

  return (
    <Page wide breadcrumbs={[{ name: 'Create new publication' }]}>
      <h1 className="govuk-heading-l">
        Create new publication
      </h1>
      {methodologies && methodologies.map(m => m.label)}
      {contacts && contacts.map(m => m.contactName)}
    </Page>
  );
};

export default CreatePublicationPage;
