import WarningMessage from '@common/components/WarningMessage';
import ChangeSection from '@common/modules/data-catalogue/components/ChangeSection';
import { ChangeSet } from '@common/services/types/apiDataSetChanges';
import React from 'react';

interface Props {
  majorChanges: ChangeSet;
  minorChanges: ChangeSet;
  version: string;
}

export default function ApiDataSetChangelog({
  majorChanges,
  minorChanges,
  version,
}: Props) {
  return (
    <>
      {Object.keys(majorChanges).length > 0 && (
        <div data-testid="major-changes">
          <h3>Major changes for version {version}</h3>

          <WarningMessage>
            This version introduces major breaking changes that may be
            incompatible with consumers of the previous version.
          </WarningMessage>

          <p>
            It is recommended to check the list of changes below and make any
            required changes to your API data set queries before upgrading to
            this version.
          </p>

          <p>Note that the following are considered breaking changes:</p>
          <ul>
            <li>options were deleted</li>
            <li>ids were changed</li>
            <li>codes were changed</li>
          </ul>

          <ChangeSection changes={majorChanges} />
        </div>
      )}

      {Object.keys(minorChanges).length > 0 && (
        <div data-testid="minor-changes">
          <h3>Minor changes for version {version}</h3>

          <p>
            This version introduces minor non-breaking changes that are unlikely
            to affect consumers of the previous version. You may wish to check
            the list of changes below before upgrading.
          </p>

          <ChangeSection changes={minorChanges} />
        </div>
      )}
    </>
  );
}
