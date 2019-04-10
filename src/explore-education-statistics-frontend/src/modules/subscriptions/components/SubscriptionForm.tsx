import * as React from 'react';
import { Field } from './Field';
import { Fields, Form, isEmail, required } from './Form';

class SubscriptionForm extends React.Component {
  public render() {
    const fields: Fields = {
      email: {
        id: 'email',
        label: '',
        type: 'email',
        validation: { rule: isEmail },
        value: 'si.shakes@gmail.com',
      },
      publicationId: {
        id: 'publication-id',
        value: '1234',
      },
    };

    return (
      <Form
        action="http://localhost:7071/api/publication/subscribe"
        fields={fields}
        render={() => (
          <React.Fragment>
            <div className="alert alert-info" role="alert">
              Please enter your email to future releases of this publication.
            </div>
            <Field {...fields.email} />
            <Field {...fields.publicationId} />
          </React.Fragment>
        )}
      />
    );
  }
}

export default SubscriptionForm;
