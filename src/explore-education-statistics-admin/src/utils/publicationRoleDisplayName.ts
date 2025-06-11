import {
  PublicationRole,
  publicationRoles,
} from '@admin/services/types/PublicationRole';

export default function publicationRoleDisplayName(
  publicationRole: PublicationRole,
) {
  switch (publicationRole) {
    case 'Owner':
      return 'Owner';
    case 'Allower':
      return 'Approver';
    default:
      throw new Error(
        `Publication role must be one of: ${publicationRoles}. Supplied value: ${publicationRole}`,
      );
  }
}
