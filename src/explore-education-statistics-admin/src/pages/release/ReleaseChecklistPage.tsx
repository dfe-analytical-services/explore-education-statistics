import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
import React from 'react';
import ReleaseStatusChecklist from './components/ReleaseStatusChecklist';

export default function ReleaseStatusPage() {
  const { releaseVersion } = useReleaseVersionContext();

  return (
    <>
      <h2>Publishing Checklist</h2>
      <ReleaseStatusChecklist releaseVersion={releaseVersion} />
    </>
  );
}
