import React from 'react';
import { RouteComponentProps } from 'react-router';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';

interface MatchProps {
  releaseId: string;
}

const ReleasePublishStatusPage = ({
  match,
}: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  return (
    <ReleasePageTemplate publicationTitle="TODO" releaseId={releaseId}>
      <h2 className="govuk-heading-m">Set publish status</h2>
    </ReleasePageTemplate>
  );
};

export default ReleasePublishStatusPage;
