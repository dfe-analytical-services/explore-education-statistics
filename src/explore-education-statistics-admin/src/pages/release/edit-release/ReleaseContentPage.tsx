import React from 'react';
import {RouteComponentProps} from 'react-router';
import ReleasePageTemplate from './components/ReleasePageTemplate';

interface MatchProps {
  releaseId: string;
}

const ReleaseContentPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  return (
    <ReleasePageTemplate publicationTitle="TODO" releaseId={releaseId}>
      <h2 className="govuk-heading-m">Add / edit content</h2>
    </ReleasePageTemplate>
  );
};

export default ReleaseContentPage;
