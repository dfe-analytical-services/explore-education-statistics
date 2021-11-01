import { BasicPublicationDetails } from '@admin/services/publicationService';
import React from 'react';

export interface Props {
  publication: BasicPublicationDetails;
}

const PublicationInviteNewUsersTab = ({ publication }: Props) => {
  return <p>Invite new users tab content</p>;
};

export default PublicationInviteNewUsersTab;
