import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import React from 'react';
import { PrototypeNotification } from '../PrototypePublicationSubjects';

interface Props {
  notifications: PrototypeNotification[];
}

const PrototypeNotificationsList = ({ notifications }: Props) => {
  return (
    <table className="govuk-!-margin-top-4 govuk-!-margin-bottom-6">
      <caption className="govuk-!-margin-bottom-3 govuk-table__caption govuk-table__caption--m">
        Notifications
      </caption>
      <thead>
        <tr>
          <th>Notification</th>
          <th>Date</th>
          <th className="govuk-!-text-align-right">Actions</th>
        </tr>
      </thead>
      <tbody>
        {notifications.map((notification, index) => {
          return (
            // eslint-disable-next-line react/no-array-index-key
            <tr key={index}>
              <td>{notification.summary}</td>
              <td>
                <FormattedDate>{new Date()}</FormattedDate>
              </td>
              <td className="govuk-!-text-align-right">
                <ButtonText className="govuk-!-margin-right-3">Edit</ButtonText>
                <ButtonText variant="warning">Remove</ButtonText>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
};
export default PrototypeNotificationsList;
