import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import EditReleasePageTemplate from '@admin/pages/edit-release/components/EditReleasePageTemplate';
import { Release } from '../../services/publicationService';

interface MatchProps {
  releaseId: string;
}

const EditReleaseContentPage = ({ match }: RouteComponentProps<MatchProps>) => {
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
    <EditReleasePageTemplate
      publicationTitle={publicationTitle}
      releaseId={releaseId}
      previousLink={{
        label: 'view / edit tables',
        linkTo: `/edit-release/${releaseId}/tables`,
      }}
      nextLink={{
        label: 'set publish status',
        linkTo: `/edit-release/${releaseId}/publish-status`,
      }}
    >
      {release && <h2 className="govuk-heading-m">Add / edit content</h2>}
    </EditReleasePageTemplate>
  );
};

export default EditReleaseContentPage;
