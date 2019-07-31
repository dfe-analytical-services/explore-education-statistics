import Page from '@admin/components/Page';
import CreatePublicationForm from '@admin/pages/create-publication/CreatePublicationForm';
import CreatePublicationSummary from '@admin/pages/create-publication/CreatePublicationSummary';
import { ContactDetails, IdTitlePair } from '@admin/services/common/types';
import service from '@admin/services/edit-publication/service';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';

interface MatchProps {
  topicId: string;
}

interface FormValues {
  publicationTitle: string;
  methodologyChoice?: 'existing' | 'new';
  selectedMethodologyId?: string;
  selectedContactId: string;
}

const CreatePublicationPage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const { topicId } = match.params;

  const [methodologies, setMethodologies] = useState<IdTitlePair[]>();
  const [contacts, setContacts] = useState<ContactDetails[]>();
  const [confirmationView, setConfirmationView] = useState(false);
  const [currentValues, setCurrentValues] = useState<FormValues>();

  useEffect(() => {
    const methodologyPromise = service.getMethodologies();
    const contactsPromise = service.getPublicationAndReleaseContacts();
    Promise.all([methodologyPromise, contactsPromise]).then(
      ([methodologiesResult, contactsResult]) => {
        setMethodologies(methodologiesResult);
        setContacts(contactsResult);
      },
    );
  }, []);

  const submitFormHandler = (values: FormValues) => {
    setCurrentValues(values);
    setConfirmationView(true);
  };

  const editHandler = () => {
    setConfirmationView(false);
  };

  const confirmHandler = (values: FormValues) => {
    service
      .createPublication({
        topicId,
        ...values,
      })
      .then(() => history.push('/admin-dashboard'));
  };

  const cancelHandler = () => {
    history.push('/admin-dashboard');
  };

  return (
    <Page
      wide
      breadcrumbs={[
        {
          name: 'Administrator dashboard',
          link: '/admin-dashboard',
        },
        {
          name: 'Create new publication',
        },
      ]}
    >
      <h1 className="govuk-heading-l">Create new publication</h1>
      {contacts &&
        methodologies &&
        (confirmationView && currentValues ? (
          <CreatePublicationSummary
            contacts={contacts}
            methodologies={methodologies}
            values={currentValues}
            onEditHandler={editHandler}
            onConfirmHandler={confirmHandler}
            onCancelHandler={cancelHandler}
          />
        ) : (
          <CreatePublicationForm
            contacts={contacts}
            methodologies={methodologies}
            currentValues={currentValues}
            onSubmitHandler={submitFormHandler}
            onCancelHandler={cancelHandler}
          />
        ))}
    </Page>
  );
};

export default CreatePublicationPage;
