import { UserDetails } from '@admin/services/types/user';
import WarningMessage from '@common/components/WarningMessage';
import { format } from 'date-fns';
import React from 'react';

interface Props {
  lockedBy: UserDetails;
  locked?: string;
}

export default function EditableBlockLockedMessage({
  lockedBy,
  locked,
}: Props) {
  const { displayName, email } = lockedBy;

  const lockedAt = locked
    ? ` (last updated ${format(new Date(locked), 'HH:mm')})`
    : '';

  return (
    <WarningMessage testId="editableBlock-lockedMessage">
      {`${displayName} (${email}) is currently editing this block${lockedAt}`}
    </WarningMessage>
  );
}
