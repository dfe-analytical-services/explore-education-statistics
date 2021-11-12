import React from 'react';
import { ManageAccessPageRelease } from '@admin/services/releasePermissionService';

export interface Props {
  releases: ManageAccessPageRelease[];
}

const PublicationInviteNewUsersTab = ({ releases }: Props) => {
  return <p>Invite new users tab content</p>;
};

export default PublicationInviteNewUsersTab;
