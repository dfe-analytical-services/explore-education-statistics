import React from 'react';
import Page from '@admin/components/Page';
import RelatedInformation from '@common/components/RelatedInformation';
import Link from '@admin/components/Link';
import { RouteComponentProps } from 'react-router';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';

interface FormValues {
  userEmail: string;
}

const UserInvitePage = ({
  history,
}: RouteComponentProps & ErrorControlProps) => {
  const cancelHandler = () => history.push('/administration/users');

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Users', link: '/administration/users' },
        { name: 'Invite' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">
            <span className="govuk-caption-xl">
              Manage access to the service
            </span>
            Invite a new user
          </h1>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/documentation/" target="blank">
                  Inviting new users{' '}
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      <div className="govuk-!-margin-top-6">
        <Link to="#" onClick={cancelHandler}>
          Cancel
        </Link>
      </div>
    </Page>
  );
};

export default UserInvitePage;
