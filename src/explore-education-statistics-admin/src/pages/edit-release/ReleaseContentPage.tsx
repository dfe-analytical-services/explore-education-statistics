import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import { Release } from '@admin/services/common/types/types';

interface MatchProps {
  releaseId: string;
}

const ReleaseContentPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [release, setRelease] = useState<Release>();

  const [publicationTitle, setPublicationTitle] = useState('');

  useEffect(() => {
    const selectedRelease = DummyPublicationsData.getReleaseById(releaseId);

    const owningPublication = DummyPublicationsData.getOwningPublicationForRelease(
      selectedRelease,
    );

    setRelease(selectedRelease);

    setPublicationTitle(owningPublication ? owningPublication.title : '');
  }, [releaseId]);

  return (
    <ReleasePageTemplate
      publicationTitle={publicationTitle}
      releaseId={releaseId}
    >
      {release && <h2 className="govuk-heading-m">Add / edit content</h2>}
    </ReleasePageTemplate>
  );
};

export default ReleaseContentPage;
