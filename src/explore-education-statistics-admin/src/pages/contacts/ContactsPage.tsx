import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import React, { useEffect, useState } from 'react';
import contactService, { ContactDetails } from '@admin/services/contactService';

interface Model {
  contacts: ContactDetails[];
}

const ContactsPage = () => {
  const [model, setModel] = useState<Model>();

  useEffect(() => {
    contactService.getContacts().then(contacts => {
      setModel({
        contacts,
      });
    });
  }, []);

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Manage contacts' },
      ]}
    >
      <h1 className="govuk-heading-xl">
        <span className="govuk-caption-xl">Content and data</span>
        Manage contacts
      </h1>

      {model && (
        <table className="govuk-table">
          <thead className="govuk-table__head">
            <tr className="govuk-table__row">
              <th scope="col" className="govuk-table__header">
                Team
              </th>
              <th scope="col" className="govuk-table__header">
                Name
              </th>
              <th scope="col" className="govuk-table__header">
                Email
              </th>
              <th scope="col" className="govuk-table__header">
                Tel
              </th>
            </tr>
          </thead>
          <tbody className="govuk-table__body">
            {model.contacts.map(contact => (
              <tr className="govuk-table__row" key={contact.id}>
                <td className="govuk-table__header">{contact.teamName}</td>
                <td className="govuk-table__cell">{contact.contactName}</td>
                <td className="govuk-table__cell">{contact.teamEmail}</td>
                <td className="govuk-table__cell">{contact.contactTelNo}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <div className="govuk-!-margin-top-6">
        <Link to="/administration/" className="govuk-back-link">
          Back
        </Link>
      </div>
    </Page>
  );
};

export default ContactsPage;
